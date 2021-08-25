using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertRegexMatchShouldNotUseBoolLiteralCheck : AssertUsageAnalyzerBase
	{
		static readonly HashSet<string> regexIsMatchSymbols = new()
		{
			"System.Text.RegularExpressions.Regex.IsMatch(string, string)",
			"System.Text.RegularExpressions.Regex.IsMatch(string)"
		};
		static readonly string[] targetMethods =
		{
			Constants.Asserts.True,
			Constants.Asserts.False
		};

		public AssertRegexMatchShouldNotUseBoolLiteralCheck()
			: base(Descriptors.X2008_AssertRegexMatchShouldNotUseBoolLiteralCheck, targetMethods)
		{ }

		protected override void Analyze(
			OperationAnalysisContext context,
			IInvocationOperation invocationOperation,
			IMethodSymbol method)
		{
			var arguments = invocationOperation.Arguments;
			if (arguments.Length != 1)
				return;

			if (arguments[0].Value is not IInvocationOperation invocationExpression)
				return;

			var methodSymbol = invocationExpression.TargetMethod;
			if (!regexIsMatchSymbols.Contains(SymbolDisplay.ToDisplayString(methodSymbol)))
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[Constants.Properties.MethodName] = method.Name;
			builder[Constants.Properties.IsStatic] = methodSymbol.IsStatic ? bool.TrueString : bool.FalseString;

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
					)
				)
			);
		}
	}
}
