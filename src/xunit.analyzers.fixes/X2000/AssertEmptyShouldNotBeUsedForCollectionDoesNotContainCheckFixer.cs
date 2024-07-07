using System.Composition;
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
public class AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheckFixer : BatchedCodeFixProvider
{
	public const string Key_UseAlternateAssert = "xUnit2029_UseAlternateAssert";

	public AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheckFixer() :
		base(Descriptors.X2029_AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		context.RegisterCodeFix(
			XunitCodeAction.Create(
				c => UseCheck(context.Document, invocation, c),
				Key_UseAlternateAssert,
				"Use DoesNotContain"
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseCheck(
		Document document,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		var arguments = invocation.ArgumentList.Arguments;
		if (arguments.Count == 1 && arguments[0].Expression is InvocationExpressionSyntax innerInvocationSyntax)
			if (invocation.Expression is MemberAccessExpressionSyntax outerMemberAccess && innerInvocationSyntax.Expression is MemberAccessExpressionSyntax memberAccess)
				if (innerInvocationSyntax.ArgumentList.Arguments[0].Expression is ExpressionSyntax innerArgument)
					editor.ReplaceNode(
						invocation,
						invocation
							.WithArgumentList(ArgumentList(SeparatedList([Argument(memberAccess.Expression), Argument(innerArgument)])))
							.WithExpression(outerMemberAccess.WithName(IdentifierName(Constants.Asserts.DoesNotContain)))
					);

		return editor.GetChangedDocument();
	}
}
