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
    public class AssertSubstringCheckShouldNotUseBoolCheckFixer : CodeFixProvider
    {
        const string TitleTemplate = "Use Assert.{0}";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.X2009_AssertSubstringCheckShouldNotUseBoolCheck.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
            var assertMethodName = context.Diagnostics.First().Properties[AssertSubstringCheckShouldNotUseBoolCheck.AssertMethodName];
            var substringMethodName = context.Diagnostics.First().Properties[AssertSubstringCheckShouldNotUseBoolCheck.SubstringMethodName];
            var replacement = GetReplacementMethodName(assertMethodName, substringMethodName);

            var title = String.Format(TitleTemplate, replacement);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument: ct => UseSubstringCheckAsync(context.Document, invocation, replacement, ct),
                    equivalenceKey: title),
                context.Diagnostics);
        }

        private static string GetReplacementMethodName(string assertMethodName, string substringMethodName)
        {
            if (substringMethodName == "Contains")
                return assertMethodName == "True" ? "Contains" : "DoesNotContain";

            return substringMethodName;
        }

        static async Task<Document> UseSubstringCheckAsync(Document document, InvocationExpressionSyntax invocation, string replacementMethod, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
            var substringInvocation = (InvocationExpressionSyntax)invocation.ArgumentList.Arguments[0].Expression;

            var substringMethodInvocation = (MemberAccessExpressionSyntax)substringInvocation.Expression;
            var substringTarget = substringMethodInvocation.Expression;

            editor.ReplaceNode(invocation,
                invocation
                    .WithArgumentList(SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(substringInvocation.ArgumentList.Arguments.Insert(1, SyntaxFactory.Argument(substringTarget)))))
                    .WithExpression(memberAccess.WithName(SyntaxFactory.IdentifierName(replacementMethod))));
            return editor.GetChangedDocument();
        }
    }
}