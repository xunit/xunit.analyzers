using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class AssertSingleShouldBeUsedForSingleParameterFixer : BatchedCodeFixProvider
{
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

			if (invocation.ArgumentList.Arguments[1].Expression is SimpleLambdaExpressionSyntax lambdaExpression)
			{
				var lambdaParameterName = lambdaExpression.Parameter.Identifier.ValueText;
				var equalsToReplacementNode = EqualsValueClause(replacementNode);

				var oneItemVariableDeclaration = VariableDeclaration(
					ParseTypeName("var"),
					SeparatedList<VariableDeclaratorSyntax>().Add(
						VariableDeclarator(Identifier(lambdaParameterName))
							.WithInitializer(equalsToReplacementNode)
					)
				).NormalizeWhitespace();

				var oneItemVariableStatement = LocalDeclarationStatement(oneItemVariableDeclaration);
				if (invocation.Parent != null)
				{
					oneItemVariableStatement = oneItemVariableStatement.WithLeadingTrivia(invocation.Parent.GetLeadingTrivia());
					editor.ReplaceNode(
						invocation.Parent,
						oneItemVariableStatement
					);

					if (lambdaExpression.ExpressionBody is InvocationExpressionSyntax lambdaBody)
					{
						var assertStatement = ExpressionStatement(lambdaBody)
							.WithLeadingTrivia(invocation.Parent.GetLeadingTrivia())
							.WithTrailingTrivia(invocation.Parent.GetTrailingTrivia());

						editor.InsertAfter(oneItemVariableStatement, assertStatement);
					}
				}
			}
		}

		return editor.GetChangedDocument();
	}
}
