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
public class AssertEqualGenericShouldNotBeUsedForStringValueFixer : XunitCodeFixProvider
{
	public const string Key_UseStringAssertEqual = "xUnit2006_UseStringAssertEqual";

	public AssertEqualGenericShouldNotBeUsedForStringValueFixer() :
		base(Descriptors.X2006_AssertEqualGenericShouldNotBeUsedForStringValue.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var syntaxNode = root.FindNode(context.Span);
		var invocation = syntaxNode.FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		if (invocation.Expression is MemberAccessExpressionSyntax)
			context.RegisterCodeFix(
				CodeAction.Create(
					"Use string Assert.Equal",
					ct => UseNonGenericStringEqualCheck(context.Document, invocation, ct),
					Key_UseStringAssertEqual
				),
				context.Diagnostics
			);
	}

	static async Task<Document> UseNonGenericStringEqualCheck(
		Document document,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
			editor.ReplaceNode(
				memberAccess,
				memberAccess.WithName(IdentifierName(Constants.Asserts.Equal))
			);

		return editor.GetChangedDocument();
	}
}
