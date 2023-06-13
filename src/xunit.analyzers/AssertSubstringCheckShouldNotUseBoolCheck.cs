using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertSubstringCheckShouldNotUseBoolCheck : AssertUsageAnalyzerBase
	{
		private static readonly HashSet<string> substringMethods = new()
		{
			// Signatures without nullable variants
			"string.Contains(string)",
			"string.StartsWith(string)",
			"string.StartsWith(string, System.StringComparison)",
			"string.EndsWith(string)",
			"string.EndsWith(string, System.StringComparison)"
		};
		static readonly string[] targetMethods =
		{
			Constants.Asserts.True,
			Constants.Asserts.False
		};

		public AssertSubstringCheckShouldNotUseBoolCheck()
			: base(Descriptors.X2009_AssertSubstringCheckShouldNotUseBoolCheck, targetMethods)
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
			if (!substringMethods.Contains(SymbolDisplay.ToDisplayString(methodSymbol)))
				return;

			if (methodSymbol.Name != Constants.Asserts.Contains && method.Name == Constants.Asserts.False)
				return;

			var replacement = GetReplacementMethodName(method.Name, methodSymbol.Name);

			var builder = ImmutableDictionary.CreateBuilder<string, string?>();
			builder[Constants.Properties.AssertMethodName] = method.Name;
			builder[Constants.Properties.SubstringMethodName] = methodSymbol.Name;
			builder[Constants.Properties.Replacement] = replacement;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2009_AssertSubstringCheckShouldNotUseBoolCheck,
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
		static string GetReplacementMethodName(
			string assertMethodName,
			string substringMethodName)
		{
			if (substringMethodName == nameof(string.Contains))
				return assertMethodName == Constants.Asserts.True ? Constants.Asserts.Contains : Constants.Asserts.DoesNotContain;

			return substringMethodName;
		}
	}
}
