using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck : AssertUsageAnalyzerBase
{
	// Signature without nullable variant
	const string enumerableAnyExtensionMethod = "System.Linq.Enumerable.Any<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)";
	static readonly string[] targetMethods =
	{
		Constants.Asserts.False,
		Constants.Asserts.True,
	};

	public AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck()
		: base(Descriptors.X2012_AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		XunitContext xunitContext,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		var arguments = invocationOperation.Arguments;
		if (arguments.Length != 1)
			return;
		if (arguments[0].Value is not IInvocationOperation invocationExpression)
			return;

		var methodSymbol = invocationExpression.TargetMethod;
		if (SymbolDisplay.ToDisplayString(methodSymbol.OriginalDefinition) != enumerableAnyExtensionMethod)
			return;

		var replacement =
			method.Name == Constants.Asserts.True
				? Constants.Asserts.Contains
				: Constants.Asserts.DoesNotContain;

		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		builder[Constants.Properties.AssertMethodName] = method.Name;
		builder[Constants.Properties.Replacement] = replacement;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2012_AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck,
				invocationOperation.Syntax.GetLocation(),
				builder.ToImmutable(),
				SymbolDisplay.ToDisplayString(
					method,
					SymbolDisplayFormat
						.CSharpShortErrorMessageFormat
						.WithParameterOptions(SymbolDisplayParameterOptions.None)
						.WithGenericsOptions(SymbolDisplayGenericsOptions.None)
				),
				replacement
			)
		);
	}
}
