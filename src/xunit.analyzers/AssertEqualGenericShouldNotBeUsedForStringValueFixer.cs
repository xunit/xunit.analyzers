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
    public class AssertEqualGenericShouldNotBeUsedForStringValueFixer : CodeFixProvider
    {
        const string title = "Use string Assert.Equal";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Constants.Descriptors.X2006_AssertEqualGenericShouldNotBeUsedForStringValue.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var syntaxNode = root.FindNode(context.Span);
            var invocation = syntaxNode.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            
            if (invocation.Expression is MemberAccessExpressionSyntax)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title,
                        createChangedDocument: ct => UseNonGenericStringEqualCheckAsync(context.Document, invocation, ct),
                        equivalenceKey: title),
                    context.Diagnostics);
            }
        }

        static async Task<Document> UseNonGenericStringEqualCheckAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
            editor.ReplaceNode(memberAccess,
                memberAccess.WithName(SyntaxFactory.IdentifierName("Equal")));
            return editor.GetChangedDocument();
        }
    }
}
