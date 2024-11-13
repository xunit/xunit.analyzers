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
public class AssertSingleShouldBeUsedForSingleParameterFixer : BatchedCodeFixProvider
{
	private const string DefaultParameterName = "item";
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

	static IEnumerable<SyntaxNode> GetLambdaStatements(SimpleLambdaExpressionSyntax lambdaExpression)
	{
		if (lambdaExpression.ExpressionBody is InvocationExpressionSyntax lambdaBody)
			yield return ExpressionStatement(lambdaBody).WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
		else if (lambdaExpression.Block is not null && lambdaExpression.Block.Statements.Count != 0)
			foreach (var statement in lambdaExpression.Block.Statements)
				yield return statement.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation);
	}

	static SyntaxNode GetMethodInvocation(
		IdentifierNameSyntax methodExpression,
		string parameterName) =>
			ExpressionStatement(
				InvocationExpression(
					methodExpression,
					ArgumentList(SingletonSeparatedList(Argument(IdentifierName(parameterName))))
				)
			);

	static LocalDeclarationStatementSyntax OneItemVariableStatement(
		string parameterName,
		InvocationExpressionSyntax replacementNode)
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
			XunitCodeAction.Create(
				ct => UseSingleMethod(context.Document, invocation, replacement, ct),
				Key_UseSingleMethod,
				"Use Assert.{0}", replacement
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
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			if (semanticModel is not null && invocation.Parent is not null)
			{
				var statements = new List<SyntaxNode>();
				var startLocation = invocation.GetLocation().SourceSpan.Start;
				var localSymbols = semanticModel.LookupSymbols(startLocation).OfType<ILocalSymbol>().Select(s => s.Name).ToImmutableHashSet();
				var replacementNode =
					invocation
						.WithArgumentList(ArgumentList(SeparatedList([Argument(collectionVariable)])))
						.WithExpression(memberAccess.WithName(IdentifierName(replacementMethod)));

				if (invocation.ArgumentList.Arguments[1].Expression is SimpleLambdaExpressionSyntax lambdaExpression)
				{
					var originalParameterName = lambdaExpression.Parameter.Identifier.Text;
					var parameterName = GetSafeVariableName(originalParameterName, localSymbols);

					if (parameterName != originalParameterName)
					{
						var body = lambdaExpression.Body;
						var tokens =
							body
								.DescendantTokens()
								.Where(t => t.IsKind(SyntaxKind.IdentifierToken) && t.Text == originalParameterName)
								.ToArray();
						body = body.ReplaceTokens(tokens, (t1, t2) => Identifier(t2.LeadingTrivia, parameterName, t2.TrailingTrivia));
						lambdaExpression = lambdaExpression.WithBody(body);
					}

					statements.Add(OneItemVariableStatement(parameterName, replacementNode).WithTriviaFrom(invocation.Parent));
					statements.AddRange(GetLambdaStatements(lambdaExpression));
				}
				else if (invocation.ArgumentList.Arguments[1].Expression is IdentifierNameSyntax identifierExpression)
				{
					var isMethod = semanticModel.GetSymbolInfo(identifierExpression, cancellationToken).Symbol?.Kind == SymbolKind.Method;
					if (isMethod)
					{
						var parameterName = GetSafeVariableName(DefaultParameterName, localSymbols);

						var oneItemVariableStatement =
							OneItemVariableStatement(parameterName, replacementNode)
								.WithLeadingTrivia(invocation.Parent.GetLeadingTrivia());

						statements.Add(OneItemVariableStatement(parameterName, replacementNode).WithTriviaFrom(invocation.Parent));
						statements.Add(GetMethodInvocation(identifierExpression, parameterName));
					}
				}

				editor.InsertBefore(invocation.Parent, statements);
				editor.RemoveNode(invocation.Parent);
			}
		}

		return editor.GetChangedDocument();
	}
}
