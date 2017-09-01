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
    public class TestMethodShouldNotHaveReturnTypeFixer : CodeFixProvider
    {
        private const string TitleTemplate = "Change return type to {0}";

        private static PredefinedTypeSyntax VoidSyntax { get; } = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(Descriptors.X1027_TestMethodShouldNotHaveReturnType.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();

            bool isAsync = methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var taskType = semanticModel.Compilation.GetTypeByMetadataName(Constants.Types.SystemThreadingTasksTask);

            var title = string.Format(TitleTemplate, "void");
            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    ct => ChangeReturnTypeAsync(context.Document, methodDeclaration, VoidSyntax, ct),
                    equivalenceKey: title),
                context.Diagnostics);

            if (isAsync && taskType != null)
            {
                int position = methodDeclaration.ReturnType.SpanStart;
                var taskSyntax = GetNameSyntax(taskType, semanticModel, position);

                title = string.Format(TitleTemplate, nameof(Task));
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title,
                        ct => ChangeReturnTypeAsync(context.Document, methodDeclaration, taskSyntax, ct),
                        equivalenceKey: title),
                    context.Diagnostics);
            }
        }

        private async Task<Document> ChangeReturnTypeAsync(Document document, MethodDeclarationSyntax methodDeclaration, TypeSyntax newReturnType, CancellationToken ct)
        {
            var editor = await DocumentEditor.CreateAsync(document, ct).ConfigureAwait(false);
            editor.ReplaceNode(methodDeclaration.ReturnType, newReturnType);
            return editor.GetChangedDocument();
        }

        private static IdentifierNameSyntax GetNameSyntax(INamedTypeSymbol namedType, SemanticModel semanticModel, int position)
        {
            var typeName = namedType.ToMinimalDisplayString(semanticModel, position);
            return SyntaxFactory.IdentifierName(SyntaxFactory.Identifier(typeName));
        }
    }
}
