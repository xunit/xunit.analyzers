using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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
	public class AssertStringEqualityCheckShouldNotUseBoolCheckFixer : CodeFixProvider
	{
		const string equivalenceKeyTemplate = "Use Assert.{0} for string equality checks";
		const string titleTemplate = "Use Assert.{0}";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer.Id);

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
			var diagnostic = context.Diagnostics.First();
			var assertMethodName = diagnostic.Properties[Constants.Properties.AssertMethodName];
			var isStaticMethodCall = diagnostic.Properties[Constants.Properties.IsStaticMethodCall];
			var ignoreCase = diagnostic.Properties[Constants.Properties.IgnoreCase];
			var replacement = GetReplacementMethodName(assertMethodName);

			context.RegisterCodeFix(
				CodeAction.Create(
					string.Format(titleTemplate, replacement),
					createChangedDocument: ct => UseEqualCheck(context.Document, invocation, replacement, isStaticMethodCall, ignoreCase, ct),
					equivalenceKey: string.Format(equivalenceKeyTemplate, replacement)
				),
				context.Diagnostics
			);
		}

		static string GetReplacementMethodName(string assertMethodName) =>
			assertMethodName == Constants.Asserts.True ? Constants.Asserts.Equal : Constants.Asserts.NotEqual;

		static async Task<Document> UseEqualCheck(
			Document document,
			InvocationExpressionSyntax invocation,
			string replacementMethodName,
			string isStaticMethodCall,
			string ignoreCase,
			CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
			var equalsInvocation = (InvocationExpressionSyntax)invocation.ArgumentList.Arguments[0].Expression;
			var equalsMethodInvocation = (MemberAccessExpressionSyntax)equalsInvocation.Expression;
			var equalsTarget = equalsMethodInvocation.Expression;
			var arguments =
				isStaticMethodCall == bool.TrueString
					? equalsInvocation.ArgumentList.Arguments
					: equalsInvocation.ArgumentList.Arguments.Insert(0, Argument(equalsTarget));

			if (ignoreCase == bool.TrueString)
				arguments = arguments.Replace(
					arguments[arguments.Count - 1],
					Argument(
						NameColon(IdentifierName(Constants.AssertArguments.IgnoreCase)),
						arguments[arguments.Count - 1].RefOrOutKeyword,
						LiteralExpression(SyntaxKind.TrueLiteralExpression)
					)
				);
			else if (ignoreCase == bool.FalseString)
				arguments = arguments.RemoveAt(arguments.Count - 1);

			editor.ReplaceNode(
				invocation,
				invocation
					.WithArgumentList(ArgumentList(SeparatedList(arguments)))
					.WithExpression(memberAccess.WithName(IdentifierName(replacementMethodName)))
			);

			return editor.GetChangedDocument();
		}
	}
}
