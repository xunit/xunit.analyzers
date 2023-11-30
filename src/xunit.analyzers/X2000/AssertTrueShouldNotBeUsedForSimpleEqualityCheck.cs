using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertTrueShouldNotBeUsedForSimpleEqualityCheck : AssertUsageAnalyzerBase
{
	public AssertTrueShouldNotBeUsedForSimpleEqualityCheck()
		: base(Descriptors.X2024_AssertTrueShouldNotBeUsedForSimpleEqualityCheck, new[] { Constants.Asserts.True, Constants.Asserts.False })
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		XunitContext xunitContext,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		if (invocationOperation.Syntax is not InvocationExpressionSyntax invocation)
			return;

		var arguments = invocation.ArgumentList.Arguments;
		if (arguments.FirstOrDefault()?.Expression is not BinaryExpressionSyntax binaryArgument)
			return;

		var trueMethod = method.Name == Constants.Asserts.True;
		var leftKind = LiteralReferenceKind(binaryArgument.Left, context.Operation.SemanticModel);
		var rightKind = LiteralReferenceKind(binaryArgument.Right, context.Operation.SemanticModel);
		var literalKind = leftKind ?? rightKind;
		if (literalKind is null)
			return;
		
		bool isEqualsOperator;
		switch (binaryArgument.Kind())
		{
			case SyntaxKind.EqualsExpression:
				isEqualsOperator = true;
				break;

			case SyntaxKind.NotEqualsExpression:
				isEqualsOperator = false;
				break;

			default:
				return;
		}

		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		builder[Constants.Properties.MethodName] = method.Name;
		switch (literalKind)
		{
			case SyntaxKind.TrueLiteralExpression:
			case SyntaxKind.FalseLiteralExpression:
				ReportShouldSimplifyBooleanOperation(context, invocationOperation, builder.ToImmutable(), method.Name);
				break;

			case SyntaxKind.NullLiteralExpression:
				var nullReplacement = trueMethod == isEqualsOperator ? Constants.Asserts.Null : Constants.Asserts.NotNull;
				builder[Constants.Properties.Replacement] = nullReplacement;
				ReportShouldReplaceBooleanOperationWithEquality(context, invocationOperation, builder.ToImmutable(), method.Name, nullReplacement);
				break;

			default:
				var equalsReplacement = trueMethod == isEqualsOperator ? Constants.Asserts.Equal : Constants.Asserts.NotEqual;
				builder[Constants.Properties.Replacement] = equalsReplacement;
				ReportShouldReplaceBooleanOperationWithEquality(context, invocationOperation, builder.ToImmutable(), method.Name, equalsReplacement);
				break;
		}
	}

	static void ReportShouldReplaceBooleanOperationWithEquality(
		OperationAnalysisContext context,
		IInvocationOperation invocationOperation,
		ImmutableDictionary<string, string?> properties,
		string currentMethodName,
		string replacement)
	{
		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2024_AssertTrueShouldNotBeUsedForSimpleEqualityCheck,
				invocationOperation.Syntax.GetLocation(),
				properties,
				currentMethodName,
				replacement
			));
	}

	static void ReportShouldSimplifyBooleanOperation(
		OperationAnalysisContext context,
		IInvocationOperation invocationOperation,
		ImmutableDictionary<string, string?> properties,
		string currentMethodName)
	{
		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2025_AssertTrueExpressionCanBeSimplified,
				invocationOperation.Syntax.GetLocation(),
				properties,
				currentMethodName
			));
	}

	static string GetReplacementMethod(SyntaxKind kind, bool isTrueMethod, bool isEqualsOperator)
	{
		if (kind == SyntaxKind.NullLiteralExpression)
			return isTrueMethod == isEqualsOperator ? Constants.Asserts.Null : Constants.Asserts.NotNull;
		return isTrueMethod == isEqualsOperator ? Constants.Asserts.Equal : Constants.Asserts.NotEqual;
	}

	static SyntaxKind? LiteralReferenceKind(ExpressionSyntax expression, SemanticModel? semanticModel)
	{
		var kind = expression.Kind();
		if (expression is LiteralExpressionSyntax)
			return kind;

		if (expression.Kind() != SyntaxKind.SimpleMemberAccessExpression)
			return null;

		var left = ((MemberAccessExpressionSyntax)expression).Expression;
		if (left.Kind() != SyntaxKind.IdentifierName)
			return null;

		var type = semanticModel?.GetTypeInfo(expression).Type;
		if (type is not INamedTypeSymbol namedType)
			return null;

		return namedType.EnumUnderlyingType is not null
			? kind
			: null;
	}
}
