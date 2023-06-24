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

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class AssertNullShouldNotBeCalledOnValueTypesFixer : BatchedCodeFixProvider
{
	public const string Key_RemoveAssert = "xUnit2002_RemoveAssert";

	public AssertNullShouldNotBeCalledOnValueTypesFixer() :
		base(Descriptors.X2002_AssertNullShouldNotBeCalledOnValueTypes.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var call = root.FindNode(context.Span).FirstAncestorOrSelf<ExpressionStatementSyntax>();
		if (call is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				"Remove unnecessary assertion",
				ct => RemoveCall(context.Document, call, ct),
				Key_RemoveAssert
			),
			context.Diagnostics
		);
	}

	async Task<Document> RemoveCall(
		Document document,
		ExpressionStatementSyntax call,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var containsLeadingComment =
			call
				.GetLeadingTrivia()
				.Any(t => t.IsKind(SyntaxKind.MultiLineCommentTrivia) || t.IsKind(SyntaxKind.SingleLineCommentTrivia));
		var removeOptions =
			containsLeadingComment
				? SyntaxRemoveOptions.KeepLeadingTrivia | SyntaxRemoveOptions.AddElasticMarker
				: SyntaxRemoveOptions.KeepNoTrivia;

		editor.RemoveNode(call, removeOptions);

		return editor.GetChangedDocument();
	}
}
