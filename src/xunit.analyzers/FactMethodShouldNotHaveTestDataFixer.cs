using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class FactMethodShouldNotHaveTestDataFixer : CodeFixProvider
    {
        const string convertToTheoryTitle = "Convert to Theory";
        const string removeDataAttributesTitle = "Remove Data Attributes";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Constants.Descriptors.X1005_FactMethodShouldNotHaveTestData.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();

            context.RegisterCodeFix(
                new ConvertAttributeCodeAction(
                    convertToTheoryTitle,
                    context.Document,
                    methodDeclaration.AttributeLists,
                    fromTypeName: Constants.Types.XunitFactAttribute,
                    toTypeName: Constants.Types.XunitTheoryAttribute),
                context.Diagnostics);
            context.RegisterCodeFix(
                CodeAction.Create(removeDataAttributesTitle,
                ct => RemoveDataAttributesAsync(context.Document, methodDeclaration.AttributeLists, ct),
                equivalenceKey: removeDataAttributesTitle),
                context.Diagnostics);
        }

        async Task<Document> RemoveDataAttributesAsync(Document document, SyntaxList<AttributeListSyntax> attributeLists, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var dataAttributeType = semanticModel.Compilation.GetTypeByMetadataName(Constants.Types.XunitSdkDataAttribute);
            foreach (var attributeList in attributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (dataAttributeType.IsAssignableFrom(semanticModel.GetTypeInfo(attribute, cancellationToken).Type)) {
                        editor.RemoveNode(attribute);
                    }
                }
            }

            return editor.GetChangedDocument();
        }
    }
}
