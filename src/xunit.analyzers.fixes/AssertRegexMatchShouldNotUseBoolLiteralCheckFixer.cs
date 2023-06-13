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
	public class AssertRegexMatchShouldNotUseBoolLiteralCheckFixer : CodeFixProvider
	{
		const string titleTemplate = "Use Assert.{0}";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X2008_AssertRegexMatchShouldNotUseBoolLiteralCheck.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			if (root is null)
				return;

			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
			if (invocation is null)
				return;

			var diagnostic = context.Diagnostics.FirstOrDefault();
			if (diagnostic is null)
				return;
			if (!diagnostic.Properties.TryGetValue(Constants.Properties.MethodName, out var methodName))
				return;
			if (!diagnostic.Properties.TryGetValue(Constants.Properties.IsStatic, out var isStatic))
				return;
			if (!diagnostic.Properties.TryGetValue(Constants.Properties.Replacement, out var replacement))
				return;
			if (replacement is null)
				return;

			var title = string.Format(titleTemplate, replacement);

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					createChangedDocument: ct => UseRegexCheckAsync(context.Document, invocation, replacement, isStatic == bool.TrueString, ct),
					equivalenceKey: title
				),
				context.Diagnostics
			);
		}

		static async Task<Document> UseRegexCheckAsync(
			Document document,
			InvocationExpressionSyntax invocation,
			string replacement,
			bool isStatic,
			CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
				if (invocation.ArgumentList.Arguments.Count > 0 && invocation.ArgumentList.Arguments[0].Expression is InvocationExpressionSyntax regexIsMatchInvocation)
				{
					if (isStatic)
					{
						editor.ReplaceNode(
							invocation,
							invocation
								.WithArgumentList(ArgumentList(SeparatedList(regexIsMatchInvocation.ArgumentList.Arguments.Reverse())))
								.WithExpression(memberAccess.WithName(IdentifierName(replacement)))
						);
					}
					else if (regexIsMatchInvocation.ArgumentList.Arguments.Count > 0 && regexIsMatchInvocation.Expression is MemberAccessExpressionSyntax regexMemberAccess)
					{
						var regexMember = regexMemberAccess.Expression;

						editor.ReplaceNode(
							invocation,
							invocation
								.WithArgumentList(ArgumentList(SeparatedList(new[] { Argument(regexMember), regexIsMatchInvocation.ArgumentList.Arguments[0] })))
								.WithExpression(memberAccess.WithName(IdentifierName(replacement)))
						);
					}
				}

			return editor.GetChangedDocument();
		}
	}
}
