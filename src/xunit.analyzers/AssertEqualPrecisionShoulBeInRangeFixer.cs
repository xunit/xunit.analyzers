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
    public class AssertEqualPrecisionShoulBeInRangeFixer : CodeFixProvider
    {
        private const string title = "Use precision 0";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.X2016_AssertEqualPrecisionShouldBeInRange.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var precisionArgument = root.FindNode(context.Span).FirstAncestorOrSelf<ArgumentSyntax>();
            if (precisionArgument == null)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument: ct => UseRecommendedPrecision(context.Document, precisionArgument, ct),
                    equivalenceKey: title),
                context.Diagnostics);
        }

        private static async Task<Document> UseRecommendedPrecision(Document document,
            ArgumentSyntax precisionArgument, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

            var fixedPrecisionExpression = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)));

            editor.ReplaceNode(precisionArgument, fixedPrecisionExpression);

            return editor.GetChangedDocument();
        }
    }
}
