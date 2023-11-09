using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class BooleanAssertsShouldNotBeNegated : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	{
		Constants.Asserts.False,
		Constants.Asserts.True,
	};

	public BooleanAssertsShouldNotBeNegated() :
		base(Descriptors.X2022_BooleanAssertionsShouldNotBeNegated, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		XunitContext xunitContext,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		if (invocationOperation.Arguments.Length < 1)
			return;

		if (invocationOperation.Arguments[0].Value is not IUnaryOperation unaryOperation)
			return;

		if (!unaryOperation.Syntax.IsKind(SyntaxKind.LogicalNotExpression))
			return;

		var suggestedAssertion =
			method.Name == Constants.Asserts.False
				? Constants.Asserts.True
				: Constants.Asserts.False;

		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		builder[Constants.Properties.Replacement] = suggestedAssertion;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2022_BooleanAssertionsShouldNotBeNegated,
				invocationOperation.Syntax.GetLocation(),
				builder.ToImmutable(),
				method.Name,
				suggestedAssertion
			)
		);
	}
}
