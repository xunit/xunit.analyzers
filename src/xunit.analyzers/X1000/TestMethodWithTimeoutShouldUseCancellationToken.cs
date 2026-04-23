using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestMethodWithTimeoutShouldUseCancellationToken() :
	XunitV3DiagnosticAnalyzer(Descriptors.X1069_TestMethodWithTimeoutShouldUseCancellationToken)
{
	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var factAndTheoryAttributeTypes = xunitContext.Core.FactAndTheoryAttributeTypes;
		if (factAndTheoryAttributeTypes.Count == 0)
			return;

		var iTestContextType = TypeSymbolFactory.ITestContext_V3(context.Compilation);
		if (iTestContextType is null)
			return;

		var cancellationTokenProperty =
			iTestContextType
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

			var timeoutAttribute = TryGetTimeoutAttribute(method, factAndTheoryAttributeTypes);
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
					Descriptors.X1069_TestMethodWithTimeoutShouldUseCancellationToken,
					location,
					method.Name
				)
			);
		});
	}

	static AttributeData? TryGetTimeoutAttribute(
		IMethodSymbol method,
		ImmutableHashSet<INamedTypeSymbol> factAndTheoryAttributeTypes)
	{
		foreach (var attribute in method.GetAttributes())
			for (var current = attribute.AttributeClass; current is not null; current = current.BaseType)
				if (factAndTheoryAttributeTypes.Contains(current))
					foreach (var named in attribute.NamedArguments)
						if (named.Key == "Timeout" && named.Value.Value is int timeout && timeout > 0)
							return attribute;

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
