using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class InlineDataShouldBeUniqueWithinTheoryFixer : BatchedCodeFixProvider
{
	public const string Key_RemoveDuplicateInlineData = "xUnit1025_RemoveDuplicateInlineData";

	public InlineDataShouldBeUniqueWithinTheoryFixer() :
		base(Descriptors.X1025_InlineDataShouldBeUniqueWithinTheory.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var reportedNode = root.FindNode(context.Span);
		if (reportedNode is AttributeSyntax attributeDuplicate)
			context.RegisterCodeFix(
				CodeAction.Create(
					"Remove duplicate InlineData",
					ct => RemoveInlineDataDuplicate(context.Document, attributeDuplicate, ct),
					Key_RemoveDuplicateInlineData
				),
				context.Diagnostics
			);
	}

	static async Task<Document> RemoveInlineDataDuplicate(
		Document document,
		AttributeSyntax attributeDuplicate,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		editor.RemoveNode(attributeDuplicate);

		return editor.GetChangedDocument();
	}
}
