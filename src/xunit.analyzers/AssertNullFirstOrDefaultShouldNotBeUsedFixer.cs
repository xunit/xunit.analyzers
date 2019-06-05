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

namespace Xunit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class AssertNullFirstOrDefaultShouldNotBeUsedFixer : CodeFixProvider
    {
        private const string Title = "Switch to Empty/Contains";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Descriptors.X2020_AssertNullFirstOrDefaultShouldNotBeUsed.Id);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context
                .Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics[0];
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var invocationExpression = root
                .FindToken(diagnosticSpan.Start)
                .Parent
                .AncestorsAndSelf()
                .OfType<InvocationExpressionSyntax>()
                .First();

            context.RegisterCodeFix(
                CodeAction.Create(Title, c =>
                FixAssertionAsync(context.Document, invocationExpression, c)), diagnostic);
        }

        private async Task<Document> FixAssertionAsync(Document document, InvocationExpressionSyntax invocationExpression, CancellationToken cancellationToken)
        {
            InvocationExpressionSyntax newAssertExpression = null;

            // TODO: Later you'll need to check whether it's a call to Null or NotNull, with/without arguments etc.

            // Is empty FirstOrDefault
            if (invocationExpression.ArgumentList.Arguments[0].Expression is InvocationExpressionSyntax argumentInvocationExpression &&
                argumentInvocationExpression.Expression is MemberAccessExpressionSyntax argumentMemberAccessExpression &&
                argumentMemberAccessExpression.Name.ToString() == "FirstOrDefault")
            {
                var assert = SyntaxFactory.IdentifierName("Assert");
                var empty = SyntaxFactory.IdentifierName("Empty");
                var memberaccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, assert, empty);

                var argument = SyntaxFactory.Argument(argumentMemberAccessExpression.Expression);
                var argumentList = SyntaxFactory.SeparatedList(new[] { argument });

                newAssertExpression =
                    SyntaxFactory.InvocationExpression(memberaccess,
                    SyntaxFactory.ArgumentList(argumentList));
            }

            var root = await document.GetSyntaxRootAsync();

            var newRoot = root.ReplaceNode(invocationExpression, newAssertExpression).NormalizeWhitespace();
            var newDocument = document.WithSyntaxRoot(newRoot);

            return newDocument;
        }
    }
}
