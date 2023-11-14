using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class AssertSingleShouldBeUsedForSingleParameterFixer : BatchedCodeFixProvider
{
	private const string DefaultParameterName = "item";
	public const string Key_UseSingleMethod = "xUnit2023_UseSingleMethod";

	public AssertSingleShouldBeUsedForSingleParameterFixer() :
		base(Descriptors.X2023_AssertSingleShouldBeUsedForSingleParameter.Id)
	{ }

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement))
			return;
		if (replacement is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				string.Format("Use Assert.{0}", replacement),
				ct => UseSingleMethod(context.Document, invocation, replacement, ct),
				Key_UseSingleMethod
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseSingleMethod(
		Document document,
		InvocationExpressionSyntax invocation,
		string replacementMethod,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
		    invocation.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax collectionVariable)
		{
			var replacementNode = invocation
				.WithArgumentList(ArgumentList(SeparatedList(new[] { Argument(collectionVariable) })))
				.WithExpression(memberAccess.WithName(IdentifierName(replacementMethod)));

			if (invocation.Parent != null)
			{
				var leadingTrivia = invocation.Parent.GetLeadingTrivia();
				var trailingTrivia = invocation.Parent.GetTrailingTrivia();

				if (invocation.ArgumentList.Arguments[1].Expression is SimpleLambdaExpressionSyntax lambdaExpression)
				{
					var oneItemVariableStatement = OneItemVariableStatement(lambdaExpression, replacementNode)
						.WithLeadingTrivia(leadingTrivia);

					ReplaceCollectionWithSingle(editor, oneItemVariableStatement, invocation.Parent);
					AppendLambdaStatements(editor, oneItemVariableStatement, lambdaExpression, leadingTrivia, trailingTrivia);
				}
				else if (invocation.ArgumentList.Arguments[1].Expression is IdentifierNameSyntax identifierExpression)
				{
					var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
					var isMethod = semanticModel.GetSymbolInfo(identifierExpression).Symbol?.Kind == SymbolKind.Method;
					if (isMethod)
					{
						var oneItemVariableStatement = OneItemVariableStatement(DefaultParameterName, replacementNode)
							.WithLeadingTrivia(leadingTrivia);

						ReplaceCollectionWithSingle(editor, oneItemVariableStatement, invocation.Parent);
						AppendMethodInvocation(editor, oneItemVariableStatement, identifierExpression, leadingTrivia, trailingTrivia);
					}
				}
			}
		}

		return editor.GetChangedDocument();
	}

	static LocalDeclarationStatementSyntax OneItemVariableStatement(
		SimpleLambdaExpressionSyntax lambdaExpression,
		InvocationExpressionSyntax replacementNode)
	{
		var lambdaParameterName = lambdaExpression.Parameter.Identifier.ValueText;

		return OneItemVariableStatement(lambdaParameterName, replacementNode);
	}

	static LocalDeclarationStatementSyntax OneItemVariableStatement(string parameterName, InvocationExpressionSyntax replacementNode)
	{
		var equalsToReplacementNode = EqualsValueClause(replacementNode);

		var oneItemVariableDeclaration = VariableDeclaration(
			ParseTypeName("var"),
			SingletonSeparatedList(
				VariableDeclarator(Identifier(parameterName))
					.WithInitializer(equalsToReplacementNode)
			)
		).NormalizeWhitespace();

		return LocalDeclarationStatement(oneItemVariableDeclaration);
	}

	static void ReplaceCollectionWithSingle(
		DocumentEditor editor,
		LocalDeclarationStatementSyntax oneItemVariableStatement,
		SyntaxNode invocationParent)
	{
		editor.ReplaceNode(
			invocationParent,
			oneItemVariableStatement
		);
	}

	static void AppendLambdaStatements(
		DocumentEditor editor,
		LocalDeclarationStatementSyntax oneItemVariableStatement,
		SimpleLambdaExpressionSyntax lambdaExpression,
		SyntaxTriviaList leadingTrivia,
		SyntaxTriviaList trailingTrivia)
	{
		if (lambdaExpression.ExpressionBody is InvocationExpressionSyntax lambdaBody)
		{
			var assertStatement = ExpressionStatement(lambdaBody)
				.WithLeadingTrivia(leadingTrivia)
				.WithTrailingTrivia(trailingTrivia);

			editor.InsertAfter(oneItemVariableStatement, assertStatement);
		}
		else if (lambdaExpression.Block != null && lambdaExpression.Block.Statements.Count != 0)
		{
			var allLambdaBlockStatements = lambdaExpression.Block.Statements.Select((s, i) =>
			{
				s = s
					.WithoutTrivia()
					.WithLeadingTrivia(leadingTrivia);
				if (i == lambdaExpression.Block.Statements.Count - 1)
					s = s.WithTrailingTrivia(trailingTrivia);
				return s;
			});

			editor.InsertAfter(oneItemVariableStatement, allLambdaBlockStatements);
		}
	}

	static void AppendMethodInvocation(
		DocumentEditor editor,
		LocalDeclarationStatementSyntax oneItemVariableStatement,
		IdentifierNameSyntax methodExpression,
		SyntaxTriviaList leadingTrivia,
		SyntaxTriviaList trailingTrivia)
	{
		var methodStatement = InvocationExpression(
				methodExpression,
				ArgumentList(
					SingletonSeparatedList(
						Argument(IdentifierName(DefaultParameterName)))
				))
			.WithLeadingTrivia(leadingTrivia)
			.WithTrailingTrivia(trailingTrivia);

		editor.InsertAfter(oneItemVariableStatement, ExpressionStatement(methodStatement));
	}
}
