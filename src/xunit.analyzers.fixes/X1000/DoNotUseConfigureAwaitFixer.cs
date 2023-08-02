using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class DoNotUseConfigureAwaitFixer : BatchedCodeFixProvider
{
	public const string Key_RemoveConfigureAwait = "xUnit1030_RemoveConfigureAwait";

	public DoNotUseConfigureAwaitFixer() :
		base(Descriptors.X1030_DoNotUseConfigureAwait.Id)
	{ }

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		// The syntax node (the invocation) will include "(any preceding trivia)(any preceding code).ConfigureAwait(args)" despite
		// the context.Span only covering "ConfigureAwait(args)". So we need to replace the whole invocation
		// with an invocation that does not include the ConfigureAwait call.
		var syntaxNode = root.FindNode(context.Span);
		var syntaxText = syntaxNode.ToFullString();

		// Remove the context span (plus the preceding .)
		var newSyntaxText = syntaxText.Substring(0, context.Span.Start - syntaxNode.FullSpan.Start - 1);
		var newSyntaxNode = SyntaxFactory.ParseExpression(newSyntaxText);

		context.RegisterCodeFix(
			CodeAction.Create(
				"Remove ConfigureAwait call",
				async ct =>
				{
					var editor = await DocumentEditor.CreateAsync(context.Document, ct).ConfigureAwait(false);
					editor.ReplaceNode(syntaxNode, newSyntaxNode);
					return editor.GetChangedDocument();
				},
				Key_RemoveConfigureAwait
			),
			context.Diagnostics
		);
	}
}
