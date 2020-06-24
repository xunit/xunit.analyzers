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
		public const string AssertMethodName = "AssertMethodName";
		public const string SubstringMethodName = "SubstringMethodName";

		private static readonly HashSet<string> BooleanMethods = new HashSet<string>(new[] { "True", "False" });
		private static readonly HashSet<string> SubstringMethods = new HashSet<string>(new[]
		{
			"string.Contains(string)",
			"string.StartsWith(string)",
			"string.StartsWith(string, System.StringComparison)",
			"string.EndsWith(string)",
			"string.EndsWith(string, System.StringComparison)"
		});

		public AssertSubstringCheckShouldNotUseBoolCheck()
			: base(Descriptors.X2009_AssertSubstringCheckShouldNotUseBoolCheck, BooleanMethods)
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, IMethodSymbol method)
		{
			var arguments = invocationOperation.Arguments;
			if (arguments.Length != 1)
				return;

			if (!(arguments[0].Value is IInvocationOperation invocationExpression))
				return;

			var methodSymbol = invocationExpression.TargetMethod;
			if (!SubstringMethods.Contains(SymbolDisplay.ToDisplayString(methodSymbol)))
				return;

			if (methodSymbol.Name != "Contains" && method.Name == "False")
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[AssertMethodName] = method.Name;
			builder[SubstringMethodName] = methodSymbol.Name;
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2009_AssertSubstringCheckShouldNotUseBoolCheck,
					invocationOperation.Syntax.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
		}
	}
}
