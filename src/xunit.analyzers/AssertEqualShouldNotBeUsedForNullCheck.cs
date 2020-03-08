using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertEqualShouldNotBeUsedForNullCheck : AssertUsageAnalyzerBase
	{
		internal static string MethodName = "MethodName";
		internal static readonly HashSet<string> EqualMethods = new HashSet<string>(new[] { "Equal", "StrictEqual", "Same" });
		internal static readonly HashSet<string> NotEqualMethods = new HashSet<string>(new[] { "NotEqual", "NotStrictEqual", "NotSame" });

		public AssertEqualShouldNotBeUsedForNullCheck()
			: base(Descriptors.X2003_AssertEqualShouldNotUsedForNullCheck, EqualMethods.Union(NotEqualMethods))
		{ }

		protected override void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
		{
			var arguments = invocation.ArgumentList.Arguments;
			var literalFirstArgument = arguments.First().Expression as LiteralExpressionSyntax;
			if (!literalFirstArgument?.IsKind(SyntaxKind.NullLiteralExpression) ?? true)
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[MethodName] = method.Name;
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2003_AssertEqualShouldNotUsedForNullCheck,
					invocation.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
		}
	}
}
