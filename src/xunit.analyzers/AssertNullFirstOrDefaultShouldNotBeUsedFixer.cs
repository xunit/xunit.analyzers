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

            (var methodToConvertTo, var newAssertExpression) = ConvertInvocation(invocationExpression);

            context.RegisterCodeFix(
                CodeAction.Create(TitleTemplate(methodToConvertTo), c =>
                FixAssertionAsync(context.Document, invocationExpression, newAssertExpression, c)), diagnostic);
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

        private static (string newAssertMethod, InvocationExpressionSyntax newAssertInvocation) ConvertInvocation(InvocationExpressionSyntax currentInvocationExpression)
        {
            var memberAccessExpression = currentInvocationExpression.Expression as MemberAccessExpressionSyntax;

            var calledAssertMethodName = memberAccessExpression?.Name.ToString();

            InvocationExpressionSyntax newAssertExpression = null;
            string methodToConvertTo = string.Empty;

            var argumentInvocationExpression = currentInvocationExpression.ArgumentList.Arguments[0].Expression as InvocationExpressionSyntax;
            var argumentMemberAccessExpression = argumentInvocationExpression?.Expression as MemberAccessExpressionSyntax;

            if (argumentMemberAccessExpression?.Name.ToString() == "FirstOrDefault")
            {
                var expressionInsideFirstOrDefault = argumentInvocationExpression
                    .ArgumentList
                    .Arguments
                    .FirstOrDefault()?
                    .Expression;

                var firstOrDefaultHasAnArgument = expressionInsideFirstOrDefault != null;

                var expressionBeforeFirstOrDefault = argumentMemberAccessExpression.Expression;

                methodToConvertTo = GetMethodToConvertTo(calledAssertMethodName, firstOrDefaultHasAnArgument);

                var argumentsList = !firstOrDefaultHasAnArgument ?
                    new[] { expressionBeforeFirstOrDefault } :
                    new[] { expressionBeforeFirstOrDefault, expressionInsideFirstOrDefault };

                newAssertExpression = BuildAssertExpression(methodToConvertTo, argumentsList);
            }

            return (methodToConvertTo, newAssertExpression);
        }

        private static string GetMethodToConvertTo(string calledAssertMethodName, bool firstOrDefaultHasAnArgument)
        {
            if (calledAssertMethodName == "Null")
            {
                return !firstOrDefaultHasAnArgument ?
                    "Empty" :
                    "DoesNotContain";
            }
            else if (calledAssertMethodName == "NotNull")
            {
                return !firstOrDefaultHasAnArgument ?
                    "NotEmpty" :
                    "Contains";
            }

            return string.Empty;
        }

        private async Task<Document> FixAssertionAsync(
            Document document,
            InvocationExpressionSyntax invocationExpression,
            InvocationExpressionSyntax newAssertExpression,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken);

            var newRoot = root
                .ReplaceNode(invocationExpression, newAssertExpression)
                .NormalizeWhitespace();

            return document.WithSyntaxRoot(newRoot);
        }

        private string TitleTemplate(string newMethod) => $"Convert to {newMethod}";
    }
}
