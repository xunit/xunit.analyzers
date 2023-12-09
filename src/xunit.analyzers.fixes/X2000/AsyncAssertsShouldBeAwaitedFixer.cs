using System.Composition;
using System.Linq;
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
public class AsyncAssertsShouldBeAwaitedFixer : BatchedCodeFixProvider
{
	public const string Key_AddAwait = "xUnit2021_AddAwait";

	public AsyncAssertsShouldBeAwaitedFixer() :
		base(Descriptors.X2021_AsyncAssertionsShouldBeAwaited.Id)
	{ }

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
		if (invocation is null)
			return;

		var method = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
		if (method is null)
			return;

		var diagnostic = context.Diagnostics.FirstOrDefault();
		if (diagnostic is null)
			return;

		context.RegisterCodeFix(
			CodeAction.Create(
				"Add await",
				ct => UseAsyncAwait(context.Document, invocation, method, ct),
				Key_AddAwait
			),
			context.Diagnostics
		);
	}

	static async Task<Document> UseAsyncAwait(
		Document document,
		InvocationExpressionSyntax invocation,
		MethodDeclarationSyntax method,
		CancellationToken cancellationToken)
	{
		var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
		var modifiers = AsyncHelper.GetModifiersWithAsyncKeywordAdded(method);
		var returnType = await AsyncHelper.GetReturnType(method, invocation, document, editor, cancellationToken).ConfigureAwait(false);
		var asyncThrowsInvocation = AwaitExpression(invocation.WithoutLeadingTrivia()).WithLeadingTrivia(invocation.GetLeadingTrivia());

		if (returnType is not null)
			editor.ReplaceNode(
				method,
				method
					.ReplaceNode(invocation, asyncThrowsInvocation)
					.WithModifiers(modifiers)
					.WithReturnType(returnType)
			);

		return editor.GetChangedDocument();
	}
}
