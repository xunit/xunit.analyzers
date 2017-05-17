using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers.CodeActions
{
    public static class Actions
    {
        public static async Task<Document> ChangeAccessibility(Document document, SyntaxNode declaration, Accessibility accessibility, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            editor.SetAccessibility(declaration, accessibility);
            return editor.GetChangedDocument();
        }

        public static async Task<Solution> ChangeMemberAccessiblity(Solution solution, ISymbol memberSymbol, Accessibility accessibility, CancellationToken cancellationToken)
        {
            var editor = SymbolEditor.Create(solution);
            await editor.EditAllDeclarationsAsync(memberSymbol, (docEditor, syntaxNode) =>
            {
                docEditor.SetAccessibility(syntaxNode, accessibility);
            }, cancellationToken).ConfigureAwait(false);
            return editor.ChangedSolution;
        }

        public static async Task<Solution> ChangeMemberStaticModifier(Solution solution, ISymbol memberSymbol, bool isStatic, CancellationToken cancellationToken)
        {
            var editor = SymbolEditor.Create(solution);
            await editor.EditAllDeclarationsAsync(memberSymbol, (docEditor, syntaxNode) =>
            {
                var newMods = DeclarationModifiers.From(memberSymbol).WithIsStatic(isStatic);
                if (memberSymbol is IPropertySymbol propertySymbol && propertySymbol.IsReadOnly)
                {
                    // Looks like there's a bug in Roslym where SetModifiers will apply the 'readonly'
                    // keyword to a property that only has a getter, which produces illegal syntax.
                    newMods = newMods.WithIsReadOnly(false);
                }
                docEditor.SetModifiers(syntaxNode, newMods);
            }, cancellationToken).ConfigureAwait(false);
            return editor.ChangedSolution;
        }

        public static async Task<Solution> ChangeMemberType(Solution solution, ISymbol memberSymbol, ITypeSymbol type, CancellationToken cancellationToken)
        {
            var editor = SymbolEditor.Create(solution);
            await editor.EditAllDeclarationsAsync(memberSymbol, (docEditor, syntaxNode) =>
            {
                docEditor.SetType(syntaxNode, docEditor.Generator.TypeExpression(type));
            }, cancellationToken).ConfigureAwait(false);
            return editor.ChangedSolution;
        }

        public static async Task<Document> RemoveNodeAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            editor.RemoveNode(node);
            return editor.GetChangedDocument();
        }
    }
}
