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
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class PublicMethodShouldBeMarkedAsTestFixer : CodeFixProvider
    {
        const string makeInternal = "Make Internal";
        const string convertToTheory = "Convert to Theory";
        const string convertToFact = "Convert to Fact";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Constants.Descriptors.X1013_PublicMethodShouldBeMarkedAsTest.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();

            var looksLikeTheory = methodDeclaration.ParameterList.Parameters.Any();
            var convertTitle = looksLikeTheory ? convertToTheory : convertToFact;
            var convertType = looksLikeTheory ? Constants.Types.XunitTheoryAttribute : Constants.Types.XunitFactAttribute;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: convertTitle,
                    createChangedDocument: ct => Foo(context.Document, methodDeclaration, convertType, ct),
                    equivalenceKey: convertTitle),
                context.Diagnostics);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: makeInternal,
                    createChangedDocument: ct => MakePublicAsync(context.Document, methodDeclaration, ct),
                    equivalenceKey: makeInternal),
                context.Diagnostics);
        }

        async Task<Document> Foo(Document document, MethodDeclarationSyntax methodDeclaration, string type, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            editor.AddAttribute(methodDeclaration, editor.Generator.Attribute(type));
            return editor.GetChangedDocument();
        }

        async Task<Document> MakePublicAsync(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            editor.SetAccessibility(methodDeclaration, Accessibility.Internal);
            return editor.GetChangedDocument();
        }
    }
}