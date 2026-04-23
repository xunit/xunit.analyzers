using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestMethodWithTimeoutShouldUseCancellationToken() :
	XunitV3DiagnosticAnalyzer(Descriptors.X1054_TestMethodWithTimeoutShouldUseCancellationToken)
{
	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var factAttributeType = xunitContext.Core.FactAttributeType;
		if (factAttributeType is null)
			return;

		var testContextType = TypeSymbolFactory.TestContext_V3(context.Compilation);
		if (testContextType is null)
			return;

		var cancellationTokenProperty = testContextType
			.GetMembers("CancellationToken")
			.OfType<IPropertySymbol>()
			.FirstOrDefault();
		if (cancellationTokenProperty is null)
			return;

		context.RegisterOperationBlockAction(ctx =>
		{
			if (ctx.OwningSymbol is not IMethodSymbol method)
				return;

			if (ctx.OperationBlocks.Length == 0)
				return;

			var timeoutAttribute = TryGetTimeoutAttribute(method, factAttributeType);
			if (timeoutAttribute is null)
				return;

			foreach (var block in ctx.OperationBlocks)
				foreach (var descendant in block.DescendantsAndSelf())
					if (descendant is IPropertyReferenceOperation propertyReference
							&& SymbolEqualityComparer.Default.Equals(propertyReference.Property, cancellationTokenProperty))
						return;

			var location = GetTimeoutLocation(timeoutAttribute) ?? method.Locations.FirstOrDefault();
			if (location is null)
				return;

			ctx.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1054_TestMethodWithTimeoutShouldUseCancellationToken,
					location,
					method.Name
				)
			);
		});
	}

	static AttributeData? TryGetTimeoutAttribute(
		IMethodSymbol method,
		INamedTypeSymbol factAttributeType)
	{
		foreach (var attribute in method.GetAttributes())
		{
			if (attribute.AttributeClass is null)
				continue;

			var isFactLike = false;
			for (var current = attribute.AttributeClass; current is not null; current = current.BaseType)
				if (SymbolEqualityComparer.Default.Equals(current, factAttributeType))
				{
					isFactLike = true;
					break;
				}

			if (!isFactLike)
				continue;

			foreach (var named in attribute.NamedArguments)
				if (named.Key == "Timeout" && named.Value.Value is int timeout && timeout > 0)
					return attribute;
		}

		return null;
	}

	static Location? GetTimeoutLocation(AttributeData attribute)
	{
		if (attribute.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax attributeSyntax)
			return null;

		if (attributeSyntax.ArgumentList is { } argumentList)
			foreach (var argument in argumentList.Arguments)
				if (argument.NameEquals?.Name.Identifier.ValueText == "Timeout")
					return argument.GetLocation();

		return attributeSyntax.GetLocation();
	}
}
