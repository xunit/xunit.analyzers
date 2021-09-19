using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class AssertEmptyCollectionCheckShouldNotBeUsedFixer : CodeFixProvider
	{
		const string addElementInspectorTitle = "Add element inspector";
		const string useAssertEmptyCheckTitle = "Use Assert.Empty";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X2011_AssertEmptyCollectionCheckShouldNotBeUsed.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();

			context.RegisterCodeFix(
				CodeAction.Create(
					useAssertEmptyCheckTitle,
					createChangedDocument: ct => UseEmptyCheck(context.Document, invocation, ct),
					equivalenceKey: useAssertEmptyCheckTitle
				),
				context.Diagnostics
			);

			context.RegisterCodeFix(
				CodeAction.Create(
					addElementInspectorTitle,
					createChangedDocument: ct => AddElementInspector(context.Document, invocation, ct),
					equivalenceKey: addElementInspectorTitle
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
}
