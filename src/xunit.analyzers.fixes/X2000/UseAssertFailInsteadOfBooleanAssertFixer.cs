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
public class UseAssertFailInsteadOfBooleanAssertFixer : BatchedCodeFixProvider
{
	public const string Key_UseAssertFail = "xUnit2020_UseAssertFail";

	public UseAssertFailInsteadOfBooleanAssertFixer() :
		base(Descriptors.X2020_UseAssertFailInsteadOfBooleanAssert.Id)
	{ }

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				"Use Assert.Fail",
				ct => UseAssertFail(context.Document, invocation, ct),
				Key_UseAssertFail
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseAssertFail(
		Document document,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
			if (invocation.ArgumentList.Arguments.Count == 2)
				editor.ReplaceNode(
					invocation,
					invocation
						.WithArgumentList(ArgumentList(SeparatedList(new[] { invocation.ArgumentList.Arguments[1] })))
						.WithExpression(memberAccess.WithName(IdentifierName(Constants.Asserts.Fail)))
				);

		return editor.GetChangedDocument();
	}
}
