using System;
using System.Collections.Immutable;
using System.Composition;
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
    public class AssertIsTypeShouldUseGenericOverloadFixer : CodeFixProvider
    {
        const string titleTemplate = "Use Assert.{0}<{1}>";
        const string equivalenceKey = "Use Assert.IsType";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.X2007_AssertIsTypeShouldUseGenericOverload.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var syntaxNode = root.FindNode(context.Span);
            var invocation = syntaxNode.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            
            var methodName = context.Diagnostics[0].Properties[AssertIsTypeShouldUseGenericOverloadType.MethodName];
            var typeName = context.Diagnostics[0].Properties[AssertIsTypeShouldUseGenericOverloadType.TypeName];
            var title = String.Format(titleTemplate, methodName, typeName);
            
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument: ct => RemoveTypeofInvocationAndAddGenericTypeAsync(context.Document, invocation, ct),
                    equivalenceKey: equivalenceKey),
                context.Diagnostics);
        }

        static async Task<Document> RemoveTypeofInvocationAndAddGenericTypeAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
            var typeOfExpression = (TypeOfExpressionSyntax)invocation.ArgumentList.Arguments[0].Expression;
            
            editor.ReplaceNode(invocation,
                invocation
                    .WithExpression(memberAccess
                        .WithName(
                            SyntaxFactory.GenericName(
                                memberAccess.Name.Identifier,
                                SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(typeOfExpression.Type)))))
                    .WithArgumentList(
                        invocation.ArgumentList
                            .WithArguments(invocation.ArgumentList.Arguments.RemoveAt(0))));

            return editor.GetChangedDocument();
        }
    }
}