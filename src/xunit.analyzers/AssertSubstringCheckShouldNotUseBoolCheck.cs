using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, InvocationExpressionSyntax invocation, IMethodSymbol method)
		{
			var arguments = invocation.ArgumentList.Arguments;
			if (arguments.Count != 1)
				return;

			if (!(arguments.First().Expression is InvocationExpressionSyntax invocationExpression))
				return;

			var symbolInfo = context.GetSemanticModel().GetSymbolInfo(invocationExpression);
			if (symbolInfo.Symbol?.Kind != SymbolKind.Method)
				return;

			var methodSymbol = (IMethodSymbol)symbolInfo.Symbol;
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
					invocation.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
		}
	}
}
