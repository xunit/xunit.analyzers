using System;
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

namespace Xunit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class AssertEqualsShouldNotBeUsedFixer : CodeFixProvider
    {
        const string titleTemplate = "Use Assert.{0}";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Constants.Descriptors.X2001_AssertEqualsShouldNotBeUsed.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
            string replacement = null;
            switch (context.Diagnostics.First().Properties[AssertEqualsShouldNotBeUsed.MethodName])
            {
                case AssertEqualsShouldNotBeUsed.EqualsMethod:
                    replacement = "Equal";
                    break;
                case AssertEqualsShouldNotBeUsed.ReferenceEqualsMethod:
                    replacement = "Same";
                    break;
            }

            if (replacement != null && invocation.Expression is MemberAccessExpressionSyntax)
            {
                var title = String.Format(titleTemplate, replacement);
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title,
                        createChangedDocument: ct => UseEqualAsync(context.Document, invocation, replacement, ct),
                        equivalenceKey: title),
                    context.Diagnostics);
            }
        }

        private async Task<Document> UseEqualAsync(Document document, InvocationExpressionSyntax invocation, string replacementMethod, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
            editor.ReplaceNode(memberAccess, memberAccess.WithName((SimpleNameSyntax)editor.Generator.IdentifierName(replacementMethod)));
            return editor.GetChangedDocument();
        }
    }
}
