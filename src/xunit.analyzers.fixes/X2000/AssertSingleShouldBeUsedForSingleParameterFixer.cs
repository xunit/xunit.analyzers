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

	static ImmutableHashSet<string> GetLocalSymbols(
		SemanticModel semanticModel,
		InvocationExpressionSyntax invocation,
		SyntaxNode nodeToReplace,
		CancellationToken cancellationToken)
	{
		var startLocation = invocation.GetLocation().SourceSpan.Start;
		var symbols =
			semanticModel
				.LookupSymbols(startLocation)
				.Where(s => s is ILocalSymbol or IParameterSymbol)
				.Select(s => s.Name)
				.ToImmutableHashSet()
				.ToBuilder();

		if (nodeToReplace.Parent is SyntaxNode enclosingScope)
		{
			var nestedLocalNames =
				enclosingScope
					.DescendantNodes(n => n is not (AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax))
					.Select(n => semanticModel.GetDeclaredSymbol(n, cancellationToken))
					.OfType<ILocalSymbol>()
					.Select(s => s.Name);

			foreach (var name in nestedLocalNames)
				symbols.Add(name);
		}

		return symbols.ToImmutable();
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

		if (context.Diagnostics.FirstOrDefault() is not Diagnostic diagnostic)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.AssertMethodName, out var assertMethodName) || assertMethodName is null)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement) || replacement is null)
			return;

		var invocation = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
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
		if (invocation.ArgumentList.Arguments.Count != 2)
			return null;

		ExpressionSyntax? replacementExpression = invocation.Expression switch
		{
			MemberAccessExpressionSyntax { Name: GenericNameSyntax genericName } memberAccess =>
				memberAccess.WithName(genericName.WithIdentifier(Identifier(replacementMethod))),
			MemberAccessExpressionSyntax memberAccess =>
				memberAccess.WithName(IdentifierName(replacementMethod)),
			GenericNameSyntax genericName =>
				genericName.WithIdentifier(Identifier(replacementMethod)),
			IdentifierNameSyntax =>
				IdentifierName(replacementMethod),
			_ => null,
		};
		if (replacementExpression is null)
			return null;

		var firstArgument = invocation.ArgumentList.Arguments[0];
		var secondArgument = invocation.ArgumentList.Arguments[1];
		var (collectionExpression, inspector) =
			firstArgument.NameColon?.Name.Identifier.Text == "elementInspectors" || secondArgument.NameColon?.Name.Identifier.Text == "collection"
				? (secondArgument.Expression, firstArgument.Expression)
				: (firstArgument.Expression, secondArgument.Expression);

		var needAwait = assertMethodName == Constants.Asserts.CollectionAsync;
		var localSymbols = GetLocalSymbols(semanticModel, invocation, nodeToReplace, cancellationToken);
		var replacementNode =
			invocation
				.WithArgumentList(ArgumentList(SeparatedList([Argument(collectionExpression)])))
				.WithExpression(replacementExpression);

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
				var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter, cancellationToken);
				var tokens =
					body
						.DescendantTokens()
						.Where(t =>
							t.IsKind(SyntaxKind.IdentifierToken)
							&& t.Text == originalParameterName
							&& t.Parent is not null
							&& SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(t.Parent, cancellationToken).Symbol, parameterSymbol)
						)
						.ToArray();

				body = body.ReplaceTokens(tokens, (t1, t2) => Identifier(t2.LeadingTrivia, parameterName, t2.TrailingTrivia));
			}

			// A non-async inspector for 'Assert.CollectionAsync' returns its task, which must now be awaited
			return [
				OneItemVariableStatement(parameterName, parameter.Type, replacementNode).WithTriviaFrom(nodeToReplace),
				.. GetLambdaStatements(body, needAwait && !lambdaExpression.Modifiers.Any(SyntaxKind.AsyncKeyword)),
			];
		}

		if ((inspector is SimpleNameSyntax or MemberAccessExpressionSyntax && semanticModel.GetSymbolInfo(inspector, cancellationToken).Symbol is IMethodSymbol)
			|| (inspector is SimpleNameSyntax or MemberAccessExpressionSyntax or InvocationExpressionSyntax or ElementAccessExpressionSyntax && semanticModel.GetTypeInfo(inspector, cancellationToken).Type?.TypeKind == TypeKind.Delegate))
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
