using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class AssertEmptyCollectionCheckShouldNotBeUsedFixer : CodeFixProvider
	{
		private const string UseAssertEmptyCheckTitle = "Use Assert.Empty";
		private const string AddElementInspectorTitle = "Add element inspector";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; }
			= ImmutableArray.Create(Descriptors.X2011_AssertEmptyCollectionCheckShouldNotBeUsed.Id);

		public sealed override FixAllProvider GetFixAllProvider()
			=> WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();

			context.RegisterCodeFix(
				CodeAction.Create(
					UseAssertEmptyCheckTitle,
					createChangedDocument: ct => UseEmptyCheckAsync(context.Document, invocation, ct),
					equivalenceKey: UseAssertEmptyCheckTitle),
				context.Diagnostics);

			context.RegisterCodeFix(
				CodeAction.Create(
					AddElementInspectorTitle,
					createChangedDocument: ct => AddElementInspectorAsync(context.Document, invocation, ct),
					equivalenceKey: AddElementInspectorTitle),
				context.Diagnostics);
		}

		private static async Task<Document> UseEmptyCheckAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
			editor.ReplaceNode(
				invocation,
				invocation.WithExpression(memberAccess.WithName(SyntaxFactory.IdentifierName("Empty"))));

			return editor.GetChangedDocument();
		}

		private static async Task<Document> AddElementInspectorAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			editor.ReplaceNode(
				invocation,
				invocation.WithArgumentList(
					invocation.ArgumentList.AddArguments(
						SyntaxFactory.Argument(
							SyntaxFactory.SimpleLambdaExpression(
								SyntaxFactory.Parameter(SyntaxFactory.Identifier("x")),
								SyntaxFactory.Block())))));

			return editor.GetChangedDocument();
		}
	}
}
