using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;

namespace Xunit.Analyzers
{
    internal static class SyntaxNodeExtensions
    {
        public static SyntaxNode GetContainingDeclaration(this SyntaxNode node, SyntaxGenerator generator, DeclarationKind declarationKind)
        {
            var declaration = node;
            while (generator.GetDeclarationKind(declaration) != declarationKind)
            {
                declaration = generator.GetDeclaration(declaration.Parent);
                if (declaration is null)
                    return null;
            }

            return declaration;
        }
    }
}
