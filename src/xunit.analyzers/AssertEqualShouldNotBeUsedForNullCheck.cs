using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertEqualShouldNotBeUsedForNullCheck : AssertUsageAnalyzerBase
{
	static readonly HashSet<string> equalMethods = new()
	{
		Constants.Asserts.Equal,
		Constants.Asserts.Same,
		Constants.Asserts.StrictEqual,
	};
	static readonly HashSet<string> notEqualMethods = new()
	{
		Constants.Asserts.NotEqual,
		Constants.Asserts.NotSame,
		Constants.Asserts.NotStrictEqual,
	};
	static readonly string[] targetMethods = equalMethods.Union(notEqualMethods).ToArray();

	public AssertEqualShouldNotBeUsedForNullCheck()
		: base(Descriptors.X2003_AssertEqualShouldNotUsedForNullCheck, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		if (invocationOperation.Syntax is not InvocationExpressionSyntax invocation)
			return;

		var arguments = invocation.ArgumentList.Arguments;
		var literalFirstArgument = arguments.FirstOrDefault()?.Expression as LiteralExpressionSyntax;
		if (!literalFirstArgument?.IsKind(SyntaxKind.NullLiteralExpression) ?? true)
			return;

		var replacement = GetReplacementMethod(method.Name);
		if (replacement is null)
			return;

		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		builder[Constants.Properties.MethodName] = method.Name;
		builder[Constants.Properties.Replacement] = replacement;

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
				),
				replacement
			)
		);
	}

	static string? GetReplacementMethod(string methodName)
	{
		if (equalMethods.Contains(methodName))
			return Constants.Asserts.Null;
		if (notEqualMethods.Contains(methodName))
			return Constants.Asserts.NotNull;

		return null;
	}
}
