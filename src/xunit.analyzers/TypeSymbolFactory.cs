﻿using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    class TypeSymbolFactory
    {
        internal static INamedTypeSymbol IEnumerableOfObjectArray(Compilation compilation)
        {
            var iEnumerableOfT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
            var objectArray = GetObjectArrayType(compilation);
            var iEnumerableOfObjectArray = iEnumerableOfT.Construct(objectArray);
            return iEnumerableOfObjectArray;
        }

        internal static IArrayTypeSymbol GetObjectArrayType(Compilation compilation)
        {
            return compilation.CreateArrayTypeSymbol(compilation.ObjectType);
        }

        internal static INamedTypeSymbol GetInlineDataType(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName(Constants.Types.XunitInlineDataAttribute);
        }

        internal static INamedTypeSymbol GetTheoryType(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute);
        }
    }
}
