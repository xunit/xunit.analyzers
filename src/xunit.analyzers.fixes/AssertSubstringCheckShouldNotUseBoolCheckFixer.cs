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
	public class AssertSubstringCheckShouldNotUseBoolCheckFixer : CodeFixProvider
	{
		const string titleTemplate = "Use Assert.{0}";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X2009_AssertSubstringCheckShouldNotUseBoolCheck.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
			var diagnostic = context.Diagnostics.First();
			var assertMethodName = diagnostic.Properties[Constants.Properties.AssertMethodName];
			var substringMethodName = diagnostic.Properties[Constants.Properties.SubstringMethodName];
			var replacement = diagnostic.Properties[Constants.Properties.Replacement];
			var title = string.Format(titleTemplate, replacement);

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					createChangedDocument: ct => UseSubstringCheckAsync(context.Document, invocation, replacement, ct),
					equivalenceKey: title
				),
				context.Diagnostics
			);
		}

		static async Task<Document> UseSubstringCheckAsync(
			Document document,
			InvocationExpressionSyntax invocation,
			string replacementMethod,
			CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
			var substringInvocation = (InvocationExpressionSyntax)invocation.ArgumentList.Arguments[0].Expression;
			var substringMethodInvocation = (MemberAccessExpressionSyntax)substringInvocation.Expression;
			var substringTarget = substringMethodInvocation.Expression;

			editor.ReplaceNode(
				invocation,
				invocation
					.WithArgumentList(ArgumentList(SeparatedList(substringInvocation.ArgumentList.Arguments.Insert(1, Argument(substringTarget)))))
					.WithExpression(memberAccess.WithName(IdentifierName(replacementMethod)))
			);

			return editor.GetChangedDocument();
		}
	}
}
