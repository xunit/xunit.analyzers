using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers.Fixes;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class AssertEmptyCollectionCheckShouldNotBeUsedFixer : XunitCodeFixProvider
{
	public const string Key_AddElementInspector = "xUnit2011_AddElementInspector";
	public const string Key_UseAssertEmpty = "xUnit2011_UseAssertEmpty";

	public AssertEmptyCollectionCheckShouldNotBeUsedFixer() :
		base(Descriptors.X2011_AssertEmptyCollectionCheckShouldNotBeUsed.Id)
	{ }

	public override FixAllProvider? GetFixAllProvider() => BatchFixer;

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				"Use Assert.Empty",
				ct => UseEmptyCheck(context.Document, invocation, ct),
				Key_UseAssertEmpty
			),
			context.Diagnostics
		);

		context.RegisterCodeFix(
			CodeAction.Create(
				"Add element inspector",
				ct => AddElementInspector(context.Document, invocation, ct),
				Key_AddElementInspector
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseEmptyCheck(
		Document document,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
			editor.ReplaceNode(
				invocation,
				invocation.WithExpression(memberAccess.WithName(IdentifierName("Empty")))
			);

		return editor.GetChangedDocument();
	}

	static async Task<Document> AddElementInspector(
		Document document,
		InvocationExpressionSyntax invocation,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

		editor.ReplaceNode(
			invocation,
			invocation.WithArgumentList(invocation.ArgumentList.AddArguments(Argument(SimpleLambdaExpression(Parameter(Identifier("x")), Block()))))
		);

		return editor.GetChangedDocument();
	}
}
