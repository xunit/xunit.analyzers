using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Analyzers.CodeActions;

namespace Xunit.Analyzers.FixProviders
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class RemoveMethodParameterFix : CodeFixProvider
    {
        const string title = "Remove Parameter";

        public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
            Constants.Descriptors.X1022_TheoryMethodCannotHaveParameterArray.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var parameter = root.FindNode(context.Span).FirstAncestorOrSelf<ParameterSyntax>();

            context.RegisterCodeFix(
                CodeAction.Create(title, ct => Actions.RemoveNodeAsync(context.Document, parameter, ct), title),
                context.Diagnostics);
        }
    }
}
