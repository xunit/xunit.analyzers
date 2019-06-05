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
        private const string Title = "Convert to Empty/Contains";

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

            // Expected behavior
            // Assert.Null({expr}.FirstOrDefault()) -> Assert.Empty({expr}) - DONE
            // Assert.NotNull({expr}.FirstOrDefault()) -> Assert.NotEmpty({expr})
            // Assert.Null({expr}.FirstOrDefault({expr1})) -> Assert.DoesNotContain({expr}, {expr1}) - DONE
            // Assert.NotNull({expr}.FirstOrDefault({expr1})) -> Assert.Contains({expr}, {expr1})

            var argumentInvocationExpression = invocationExpression.ArgumentList.Arguments[0].Expression as InvocationExpressionSyntax;
            var argumentMemberAccessExpression = argumentInvocationExpression?.Expression as MemberAccessExpressionSyntax;

            if (argumentMemberAccessExpression?.Name.ToString() == "FirstOrDefault")
            {
                var expressionInFirstOrDefault = argumentInvocationExpression
                    .ArgumentList
                    .Arguments
                    .FirstOrDefault()?
                    .Expression;

                newAssertExpression = expressionInFirstOrDefault == null ?
                    BuildAssertExpression("Empty", argumentMemberAccessExpression.Expression) :
                    BuildAssertExpression("DoesNotContain", argumentMemberAccessExpression.Expression, expressionInFirstOrDefault);
            }

            var root = await document.GetSyntaxRootAsync();

            var newRoot = root
                .ReplaceNode(invocationExpression, newAssertExpression)
                .NormalizeWhitespace();

            return document.WithSyntaxRoot(newRoot);
        }

        private static InvocationExpressionSyntax BuildAssertExpression(string identifier, params ExpressionSyntax[] argumentExpressions)
        {
            var assert = SyntaxFactory.IdentifierName("Assert");
            var identifierName = SyntaxFactory.IdentifierName(identifier);
            var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, assert, identifierName);

            var arguments = argumentExpressions
                .Select(SyntaxFactory.Argument)
                .ToArray();

            var argumentList = SyntaxFactory.SeparatedList(arguments);

            return SyntaxFactory.InvocationExpression(memberAccess,
                   SyntaxFactory.ArgumentList(argumentList));
        }
    }
}
