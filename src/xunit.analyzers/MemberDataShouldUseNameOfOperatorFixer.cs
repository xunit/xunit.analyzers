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
    public class MemberDataShouldUseNameOfOperatorFixer : CodeFixProvider
    {
        const string title = "Use nameof";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Constants.Descriptors.X1014_MemberDataShouldUseNameOfOperator.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

            var memberNameExpression = root.FindNode(context.Span).FirstAncestorOrSelf<LiteralExpressionSyntax>();
            INamedTypeSymbol memberType = null;
            string memberTypeName = null;
            if (context.Diagnostics.First().Properties.TryGetValue(MemberDataShouldUseNameOfOperator.MemberType, out memberTypeName))
                memberType = semanticModel.Compilation.GetTypeByMetadataName(memberTypeName);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    createChangedDocument: ct => UseNameof(context.Document, memberNameExpression, memberType, ct),
                    equivalenceKey: title),
                context.Diagnostics);
        }

        private async Task<Document> UseNameof(Document document, LiteralExpressionSyntax memberNameExpression, INamedTypeSymbol memberType, CancellationToken cancellationToken)
        {
            var documentEditor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            documentEditor.ReplaceNode(memberNameExpression, (node, generator) =>
            {
                var nameofParam = generator.IdentifierName(memberNameExpression.Token.ValueText);
                if (memberType != null)
                    nameofParam = generator.MemberAccessExpression(generator.TypeExpression(memberType), nameofParam);
                return generator.InvocationExpression(generator.IdentifierName("nameof"), nameofParam);
            });
            return documentEditor.GetChangedDocument();
        }
    }
}
