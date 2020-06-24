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

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, IMethodSymbol method)
		{
			var arguments = invocationOperation.Arguments;
			if (arguments.Length != 1)
				return;

			if (!(arguments[0].Value is IInvocationOperation invocationExpression))
				return;

			var methodSymbol = invocationExpression.TargetMethod;
			if (!RegexIsMatchSymbols.Contains(SymbolDisplay.ToDisplayString(methodSymbol)))
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[MethodName] = method.Name;
			builder[IsStatic] = methodSymbol.IsStatic ? bool.TrueString : bool.FalseString;
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2008_AssertRegexMatchShouldNotUseBoolLiteralCheck,
					invocationOperation.Syntax.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
		}
	}
}
