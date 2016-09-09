using System.Collections.Immutable;
using System.Linq;
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
                    if (attributeType.IsAssignableFrom(type, exactMatch))
                        return true;
                }
            }
            return false;
        }

        internal static bool ContainsAttributeType(this ImmutableArray<AttributeData> attributes, INamedTypeSymbol attributeType, bool exactMatch = false)
        {
            return attributes.Any(a => attributeType.IsAssignableFrom(a.AttributeClass, exactMatch));
        }

        internal static bool IsAssignableFrom(this ITypeSymbol targetType, ITypeSymbol sourceType, bool exactMatch = false)
        {
            if (targetType != null)
            {
                while (sourceType != null)
                {
                    if (sourceType == targetType)
                        return true;

                    if (exactMatch)
                        return false;

                    if (targetType.TypeKind == TypeKind.Interface)
                        return sourceType.AllInterfaces.Contains(targetType);

                    sourceType = sourceType.BaseType;
                }
            }

            return false;
        }
    }
}
