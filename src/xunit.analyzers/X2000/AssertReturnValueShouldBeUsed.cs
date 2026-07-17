using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertReturnValueShouldBeUsed : AssertUsageAnalyzerBase
{
	const string elementAtMethod = "ElementAt";
	const string firstMethod = "First";

	static readonly string[] targetMethods =
	[
		Constants.Asserts.IsAssignableFrom,
		Constants.Asserts.IsType,
		Constants.Asserts.Single,
	];

	public AssertReturnValueShouldBeUsed() :
		base(Descriptors.X2033_AssertReturnValueShouldBeUsed, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		XunitContext xunitContext,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		Guard.ArgumentNotNull(xunitContext);
		Guard.ArgumentNotNull(invocationOperation);
		Guard.ArgumentNotNull(method);

		// Only interested in assertions whose return value is thrown away
		if (method.ReturnsVoid || invocationOperation.Parent is not IExpressionStatementOperation)
			return;

		if (!TryGetValueExpression(invocationOperation, method, out var valueExpression, out var typeArgument) || valueExpression is null)
			return;

		if (invocationOperation.Syntax.FirstAncestorOrSelf<ExpressionStatementSyntax>() is not { Parent: BlockSyntax block } assertStatement)
			return;

		var rootIdentifier = GetRootIdentifier(valueExpression);

		foreach (var statement in block.Statements.Skip(block.Statements.IndexOf(assertStatement) + 1))
		{
			var rederivation = FindRederivation(statement, method, valueExpression, typeArgument, invocationOperation.SemanticModel);
			if (rederivation is not null)
			{
				var properties = ImmutableDictionary.CreateBuilder<string, string?>();
				properties[Constants.Properties.AssertMethodName] = method.Name;
				properties[Constants.Properties.RederivationSpanStart] = rederivation.SpanStart.ToString(CultureInfo.InvariantCulture);
				properties[Constants.Properties.RederivationSpanLength] = rederivation.Span.Length.ToString(CultureInfo.InvariantCulture);

				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X2033_AssertReturnValueShouldBeUsed,
						invocationOperation.Syntax.GetLocation(),
						properties.ToImmutable(),
						method.Name,
						rederivation.ToString()
					)
				);
				return;
			}

			// Once the value may have changed, later matches are no longer re-derivations
			if (rootIdentifier is not null && ReassignsIdentifier(statement, rootIdentifier))
				return;
		}
	}

	static bool TryGetValueExpression(
		IInvocationOperation invocation,
		IMethodSymbol method,
		out ExpressionSyntax? valueExpression,
		out ITypeSymbol? typeArgument)
	{
		valueExpression = null;
		typeArgument = null;

		// For Single, only the single-argument overload applies; the expected-value and
		// predicate overloads assert something else than "the one and only item". For the
		// type assertions, only the generic overloads carry the type being asserted.
		if (method.Name == Constants.Asserts.Single)
		{
			if (method.Parameters.Length != 1)
				return false;
		}
		else if (method is { IsGenericMethod: true, TypeArguments.Length: 1 })
			typeArgument = method.TypeArguments[0];
		else
			return false;

		var argument = invocation.Arguments.FirstOrDefault(a => a.Parameter?.Ordinal == 0);
		if (argument is null)
			return false;

		var value = argument.Value;
		while (value is IConversionOperation { IsImplicit: true } conversion)
			value = conversion.Operand;

		valueExpression = value.Syntax as ExpressionSyntax;
		return valueExpression is not null;
	}

	static ExpressionSyntax? FindRederivation(
		StatementSyntax statement,
		IMethodSymbol method,
		ExpressionSyntax valueExpression,
		ITypeSymbol? typeArgument,
		SemanticModel? semanticModel)
	{
		// Lambdas and local functions run at some other time, so they are not re-derivations
		var candidates =
			statement.DescendantNodesAndSelf(node =>
				node is not AnonymousFunctionExpressionSyntax && node is not LocalFunctionStatementSyntax
			);

		return candidates
			.Select(candidate =>
				method.Name == Constants.Asserts.Single
					? GetSingleItemRederivation(candidate, valueExpression)
					: GetTypedRederivation(candidate, valueExpression, typeArgument, semanticModel)
			)
			.FirstOrDefault(match => match is not null);
	}

	static ExpressionSyntax? GetSingleItemRederivation(
		SyntaxNode candidate,
		ExpressionSyntax valueExpression) =>
			candidate switch
			{
				InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocation
					when RederivesSingleItem(memberAccess.Name.Identifier.ValueText, invocation.ArgumentList.Arguments)
						&& AreEquivalent(valueExpression, memberAccess.Expression) =>
							invocation,
				ElementAccessExpressionSyntax elementAccess
					when IsZeroIndexer(elementAccess.ArgumentList.Arguments) && AreEquivalent(valueExpression, elementAccess.Expression) =>
						elementAccess,
				_ => null,
			};

	static bool RederivesSingleItem(
		string methodName,
		SeparatedSyntaxList<ArgumentSyntax> arguments) =>
			methodName switch
			{
				Constants.Asserts.Single or firstMethod => arguments.Count == 0,
				elementAtMethod => IsZeroIndexer(arguments),
				_ => false,
			};

	static ExpressionSyntax? GetTypedRederivation(
		SyntaxNode candidate,
		ExpressionSyntax valueExpression,
		ITypeSymbol? typeArgument,
		SemanticModel? semanticModel)
	{
		if (typeArgument is null || semanticModel is null)
			return null;

		var (operand, typeSyntax) = candidate switch
		{
			CastExpressionSyntax cast =>
				(cast.Expression, cast.Type),
			BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AsExpression, Right: TypeSyntax asType } binary =>
				(binary.Left, asType),
			_ =>
				(default, default),
		};

		if (operand is null || typeSyntax is null || !AreEquivalent(valueExpression, operand))
			return null;

		return SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeInfo(typeSyntax).Type, typeArgument)
			? (ExpressionSyntax)candidate
			: null;
	}

	static IdentifierNameSyntax? GetRootIdentifier(ExpressionSyntax expression)
	{
		while (true)
			switch (expression)
			{
				case IdentifierNameSyntax identifier:
					return identifier;
				case MemberAccessExpressionSyntax memberAccess:
					expression = memberAccess.Expression;
					break;
				case ElementAccessExpressionSyntax elementAccess:
					expression = elementAccess.Expression;
					break;
				case InvocationExpressionSyntax invocation:
					expression = invocation.Expression;
					break;
				case ParenthesizedExpressionSyntax parenthesized:
					expression = parenthesized.Expression;
					break;
				default:
					return null;
			}
	}

	static bool ReassignsIdentifier(
		StatementSyntax statement,
		IdentifierNameSyntax rootIdentifier)
	{
		var identifier = rootIdentifier.Identifier.ValueText;

		return statement.DescendantNodesAndSelf().Any(node => node switch
		{
			AssignmentExpressionSyntax assignment =>
				IsIdentifier(assignment.Left, identifier),
			PrefixUnaryExpressionSyntax prefix when prefix.IsKind(SyntaxKind.PreIncrementExpression) || prefix.IsKind(SyntaxKind.PreDecrementExpression) =>
				IsIdentifier(prefix.Operand, identifier),
			PostfixUnaryExpressionSyntax postfix when postfix.IsKind(SyntaxKind.PostIncrementExpression) || postfix.IsKind(SyntaxKind.PostDecrementExpression) =>
				IsIdentifier(postfix.Operand, identifier),
			ArgumentSyntax argument =>
				!argument.RefKindKeyword.IsKind(SyntaxKind.None) && IsIdentifier(argument.Expression, identifier),
			_ =>
				false,
		});
	}

	static bool AreEquivalent(
		ExpressionSyntax valueExpression,
		ExpressionSyntax candidate) =>
			SyntaxFactory.AreEquivalent(valueExpression, candidate, topLevel: false);

	static bool IsIdentifier(
		ExpressionSyntax expression,
		string identifier) =>
			expression is IdentifierNameSyntax identifierName && identifierName.Identifier.ValueText == identifier;

	static bool IsZeroIndexer(SeparatedSyntaxList<ArgumentSyntax> arguments) =>
		arguments.Count == 1 && IsZeroLiteral(arguments[0].Expression);

	static bool IsZeroLiteral(ExpressionSyntax expression) =>
		expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NumericLiteralExpression) && literal.Token.ValueText == "0";
}
