using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class DoNotUseConfigureAwaitFixer : XunitCodeFixProvider
{
	public const string Key_RemoveConfigureAwait = "xUnit1030_RemoveConfigureAwait";
	public const string Key_ReplaceArgumentValue = "xUnit1030_ReplaceArgumentValue";

	public DoNotUseConfigureAwaitFixer() :
		base(Descriptors.X1030_DoNotUseConfigureAwait.Id)
	{ }

	public override FixAllProvider? GetFixAllProvider() => BatchFixer;

	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;

		// Get the original and replacement values
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.ArgumentValue, out var original))
			return;
		if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement))
			return;

		// The syntax node (the invocation) will include "(any preceding trivia)(any preceding code).ConfigureAwait(args)" despite
		// the context.Span only covering "ConfigureAwait(args)". So we need to replace the whole invocation
		// with an invocation that does not include the ConfigureAwait call.
		var syntaxNode = root.FindNode(context.Span);
		var syntaxText = syntaxNode.ToFullString();

		// Remove the context span (plus the preceding .)
		var removeConfigureAwaitText = syntaxText.Substring(0, context.Span.Start - syntaxNode.FullSpan.Start - 1);
		var removeConfigureAwaitNode = SyntaxFactory.ParseExpression(removeConfigureAwaitText);

		// Only offer the removal fix if the replacement value is 'true', because anybody using ConfigureAwaitOptions
		// will want to just add the extra value, not remove the call entirely.
		if (replacement == "true")
			context.RegisterCodeFix(
				CodeAction.Create(
					"Remove ConfigureAwait call",
					async ct =>
					{
						var editor = await DocumentEditor.CreateAsync(context.Document, ct).ConfigureAwait(false);
						editor.ReplaceNode(syntaxNode, removeConfigureAwaitNode);
						return editor.GetChangedDocument();
					},
					Key_RemoveConfigureAwait
				),
				context.Diagnostics
			);

		// Offer the replacement fix
		var replaceConfigureAwaitText = removeConfigureAwaitText + ".ConfigureAwait(" + replacement + ")";
		var replaceConfigureAwaitNode = SyntaxFactory.ParseExpression(replaceConfigureAwaitText);

		context.RegisterCodeFix(
			XunitCodeAction.Create(
				async ct =>
				{
					var editor = await DocumentEditor.CreateAsync(context.Document, ct).ConfigureAwait(false);
					editor.ReplaceNode(syntaxNode, replaceConfigureAwaitNode);
					return editor.GetChangedDocument();
				},
				Key_ReplaceArgumentValue,
				"Replace ConfigureAwait({0}) with ConfigureAwait({1})", original, replacement
			),
			context.Diagnostics
		);
	}
}
