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
    public class TestClassMustBePublicFixer : CodeFixProvider
    {
        const string title = "Make Public";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Constants.Descriptors.X1000_TestClassMustBePublic.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var classDeclaration = (ClassDeclarationSyntax)root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: ct => MakePublicAsync(context.Document, classDeclaration, ct),
                    equivalenceKey: title),
                diagnostic);
        }

        async Task<Document> MakePublicAsync(Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            editor.SetAccessibility(classDeclaration, Accessibility.Public);
            return editor.GetChangedDocument();
        }
    }
}