using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class TestMethodShouldNotBeSkippedFixer : XunitCodeFixProvider
{
	public const string Key_RemoveSkipArgument = "xUnit1004_RemoveSkipArgument";

	public TestMethodShouldNotBeSkippedFixer() :
		base(Descriptors.X1004_TestMethodShouldNotBeSkipped.Id)
	{ }

	public override FixAllProvider? GetFixAllProvider() => BatchFixer;

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
				"Remove Skip argument",
				ct => RemoveArgument(context.Document, argument, ct),
				Key_RemoveSkipArgument
			),
			context.Diagnostics
		);
	}

	static async Task<Document> RemoveArgument(
		Document document,
		AttributeArgumentSyntax argument,
		CancellationToken ct)
	{
		var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);

		editor.RemoveNode(argument);

		return editor.GetChangedDocument();
	}
}
