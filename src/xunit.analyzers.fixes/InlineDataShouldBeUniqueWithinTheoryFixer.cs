using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class InlineDataShouldBeUniqueWithinTheoryFixer : CodeFixProvider
	{
		const string title = "Remove InlineData duplicate";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X1025_InlineDataShouldBeUniqueWithinTheory.Id);

		// There is an issue when two (or more) duplicate attributes occur one after another. The batch fixer won't
		// merge multiple deletes at once due to close vicinity and the fixer would have to execute a few times.
		// If sb wants all to be deleted at once maybe reporting the original attribute with all its duplicates
		// as a collection passed with a fixer parameter. To be considered.
		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var reportedNode = root.FindNode(context.Span);
			if (reportedNode is AttributeSyntax attributeDuplicate)
				context.RegisterCodeFix(
					CodeAction.Create(
						title,
						ct => RemoveInlineDataDuplicate(context.Document, attributeDuplicate, ct),
						equivalenceKey: title
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
}
