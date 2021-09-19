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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class AssertIsTypeShouldNotBeUsedForAbstractTypeFixer : CodeFixProvider
	{
		static readonly string title = $"Use Assert.{Constants.Asserts.IsAssignableFrom}";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X2018_AssertIsTypeShouldNotBeUsedForAbstractType.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
			var simpleNameSyntax = invocation.GetSimpleName();
			if (simpleNameSyntax is null)
				return;

			var methodName = simpleNameSyntax.Identifier.Text;

			if (methodName == Constants.Asserts.IsType)
				context.RegisterCodeFix(
					CodeAction.Create(
						title,
						createChangedDocument: ct => UseIsAssignableFrom(context.Document, simpleNameSyntax, ct),
						equivalenceKey: title
					),
					context.Diagnostics
				);
		}

		static async Task<Document> UseIsAssignableFrom(
			Document document,
			SimpleNameSyntax simpleName,
			CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

			editor.ReplaceNode(
				simpleName,
				simpleName.WithIdentifier(Identifier(Constants.Asserts.IsAssignableFrom))
			);

			return editor.GetChangedDocument();
		}
	}
}
