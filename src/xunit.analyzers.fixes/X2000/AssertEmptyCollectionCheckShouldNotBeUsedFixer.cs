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
public class AssertEmptyCollectionCheckShouldNotBeUsedFixer : BatchedCodeFixProvider
{
	public const string AddElementInspectorTitle = "Add element inspector";
	public const string UseAssertEmptyCheckTitle = "Use Assert.Empty";

	public AssertEmptyCollectionCheckShouldNotBeUsedFixer() :
		base(Descriptors.X2011_AssertEmptyCollectionCheckShouldNotBeUsed.Id)
	{ }

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
				UseAssertEmptyCheckTitle,
				createChangedDocument: ct => UseEmptyCheck(context.Document, invocation, ct),
				equivalenceKey: UseAssertEmptyCheckTitle
			),
			context.Diagnostics
		);

		context.RegisterCodeFix(
			CodeAction.Create(
				AddElementInspectorTitle,
				createChangedDocument: ct => AddElementInspector(context.Document, invocation, ct),
				equivalenceKey: AddElementInspectorTitle
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
