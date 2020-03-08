using System;
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

namespace Xunit.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class AssertStringEqualityCheckShouldNotUseBoolCheckFixer : CodeFixProvider
	{
		const string TitleTemplate = "Use Assert.{0}";
		const string EquivalenceKeyTemplate = "Use Assert.{0} for string equality checks";

		public sealed override ImmutableArray<string> FixableDiagnosticIds { get; }
			= ImmutableArray.Create(Descriptors.X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixer.Id);

		public sealed override FixAllProvider GetFixAllProvider()
			=> WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
			var assertMethodName = context.Diagnostics.First().Properties[AssertStringEqualityCheckShouldNotUseBoolCheck.AssertMethodName];
			var isStaticMethodCall = context.Diagnostics.First().Properties[AssertStringEqualityCheckShouldNotUseBoolCheck.IsStaticMethodCall];
			var ignoreCase = context.Diagnostics.First().Properties[AssertStringEqualityCheckShouldNotUseBoolCheck.IgnoreCase];
			var replacement = GetReplacementMethodName(assertMethodName);

			context.RegisterCodeFix(
				CodeAction.Create(
					string.Format(TitleTemplate, replacement),
					createChangedDocument: ct => UseEqualCheckAsync(context.Document, invocation, replacement, isStaticMethodCall, ignoreCase, ct),
					equivalenceKey: string.Format(EquivalenceKeyTemplate, replacement)),
				context.Diagnostics);
		}

		private static string GetReplacementMethodName(string assertMethodName)
		{
			return assertMethodName == "True" ? "Equal" : "NotEqual";
		}

		static async Task<Document> UseEqualCheckAsync(Document document, InvocationExpressionSyntax invocation, string replacementMethodName,
			string isStaticMethodCall, string ignoreCase, CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
			var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
			var equalsInvocation = (InvocationExpressionSyntax)invocation.ArgumentList.Arguments[0].Expression;
			var equalsMethodInvocation = (MemberAccessExpressionSyntax)equalsInvocation.Expression;
			var equalsTarget = equalsMethodInvocation.Expression;

			var arguments = isStaticMethodCall == bool.TrueString
				? equalsInvocation.ArgumentList.Arguments
				: equalsInvocation.ArgumentList.Arguments.Insert(0, SyntaxFactory.Argument(equalsTarget));

			if (ignoreCase == bool.TrueString)
			{
				arguments = arguments.Replace(
					arguments[arguments.Count - 1],
					SyntaxFactory.Argument(
						SyntaxFactory.NameColon(SyntaxFactory.IdentifierName("ignoreCase")),
						arguments[arguments.Count - 1].RefOrOutKeyword,
						SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)));
			}
			else if (ignoreCase == bool.FalseString)
			{
				arguments = arguments.RemoveAt(arguments.Count - 1);
			}

			editor.ReplaceNode(
				invocation,
				invocation.WithArgumentList(
					SyntaxFactory.ArgumentList(
						SyntaxFactory.SeparatedList(arguments)))
					.WithExpression(memberAccess.WithName(SyntaxFactory.IdentifierName(replacementMethodName))));

			return editor.GetChangedDocument();
		}
	}
}
