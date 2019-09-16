using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Editing;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic), Shared]
    public class TestClassMustBePublicFixer : CodeFixProvider
    {
        const string title = "Make Public";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Descriptors.X1000_TestClassMustBePublic.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: async ct =>
                    {
                        var root = await context.Document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
                        var generator = SyntaxGenerator.GetGenerator(context.Document);
                        var classDeclaration = root.FindNode(context.Span).GetContainingDeclaration(generator, DeclarationKind.Class);
                        return await Actions.ChangeAccessibility(context.Document, classDeclaration, Accessibility.Public, ct).ConfigureAwait(false);
                    },
                    equivalenceKey: title),
                context.Diagnostics);

            return Task.FromResult(true);
        }
    }
}
