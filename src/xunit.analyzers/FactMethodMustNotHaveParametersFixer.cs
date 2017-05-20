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
    public class FactMethodMustNotHaveParametersFixer : CodeFixProvider
    {
        const string removeParametersTitle = "Remove Parameters";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.X1001_FactMethodMustNotHaveParameters.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: removeParametersTitle,
                    createChangedDocument: ct => RemoveParametersAsync(context.Document, methodDeclaration.ParameterList, ct),
                    equivalenceKey: removeParametersTitle),
                context.Diagnostics);
        }

        async Task<Document> RemoveParametersAsync(Document document, ParameterListSyntax parameterListSyntax, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            foreach (var parameter in parameterListSyntax.Parameters)
                editor.RemoveNode(parameter);
            return editor.GetChangedDocument();
        }
    }
}
