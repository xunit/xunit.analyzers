using System;
using System.Collections.Immutable;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FactMethodMustNotHaveParametersFixer)), Shared]
    public class FactMethodMustNotHaveParametersFixer : CodeFixProvider
    {
        const string convertToTheoryTitle = "Convert to Theory";
        const string removeParametersTitle = "Remove Parameters";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Constants.Descriptors.X1001_FactMethodMustNotHaveParameters.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        Task<Solution> Foo(CancellationToken arg)
        {
            throw new NotImplementedException();
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: convertToTheoryTitle,
                    createChangedDocument: ct => ConvertToTheoryAsync(context.Document, methodDeclaration, ct),
                    equivalenceKey: convertToTheoryTitle),
                context.Diagnostics);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: removeParametersTitle,
                    createChangedDocument: ct => RemoveParametersAsync(context.Document, methodDeclaration.ParameterList, ct),
                    equivalenceKey: removeParametersTitle),
                context.Diagnostics);
        }

        async Task<Document> ConvertToTheoryAsync(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            foreach (var attributeList in methodDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var s = semanticModel.GetTypeInfo(attribute).Type.ToDisplayString();
                    if (s == Constants.Types.XunitFactAttribute)
                        editor.ReplaceNode(attribute, editor.Generator.Attribute(Constants.Types.XunitTheoryAttribute));
                }
            }
            return editor.GetChangedDocument();
        }

        async Task<Document> RemoveParametersAsync(Document document, ParameterListSyntax parameterListSyntax, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            editor.ReplaceNode(parameterListSyntax, parameterListSyntax.RemoveNodes(parameterListSyntax.Parameters, SyntaxRemoveOptions.KeepNoTrivia));
            return editor.GetChangedDocument();
        }
    }
}
