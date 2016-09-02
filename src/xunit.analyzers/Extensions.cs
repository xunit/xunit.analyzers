using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Analyzers
{
    static class Extensions
    {
        internal static bool ContainsAttributeType(this SyntaxList<AttributeListSyntax> attributeLists, SemanticModel semanticModel, INamedTypeSymbol attributeType, bool exactMatch = false)
        {
            foreach (var attributeList in attributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var type = semanticModel.GetTypeInfo(attribute).Type;
                    while (type != null)
                    {
                        if (type == attributeType)
                            return true;

                        if (exactMatch)
                            return false;

                        type = type.BaseType;
                    }
                }
            }
            return false;
        }
    }
}
