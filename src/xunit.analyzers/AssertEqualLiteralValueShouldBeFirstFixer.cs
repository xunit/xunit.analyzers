﻿using System.Collections.Immutable;
using System.Composition;
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
    public class AssertEqualLiteralValueShouldBeFirstFixer : CodeFixProvider
    {
        const string title = "Swap Arguments";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Constants.Descriptors.X2000_AssertEqualLiteralValueShouldBeFirst.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument: ct => SwapArgumentsAsync(context.Document, invocation, ct),
                    equivalenceKey: title),
                context.Diagnostics);
        }

        private async Task<Document> SwapArgumentsAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var firstArg = invocation.ArgumentList.Arguments[0];
            var secondArg = invocation.ArgumentList.Arguments[1];
            editor.RemoveNode(firstArg);
            editor.InsertAfter(secondArg, firstArg);
            return editor.GetChangedDocument();
        }
    }
}
