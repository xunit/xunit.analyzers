using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class TypeSymbolFactory
{
	public static INamedTypeSymbol IEnumerableOfObjectArray(Compilation compilation)
	{
		var iEnumerableOfT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
		var objectArray = ObjectArray(compilation);
		var iEnumerableOfObjectArray = iEnumerableOfT.Construct(objectArray);

		return iEnumerableOfObjectArray;
	}

	public static INamedTypeSymbol? InlineDataAttribute(Compilation compilation) =>
		compilation.GetTypeByMetadataName(Constants.Types.XunitInlineDataAttribute);

	public static IArrayTypeSymbol ObjectArray(Compilation compilation) =>
		compilation.CreateArrayTypeSymbol(compilation.ObjectType);

	public static INamedTypeSymbol? ObsoleteAttribute(Compilation compilation) =>
		compilation.GetTypeByMetadataName(Constants.Types.SystemObsoleteAttribute);

	public static INamedTypeSymbol? TheoryAttribute(Compilation compilation) =>
		compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute);
}
