using System.Collections.Immutable;
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

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheckFixer : CodeFixProvider
	{
		const string titleTemplate = "Use Assert.{0}";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X2012_AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
			var diagnostic = context.Diagnostics.First();
			var assertMethodName = diagnostic.Properties[Constants.Properties.AssertMethodName];
			var replacement = assertMethodName == Constants.Asserts.True ? Constants.Asserts.Contains : Constants.Asserts.DoesNotContain;

			var title = string.Format(titleTemplate, replacement);
			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					createChangedDocument: ct => UseContainsCheck(context.Document, invocation, replacement, ct),
					equivalenceKey: title
				),
				context.Diagnostics
			);
		}

		static async Task<Document> UseContainsCheck(
			Document document,
			InvocationExpressionSyntax invocation,
			string replacementMethod,
			CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
			var invocationExpressionSyntax = (InvocationExpressionSyntax)invocation.ArgumentList.Arguments[0].Expression;
			var anyMethodInvocation = (MemberAccessExpressionSyntax)invocationExpressionSyntax.Expression;
			var anyTarget = anyMethodInvocation.Expression;

			editor.ReplaceNode(
				invocation,
				invocation
					.WithArgumentList(ArgumentList(SeparatedList(invocationExpressionSyntax.ArgumentList.Arguments.Insert(0, Argument(anyTarget)))))
					.WithExpression(memberAccess.WithName(IdentifierName(replacementMethod)))
			);

			return editor.GetChangedDocument();
		}
	}
}
