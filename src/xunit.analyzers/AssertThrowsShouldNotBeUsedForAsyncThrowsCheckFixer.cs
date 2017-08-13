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
    public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheckFixer : CodeFixProvider
    {
        const string TitleTemplate = "Use Assert.{0}";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Descriptors.X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck.Id,
            Descriptors.X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck_Hidden.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
            var method = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            var methodName = context.Diagnostics.First().Properties[AssertThrowsShouldNotBeUsedForAsyncThrowsCheck.MethodName];

            var title = string.Format(TitleTemplate, methodName + "Async");
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument: ct => UseAsyncThrowsCheckAsync(context.Document, invocation, method, methodName + "Async", ct),
                    equivalenceKey: title),
                context.Diagnostics);
        }

        static async Task<Document> UseAsyncThrowsCheckAsync(Document document, InvocationExpressionSyntax invocation, MethodDeclarationSyntax method, string replacementMethod, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;

            var modifiers = GetModifiersWithAsyncKeywordAdded(method);
            var returnType = await GetReturnType(method, invocation, document, editor, cancellationToken);
            var asyncThrowsInvocation = GetAsyncThrowsInvocation(invocation, replacementMethod, memberAccess);

            editor.ReplaceNode(method,
                method
                    .ReplaceNode(invocation, asyncThrowsInvocation)
                    .WithModifiers(modifiers)
                    .WithReturnType(returnType));

            return editor.GetChangedDocument();
        }

        private static SyntaxTokenList GetModifiersWithAsyncKeywordAdded(MethodDeclarationSyntax method)
        {
            return method.Modifiers.Any(SyntaxKind.AsyncKeyword)
                ? method.Modifiers
                : method.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));
        }

        private static async Task<TypeSyntax> GetReturnType(MethodDeclarationSyntax method, InvocationExpressionSyntax invocation,
            Document document, DocumentEditor editor, CancellationToken cancellationToken)
        {
            // Consider the case where a custom awaiter type is awaited
            if (invocation.Parent.IsKind(SyntaxKind.AwaitExpression))
                return method.ReturnType;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var methodSymbol = semanticModel.GetSymbolInfo(method.ReturnType, cancellationToken).Symbol as ITypeSymbol;
            var taskType = semanticModel.Compilation.GetTypeByMetadataName(typeof(Task).FullName);

            if (taskType.IsAssignableFrom(methodSymbol))
                return method.ReturnType;

            return (TypeSyntax)editor.Generator.TypeExpression(taskType);
        }

        private static ExpressionSyntax GetAsyncThrowsInvocation(InvocationExpressionSyntax invocation,
            string replacementMethod, MemberAccessExpressionSyntax memberAccess)
        {
            ExpressionSyntax asyncThrowsInvocation =
                invocation
                    .WithExpression(memberAccess.WithName(GetName(replacementMethod, memberAccess)))
                    .WithArgumentList(GetArguments(invocation));

            if (invocation.Parent.IsKind(SyntaxKind.AwaitExpression))
                return asyncThrowsInvocation;

            return SyntaxFactory.AwaitExpression(asyncThrowsInvocation.WithoutLeadingTrivia())
                    .WithLeadingTrivia(invocation.GetLeadingTrivia());
        }

        private static SimpleNameSyntax GetName(string replacementMethod, MemberAccessExpressionSyntax memberAccess)
        {
            var genericNameSyntax = memberAccess.Name as GenericNameSyntax;

            if (genericNameSyntax == null)
                return SyntaxFactory.IdentifierName(replacementMethod);

            return SyntaxFactory.GenericName(SyntaxFactory.IdentifierName(replacementMethod).Identifier, genericNameSyntax.TypeArgumentList);
        }

        private static ArgumentListSyntax GetArguments(InvocationExpressionSyntax invocation)
        {
            var arguments = invocation.ArgumentList;
            var argumentSyntax = invocation.ArgumentList.Arguments.Last();
            var lambdaExpression = argumentSyntax.Expression as LambdaExpressionSyntax;

            if (lambdaExpression == null)
                return arguments;

            var awaitExpression = lambdaExpression.Body as AwaitExpressionSyntax;
            if (awaitExpression == null)
                return arguments;

            var lambdaExpressionWithoutAsyncKeyword = RemoveAsyncKeywordFromLambdaExpression(lambdaExpression, awaitExpression);

            return invocation.ArgumentList.WithArguments(
                invocation.ArgumentList.Arguments
                    .Replace(argumentSyntax, SyntaxFactory.Argument(lambdaExpressionWithoutAsyncKeyword)));
        }

        private static ExpressionSyntax RemoveAsyncKeywordFromLambdaExpression(LambdaExpressionSyntax lambdaExpression,
            AwaitExpressionSyntax awaitExpression)
        {
            if (lambdaExpression is SimpleLambdaExpressionSyntax simpleLambdaExpression)
            {
                return simpleLambdaExpression.ReplaceNode(awaitExpression, awaitExpression.Expression)
                    .WithAsyncKeyword(default(SyntaxToken))
                    .WithLeadingTrivia(simpleLambdaExpression.AsyncKeyword.LeadingTrivia);
            }

            if (lambdaExpression is ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpression)
            {
                return parenthesizedLambdaExpression.ReplaceNode(awaitExpression, awaitExpression.Expression)
                    .WithAsyncKeyword(default(SyntaxToken))
                    .WithLeadingTrivia(parenthesizedLambdaExpression.AsyncKeyword.LeadingTrivia);
            }

            return lambdaExpression;
        }
    }
}