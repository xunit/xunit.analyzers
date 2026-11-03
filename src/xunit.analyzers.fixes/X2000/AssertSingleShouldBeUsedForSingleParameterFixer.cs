using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class AssertSingleShouldBeUsedForSingleParameterFixer : XunitCodeFixProvider
{
	const string DefaultParameterName = "item";
	public const string Key_UseSingleMethod = "xUnit2023_UseSingleMethod";

	public AssertSingleShouldBeUsedForSingleParameterFixer() :
		base(Descriptors.X2023_AssertSingleShouldBeUsedForSingleParameter.Id)
	{ }

	static string GetSafeVariableName(
		string targetParameterName,
		ImmutableHashSet<string> localSymbols)
	{
		var idx = 2;
		var result = targetParameterName;

		while (localSymbols.Contains(result))
			result = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", targetParameterName, idx++);

		return result;
	}

	static IEnumerable<SyntaxNode> GetLambdaStatements(
		CSharpSyntaxNode lambdaBody,
		bool needAwait)
	{
		if (lambdaBody is ThrowExpressionSyntax throwExpression)
			yield return ThrowStatement(throwExpression.Expression.NormalizeWhitespace()).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
		else if (lambdaBody is ExpressionSyntax expression)
		{
			// The simplifier removes the parentheses unless they're needed for precedence
			if (needAwait)
				expression = AwaitExpression(ParenthesizedExpression(expression));

			yield return ExpressionStatement(expression.NormalizeWhitespace()).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
		}
		else if (lambdaBody is BlockSyntax block)
			foreach (var statement in block.Statements)
				yield return statement.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
	}

	static SyntaxNode GetMethodInvocation(
		ExpressionSyntax methodExpression,
		string parameterName,
		bool needAwait)
	{
		ExpressionSyntax invocation = InvocationExpression(
			methodExpression,
			ArgumentList(SingletonSeparatedList(Argument(IdentifierName(parameterName))))
		);

		if (needAwait)
			invocation = AwaitExpression(invocation);

		return ExpressionStatement(invocation);
	}

	static LocalDeclarationStatementSyntax OneItemVariableStatement(
		string parameterName,
		TypeSyntax? parameterType,
		InvocationExpressionSyntax replacementNode)
	{
		var equalsToReplacementNode = EqualsValueClause(replacementNode.WithoutTrivia());

		var oneItemVariableDeclaration = VariableDeclaration(
			parameterType ?? ParseTypeName("var"),
			SingletonSeparatedList(
				VariableDeclarator(Identifier(parameterName))
					.WithInitializer(equalsToReplacementNode)
			)
		).NormalizeWhitespace();

		return LocalDeclarationStatement(oneItemVariableDeclaration);
	}

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		if (context.Diagnostics.FirstOrDefault() is not Diagnostic diagnostic)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.AssertMethodName, out var assertMethodName) || assertMethodName is null)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement) || replacement is null)
			return;

		// We want to remove the 'await' in front of 'Assert.CollectionAsync' since 'Assert.Single' doesn't need it
		var nodeToReplace = invocation.Parent;
		if (assertMethodName == Constants.Asserts.CollectionAsync && nodeToReplace is AwaitExpressionSyntax)
			nodeToReplace = nodeToReplace.Parent;

		// Can't replace something that's not a standlone expression
		if (nodeToReplace is not ExpressionStatementSyntax)
			return;

		var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return;

		// Only offer the fix when the inspector can be kept, otherwise the assertion would be lost
		var statements = GetReplacementStatements(semanticModel, invocation, nodeToReplace, assertMethodName, replacement, context.CancellationToken);
		if (statements is null)
			return;

		context.RegisterCodeFix(
			XunitCodeAction.Create(
				ct => UseSingleMethod(context.Document, nodeToReplace, statements, ct),
				Key_UseSingleMethod,
				"Use Assert.{0}", replacement
			),
			context.Diagnostics
		);
	}

	static List<SyntaxNode>? GetReplacementStatements(
		SemanticModel semanticModel,
		InvocationExpressionSyntax invocation,
		SyntaxNode nodeToReplace,
		string assertMethodName,
		string replacementMethod,
		CancellationToken cancellationToken)
	{
		if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
			return null;

		var needAwait = assertMethodName == Constants.Asserts.CollectionAsync;
		var inspector = invocation.ArgumentList.Arguments[1].Expression;
		var startLocation = invocation.GetLocation().SourceSpan.Start;
		var localSymbols = semanticModel.LookupSymbols(startLocation).OfType<ILocalSymbol>().Select(s => s.Name).ToImmutableHashSet();
		var replacementNode =
			invocation
				.WithArgumentList(ArgumentList(SeparatedList([Argument(invocation.ArgumentList.Arguments[0].Expression)])))
				.WithExpression(memberAccess.WithName(IdentifierName(replacementMethod)));

		if (inspector is LambdaExpressionSyntax lambdaExpression)
		{
			var parameter = lambdaExpression switch
			{
				SimpleLambdaExpressionSyntax simpleLambda => simpleLambda.Parameter,
				ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 1 } parenthesizedLambda => parenthesizedLambda.ParameterList.Parameters[0],
				_ => null,
			};
			if (parameter is null)
				return null;

			// Inlining a 'return' would return from the test method rather than the inspector
			if (lambdaExpression.Block?.DescendantNodes(n => n is not (AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax)).OfType<ReturnStatementSyntax>().Any() == true)
				return null;

			var body = lambdaExpression.Body;
			var originalParameterName = parameter.Identifier.Text;
			var parameterName = GetSafeVariableName(originalParameterName, localSymbols);

			if (parameterName != originalParameterName)
			{
				var tokens =
					body
						.DescendantTokens()
						.Where(t => t.IsKind(SyntaxKind.IdentifierToken) && t.Text == originalParameterName)
						.ToArray();

				body = body.ReplaceTokens(tokens, (t1, t2) => Identifier(t2.LeadingTrivia, parameterName, t2.TrailingTrivia));
			}

			// A non-async inspector for 'Assert.CollectionAsync' returns its task, which must now be awaited
			return [
				OneItemVariableStatement(parameterName, parameter.Type, replacementNode).WithTriviaFrom(nodeToReplace),
				.. GetLambdaStatements(body, needAwait && !lambdaExpression.Modifiers.Any(SyntaxKind.AsyncKeyword)),
			];
		}

		if (inspector is SimpleNameSyntax or MemberAccessExpressionSyntax
			&& (semanticModel.GetSymbolInfo(inspector, cancellationToken).Symbol is IMethodSymbol || semanticModel.GetTypeInfo(inspector, cancellationToken).Type?.TypeKind == TypeKind.Delegate))
		{
			var parameterName = GetSafeVariableName(DefaultParameterName, localSymbols);

			return [
				OneItemVariableStatement(parameterName, null, replacementNode).WithTriviaFrom(nodeToReplace),
				GetMethodInvocation(inspector.WithoutTrivia(), parameterName, needAwait),
			];
		}

		return null;
	}

	static async Task<Document> UseSingleMethod(
		Document document,
		SyntaxNode nodeToReplace,
		List<SyntaxNode> statements,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		editor.InsertBefore(nodeToReplace, statements);
		editor.RemoveNode(nodeToReplace);

		return editor.GetChangedDocument();
	}
}
