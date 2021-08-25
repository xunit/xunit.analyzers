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
	public class AssertEqualShouldNotBeUsedForCollectionSizeCheckFixer : CodeFixProvider
	{
		const string titleTemplate = "Use Assert.{0}";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X2013_AssertEqualShouldNotBeUsedForCollectionSizeCheck.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
			var diagnostic = context.Diagnostics.First();
			var methodName = diagnostic.Properties[Constants.Properties.MethodName];
			var sizeValue = diagnostic.Properties[Constants.Properties.SizeValue];
			var replacement = GetReplacementMethodName(methodName, sizeValue);
			var title = string.Format(titleTemplate, replacement);

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					createChangedDocument: ct => UseCollectionSizeAssertionAsync(context.Document, invocation, replacement, ct),
					equivalenceKey: title
				),
				context.Diagnostics
			);
		}

		static string GetReplacementMethodName(
			string methodName,
			string literalValue)
		{
			if (literalValue == "1")
				return Constants.Asserts.Single;

			return methodName == Constants.Asserts.Equal ? Constants.Asserts.Empty : Constants.Asserts.NotEmpty;
		}

		static async Task<Document> UseCollectionSizeAssertionAsync(
			Document document,
			InvocationExpressionSyntax invocation,
			string replacementMethod,
			CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
			var expression = GetExpressionSyntax(invocation);

			editor.ReplaceNode(
				invocation,
				invocation
					.WithArgumentList(invocation.ArgumentList.WithArguments(SingletonSeparatedList(Argument(expression))))
					.WithExpression(memberAccess.WithName(IdentifierName(replacementMethod)))
			);

			return editor.GetChangedDocument();
		}

		private static ExpressionSyntax GetExpressionSyntax(InvocationExpressionSyntax invocation)
		{
			if (invocation.ArgumentList.Arguments[1].Expression is InvocationExpressionSyntax sizeInvocation)
				return ((MemberAccessExpressionSyntax)sizeInvocation.Expression).Expression;

			var sizeMemberAccess = invocation.ArgumentList.Arguments[1].Expression as MemberAccessExpressionSyntax;
			return sizeMemberAccess?.Expression;
		}
	}
}
