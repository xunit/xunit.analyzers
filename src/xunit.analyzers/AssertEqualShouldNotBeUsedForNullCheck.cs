using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertEqualShouldNotBeUsedForNullCheck : AssertUsageAnalyzerBase
	{
		public static readonly HashSet<string> EqualMethods = new()
		{
			Constants.Asserts.Equal,
			Constants.Asserts.StrictEqual,
			Constants.Asserts.Same
		};
		public static readonly HashSet<string> NotEqualMethods = new()
		{
			Constants.Asserts.NotEqual,
			Constants.Asserts.NotStrictEqual,
			Constants.Asserts.NotSame
		};

		static readonly string[] targetMethods = EqualMethods.Union(NotEqualMethods).ToArray();

		public AssertEqualShouldNotBeUsedForNullCheck()
			: base(Descriptors.X2003_AssertEqualShouldNotUsedForNullCheck, targetMethods)
		{ }

		protected override void Analyze(
			OperationAnalysisContext context,
			IInvocationOperation invocationOperation,
			IMethodSymbol method)
		{
			var invocation = (InvocationExpressionSyntax)invocationOperation.Syntax;
			var arguments = invocation.ArgumentList.Arguments;
			var literalFirstArgument = arguments.First().Expression as LiteralExpressionSyntax;
			if (!literalFirstArgument?.IsKind(SyntaxKind.NullLiteralExpression) ?? true)
				return;

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[Constants.Properties.MethodName] = method.Name;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2003_AssertEqualShouldNotUsedForNullCheck,
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
