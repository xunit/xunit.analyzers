using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
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

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		if (!Debugger.IsAttached)
			Debugger.Launch();

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
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			if (semanticModel != null && invocation.Parent != null)
			{
				var startLocation = invocation.GetLocation().SourceSpan.Start;
				var localSymbols = semanticModel.LookupSymbols(startLocation).OfType<ILocalSymbol>().Select(s => s.Name).ToImmutableHashSet();
				var replacementNode =
					invocation
						.WithArgumentList(ArgumentList(SeparatedList(new[] { Argument(collectionVariable) })))
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
								.Where(t => t.Kind() == SyntaxKind.IdentifierToken && t.Text == originalParameterName)
								.ToArray();
						body = body.ReplaceTokens(tokens, (t1, t2) => Identifier(t2.LeadingTrivia, parameterName, t2.TrailingTrivia));
						lambdaExpression = lambdaExpression.WithBody(body);
					}

					var oneItemVariableStatement =
						OneItemVariableStatement(parameterName, replacementNode)
							.WithLeadingTrivia(invocation.GetLeadingTrivia());

					ReplaceCollectionWithSingle(editor, oneItemVariableStatement, invocation.Parent);
					AppendLambdaStatements(editor, oneItemVariableStatement, lambdaExpression);
				}
				else if (invocation.ArgumentList.Arguments[1].Expression is IdentifierNameSyntax identifierExpression)
				{
					var isMethod = semanticModel.GetSymbolInfo(identifierExpression).Symbol?.Kind == SymbolKind.Method;
					if (isMethod)
					{
						var parameterName = GetSafeVariableName(DefaultParameterName, localSymbols);

						var oneItemVariableStatement =
							OneItemVariableStatement(parameterName, replacementNode)
								.WithLeadingTrivia(invocation.GetLeadingTrivia());

						ReplaceCollectionWithSingle(editor, oneItemVariableStatement, invocation.Parent);
						AppendMethodInvocation(editor, oneItemVariableStatement, identifierExpression, parameterName);
					}
				}
			}
		}

		return editor.GetChangedDocument();
	}

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
		SimpleLambdaExpressionSyntax lambdaExpression)
	{
		if (lambdaExpression.ExpressionBody is InvocationExpressionSyntax lambdaBody)
		{
			editor.InsertAfter(
				oneItemVariableStatement,
				ExpressionStatement(lambdaBody)
					.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)
			);
		}
		else if (lambdaExpression.Block != null && lambdaExpression.Block.Statements.Count != 0)
		{
			editor.InsertAfter(
				oneItemVariableStatement,
				lambdaExpression.Block.Statements.Select(
					(s, i) => s.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)
				)
			);
		}
	}

	static void AppendMethodInvocation(
		DocumentEditor editor,
		LocalDeclarationStatementSyntax oneItemVariableStatement,
		IdentifierNameSyntax methodExpression,
		string parameterName)
	{
		editor.InsertAfter(
			oneItemVariableStatement,
			ExpressionStatement(
				InvocationExpression(
					methodExpression,
					ArgumentList(SingletonSeparatedList(Argument(IdentifierName(parameterName))))
				)
			)
			.WithAdditionalAnnotations(Formatter.Annotation, Simplifier.Annotation)
		);
	}
}
