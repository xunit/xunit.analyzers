using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertRegexMatchShouldNotUseBoolLiteralCheck : AssertUsageAnalyzerBase
	{
		internal const string MethodName = "MethodName";
		internal const string IsStatic = "IsStatic";
		internal static readonly HashSet<string> Methods = new HashSet<string>(new[] { "True", "False" });
		internal static readonly HashSet<string> RegexIsMatchSymbols = new HashSet<string>(new[]
		{
			"System.Text.RegularExpressions.Regex.IsMatch(string, string)",
			"System.Text.RegularExpressions.Regex.IsMatch(string)"
		});

		public AssertRegexMatchShouldNotUseBoolLiteralCheck()
			: base(Descriptors.X2008_AssertRegexMatchShouldNotUseBoolLiteralCheck, Methods)
		{ }

		protected override void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
		{
			var arguments = invocation.ArgumentList.Arguments;
			if (arguments.Count != 1)
				return;

			var invocationExpression = arguments.First().Expression as InvocationExpressionSyntax;
			if (invocationExpression == null)
				return;

			var symbolInfo = context.SemanticModel.GetSymbolInfo(invocationExpression);
			if (symbolInfo.Symbol?.Kind != SymbolKind.Method)
				return;

			var methodSymbol = (IMethodSymbol)symbolInfo.Symbol;
			if (!RegexIsMatchSymbols.Contains(SymbolDisplay.ToDisplayString(methodSymbol)))
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[MethodName] = method.Name;
			builder[IsStatic] = methodSymbol.IsStatic ? bool.TrueString : bool.FalseString;
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2008_AssertRegexMatchShouldNotUseBoolLiteralCheck,
					invocation.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
		}
	}
}
