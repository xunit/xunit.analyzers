using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class TheoryMethodShouldHaveParametersFixer : CodeFixProvider
    {
        const string title = "Convert to Fact";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(Constants.Descriptors.X1006_TheoryMethodShouldHaveParameters.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var methodDeclaration = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();

            context.RegisterCodeFix(
                new ConvertAttributeCodeAction(
                    title,
                    context.Document,
                    methodDeclaration.AttributeLists,
                    fromTypeName: Constants.Types.XunitTheoryAttribute,
                    toTypeName: Constants.Types.XunitFactAttribute),
                context.Diagnostics);
        }
    }
}
