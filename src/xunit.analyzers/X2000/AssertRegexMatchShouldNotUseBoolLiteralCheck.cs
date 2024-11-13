using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertRegexMatchShouldNotUseBoolLiteralCheck : AssertUsageAnalyzerBase
{
	static readonly HashSet<string> regexIsMatchSymbols =
	[
		// Signatures without nullable variants
		"System.Text.RegularExpressions.Regex.IsMatch(string)",
		"System.Text.RegularExpressions.Regex.IsMatch(string, string)",
	];
	static readonly string[] targetMethods =
	[
		Constants.Asserts.True,
		Constants.Asserts.False
	];

	public AssertRegexMatchShouldNotUseBoolLiteralCheck()
		: base(Descriptors.X2008_AssertRegexMatchShouldNotUseBoolLiteralCheck, targetMethods)
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

		var arguments = invocationOperation.Arguments;
		if (arguments.Length != 1)
			return;

		if (arguments[0].Value is not IInvocationOperation invocationExpression)
			return;

		var methodSymbol = invocationExpression.TargetMethod;
		if (!regexIsMatchSymbols.Contains(SymbolDisplay.ToDisplayString(methodSymbol)))
			return;

		var replacement =
			method.Name == Constants.Asserts.True
				? Constants.Asserts.Matches
				: Constants.Asserts.DoesNotMatch;

		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		builder[Constants.Properties.MethodName] = method.Name;
		builder[Constants.Properties.IsStatic] = methodSymbol.IsStatic ? bool.TrueString : bool.FalseString;
		builder[Constants.Properties.Replacement] = replacement;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2008_AssertRegexMatchShouldNotUseBoolLiteralCheck,
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
