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
    public class AssertRegexMatchShouldNotUseBoolLiteralCheckFixer : CodeFixProvider
    {
        const string titleTemplate = "Use Assert.{0}";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.X2008_AssertRegexMatchShouldNotUseBoolLiteralCheck.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
            var methodName = context.Diagnostics.First().Properties[AssertRegexMatchShouldNotUseBoolLiteralCheck.MethodName];
            var isStatic = context.Diagnostics.First().Properties[AssertRegexMatchShouldNotUseBoolLiteralCheck.IsStatic];
            var replacementMethod = methodName == "True" ? "Matches" : "DoesNotMatch";

            var title = string.Format(titleTemplate, replacementMethod);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument: ct => UseRegexCheckAsync(context.Document, invocation, replacementMethod, isStatic, ct),
                    equivalenceKey: title),
                context.Diagnostics);
        }

        static async Task<Document> UseRegexCheckAsync(Document document, InvocationExpressionSyntax invocation, string replacementMethod, string isStatic, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
            var regexIsMatchInvocation = (InvocationExpressionSyntax)invocation.ArgumentList.Arguments[0].Expression;

            if (isStatic == bool.TrueString)
            {
                editor.ReplaceNode(invocation,
                    invocation
                        .WithArgumentList(regexIsMatchInvocation.ArgumentList)
                        .WithExpression(memberAccess.WithName(SyntaxFactory.IdentifierName(replacementMethod))));
            }
            else
            {
                var regexMemberAccess = (MemberAccessExpressionSyntax)regexIsMatchInvocation.Expression;
                var regexMember = regexMemberAccess.Expression;

                editor.ReplaceNode(invocation,
                    invocation
                        .WithArgumentList(SyntaxFactory.ArgumentList(
                            SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Argument(regexMember), regexIsMatchInvocation.ArgumentList.Arguments[0] })))
                        .WithExpression(memberAccess.WithName(SyntaxFactory.IdentifierName(replacementMethod))));
            }

            return editor.GetChangedDocument();
        }
    }
}