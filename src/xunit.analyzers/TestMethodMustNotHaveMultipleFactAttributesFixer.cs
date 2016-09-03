using System.Collections.Generic;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TestClassMustBePublicFixer)), Shared]
    public class TestMethodMustNotHaveMultipleFactAttributesFixer : CodeFixProvider
    {
        const string genericTitle = "Keep {0} Attribute";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Constants.Descriptors.X1002_TestMethodMustNotHaveMultipleFactAttributes.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();

            var attributeTypes = context.Diagnostics.First().Properties.Keys.ToList();
            foreach (var attributeType in attributeTypes)
            {
                string simpleName = GetAttributeSimpleName(attributeType);
                string title = string.Format(genericTitle, simpleName);
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: ct => RemoveAttributesAsync(context.Document, methodDeclaration, attributeTypes, attributeType, ct),
                        equivalenceKey: title),
                    context.Diagnostics);
            }
        }

        static string GetAttributeSimpleName(string attributeType)
        {
            string simpleName = attributeType;
            if (simpleName.Contains("."))
                simpleName = simpleName.Substring(attributeType.LastIndexOf('.') + 1);
            const string nameSuffix = "Attribute";
            if (simpleName.EndsWith(nameSuffix))
                simpleName = simpleName.Substring(0, simpleName.Length - nameSuffix.Length);
            return simpleName;
        }

        async Task<Document> RemoveAttributesAsync(Document document, MethodDeclarationSyntax methodDeclaration, IReadOnlyList<string> attributeTypesToConsider, string attributeTypeToKeep, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            bool oneKept = false;
            foreach (var attributeList in methodDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeType = semanticModel.GetTypeInfo(attribute, cancellationToken).Type.ToDisplayString();
                    if (attributeTypesToConsider.Contains(attributeType))
                    {
                        if (attributeType == attributeTypeToKeep && !oneKept)
                        {
                            oneKept = true;
                            continue;
                        }
                        editor.RemoveNode(attribute);
                    }
                }
            }
            return editor.GetChangedDocument();
        }
    }
}