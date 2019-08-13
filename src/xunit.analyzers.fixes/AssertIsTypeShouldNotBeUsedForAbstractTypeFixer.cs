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
    public class AssertIsTypeShouldNotBeUsedForAbstractTypeFixer : CodeFixProvider
    {
        private const string Title = "Use Assert." + IsAssignableFrom;

        private const string IsAssignableFrom = "IsAssignableFrom";

        private const string IsType = "IsType";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.X2018_AssertIsTypeShouldNotBeUsedForAbstractType.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var invocation = root.FindNode(context.Span).FirstAncestorOrSelf<InvocationExpressionSyntax>();
            var methodName = invocation.GetSimpleName()?.Identifier.Text;
            if (methodName == IsType)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        Title,
                        createChangedDocument: ct => UseIsAssignableFromAsync(context.Document, invocation, ct),
                        equivalenceKey: Title),
                    context.Diagnostics);
            }
        }

        private static async Task<Document> UseIsAssignableFromAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var simpleName = invocation.GetSimpleName();
            editor.ReplaceNode(simpleName, simpleName.WithIdentifier(SyntaxFactory.Identifier(IsAssignableFrom)));
            return editor.GetChangedDocument();
        }
    }
}
