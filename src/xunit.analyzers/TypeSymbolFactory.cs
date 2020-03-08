using Microsoft.CodeAnalysis;

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
			=> compilation.CreateArrayTypeSymbol(compilation.ObjectType);

		internal static INamedTypeSymbol GetInlineDataType(Compilation compilation)
			=> compilation.GetTypeByMetadataName(Constants.Types.XunitInlineDataAttribute);

		internal static INamedTypeSymbol GetTheoryType(Compilation compilation)
			=> compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute);

		internal static INamedTypeSymbol GetObsoleteAttributeType(Compilation compilation)
			=> compilation.GetTypeByMetadataName(Constants.Types.SystemObsoleteAttribute);
	}
}
