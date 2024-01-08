using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertSameShouldNotBeCalledOnValueTypes : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	{
		Constants.Asserts.Same,
		Constants.Asserts.NotSame
	};

	public AssertSameShouldNotBeCalledOnValueTypes()
		: base(Descriptors.X2005_AssertSameShouldNotBeCalledOnValueTypes, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		XunitContext xunitContext,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		Guard.ArgumentNotNull(xunitContext);
		Guard.ArgumentNotNull(invocationOperation);
		Guard.ArgumentNotNull(method);

		if (invocationOperation.Arguments.Length != 2)
			return;

		var firstArgumentType = invocationOperation.Arguments[0].Value.WalkDownImplicitConversions()?.Type;
		var secondArgumentType = invocationOperation.Arguments[1].Value.WalkDownImplicitConversions()?.Type;

		if (firstArgumentType is null && secondArgumentType is null)
			return;

		if (firstArgumentType?.IsReferenceType == true && secondArgumentType?.IsReferenceType == true)
			return;

		var typeToDisplay = firstArgumentType is null || firstArgumentType.IsReferenceType
			? secondArgumentType
			: firstArgumentType;

		if (typeToDisplay is null)
			return;

		var replacement = method.Name switch
		{
			Constants.Asserts.Same => Constants.Asserts.Equal,
			Constants.Asserts.NotSame => Constants.Asserts.NotEqual,
			_ => null,
		};

		if (replacement is null)
			return;

		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		builder[Constants.Properties.Replacement] = replacement;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2005_AssertSameShouldNotBeCalledOnValueTypes,
				invocationOperation.Syntax.GetLocation(),
				builder.ToImmutable(),
				SymbolDisplay.ToDisplayString(
					method,
					SymbolDisplayFormat
						.CSharpShortErrorMessageFormat
						.WithParameterOptions(SymbolDisplayParameterOptions.None)
				),
				SymbolDisplay.ToDisplayString(
					typeToDisplay,
					SymbolDisplayFormat
						.CSharpShortErrorMessageFormat
						.WithParameterOptions(SymbolDisplayParameterOptions.None)
				),
				replacement
			)
		);
	}
}
