using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    class TypeSymbolFactory
    {
        internal static INamedTypeSymbol IEnumerableOfObjectArray(Compilation compilation)
        {
            var iEnumerableOfT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
            var objectArray = compilation.CreateArrayTypeSymbol(compilation.ObjectType);
            var iEnumerableOfObjectArray = iEnumerableOfT.Construct(objectArray);
            return iEnumerableOfObjectArray;
        }
    }
}
