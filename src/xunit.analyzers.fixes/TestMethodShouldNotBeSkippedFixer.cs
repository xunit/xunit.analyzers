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
	public class TestMethodShouldNotBeSkippedFixer : CodeFixProvider
	{
		const string title = "Remove Skip Argument";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X1004_TestMethodShouldNotBeSkipped.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			if (root is null)
				return;

			var argument = root.FindNode(context.Span).FirstAncestorOrSelf<AttributeArgumentSyntax>();
			if (argument is null)
				return;

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					ct => RemoveArgument(context.Document, argument, ct),
					equivalenceKey: title
				),
				context.Diagnostics
			);
		}

		async Task<Document> RemoveArgument(
			Document document,
			AttributeArgumentSyntax argument,
			CancellationToken ct)
		{
			var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

			editor.RemoveNode(argument);

			return editor.GetChangedDocument();
		}
	}
}
