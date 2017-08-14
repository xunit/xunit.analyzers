using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    internal static class SymbolExtensions
    {
        internal static bool ContainsAttributeType(this ImmutableArray<AttributeData> attributes, INamedTypeSymbol attributeType, bool exactMatch = false)
        {
            return attributes.Any(a => attributeType.IsAssignableFrom(a.AttributeClass, exactMatch));
        }

        internal static INamedTypeSymbol GetAssertType(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName(Constants.Types.XunitAssert);
        }

        internal static INamedTypeSymbol GetDataAttributeType(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName(Constants.Types.XunitSdkDataAttribute);
        }

        internal static INamedTypeSymbol GetFactAttributeType(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName(Constants.Types.XunitFactAttribute);
        }

        internal static INamedTypeSymbol GetIEnumerableOfObjectArrayType(this Compilation compilation)
        {
            var iEnumerableOfT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
            var objectArray = compilation.GetObjectArrayType();
            var iEnumerableOfObjectArray = iEnumerableOfT.Construct(objectArray);
            return iEnumerableOfObjectArray;
        }

        internal static ImmutableArray<ISymbol> GetInheritedAndOwnMembers(this ITypeSymbol symbol, string name = null)
        {
            var builder = ImmutableArray.CreateBuilder<ISymbol>();
            while (symbol != null)
            {
                builder.AddRange(name == null ? symbol.GetMembers() : symbol.GetMembers(name));
                symbol = symbol.BaseType;
            }
            return builder.ToImmutable();
        }

        internal static INamedTypeSymbol GetInlineDataAttributeType(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName(Constants.Types.XunitInlineDataAttribute);
        }

        internal static IArrayTypeSymbol GetObjectArrayType(this Compilation compilation)
        {
            return compilation.CreateArrayTypeSymbol(compilation.ObjectType);
        }

        internal static INamedTypeSymbol GetTheoryAttributeType(this Compilation compilation)
        {
            return compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute);
        }

        internal static bool IsAssignableFrom(this ITypeSymbol targetType, ITypeSymbol sourceType, bool exactMatch = false)
        {
            if (targetType != null)
            {
                while (sourceType != null)
                {
                    if (sourceType.Equals(targetType))
                        return true;

                    if (exactMatch)
                        return false;

                    if (targetType.TypeKind == TypeKind.Interface)
                        return sourceType.AllInterfaces.Any(i => i.Equals(targetType));

                    sourceType = sourceType.BaseType;
                }
            }

            return false;
        }
    }
}
