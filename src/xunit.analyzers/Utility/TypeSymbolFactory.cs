using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class TypeSymbolFactory
{
	public static INamedTypeSymbol? Assert(Compilation compilation) =>
		compilation.GetTypeByMetadataName("Xunit.Assert");

	public static INamedTypeSymbol? ClassDataAttribute(Compilation compilation) =>
		compilation.GetTypeByMetadataName("Xunit.ClassDataAttribute");

	public static INamedTypeSymbol? CollectionDefinitionAttribute(Compilation compilation) =>
		compilation.GetTypeByMetadataName("Xunit.CollectionDefinitionAttribute");

	public static INamedTypeSymbol? ConfiguredTaskAwaitable(Compilation compilation) =>
		compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.ConfiguredTaskAwaitable");

	public static INamedTypeSymbol? DataAttribute(Compilation compilation) =>
		compilation.GetTypeByMetadataName(Constants.Types.Xunit.Sdk.DataAttribute);

	public static INamedTypeSymbol? FactAttribute(Compilation compilation) =>
		compilation.GetTypeByMetadataName(Constants.Types.Xunit.FactAttribute);

	public static INamedTypeSymbol? IAsyncLifetime(Compilation compilation) =>
		compilation.GetTypeByMetadataName("Xunit.IAsyncLifetime");

	public static INamedTypeSymbol? IClassFixureOfT(Compilation compilation) =>
		 compilation.GetTypeByMetadataName("Xunit.IClassFixture`1");

	public static INamedTypeSymbol? ICollection(Compilation compilation) =>
		compilation.GetTypeByMetadataName("System.Collections.ICollection");

	public static INamedTypeSymbol? ICollectionFixtureOfT(Compilation compilation) =>
		compilation.GetTypeByMetadataName("Xunit.ICollectionFixture`1");

	public static INamedTypeSymbol ICollectionOfT(Compilation compilation) =>
		compilation.GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T);

	public static INamedTypeSymbol IDisposable(Compilation compilation) =>
		compilation.GetSpecialType(SpecialType.System_IDisposable);

	public static INamedTypeSymbol IEnumerableOfObjectArray(Compilation compilation)
	{
		var iEnumerableOfT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
		var objectArray = ObjectArray(compilation);
		var iEnumerableOfObjectArray = iEnumerableOfT.Construct(objectArray);

		return iEnumerableOfObjectArray;
	}

	public static INamedTypeSymbol? IEnumerableOfITheoryDataRow(Compilation compilation)
	{
		var iTheoryDataRow = ITheoryDataRow(compilation);
		if (iTheoryDataRow is null)
			return null;

		var iEnumerableOfT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
		return iEnumerableOfT.Construct(iTheoryDataRow);
	}

	public static INamedTypeSymbol? InlineDataAttribute(Compilation compilation) =>
		compilation.GetTypeByMetadataName("Xunit.InlineDataAttribute");

	public static INamedTypeSymbol IReadOnlyCollectionOfT(Compilation compilation) =>
		compilation.GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyCollection_T);

	public static INamedTypeSymbol? ITestCase(Compilation compilation) =>
		compilation.GetTypeByMetadataName("Xunit.Abstractions.ITestCase");

	public static INamedTypeSymbol? ITheoryDataRow(Compilation compilation) =>
		compilation.GetTypeByMetadataName("Xunit.ITheoryDataRow");

	public static INamedTypeSymbol? IXunitSerializable(Compilation compilation) =>
		compilation.GetTypeByMetadataName("Xunit.Abstractions.IXunitSerializable");

	public static INamedTypeSymbol? LongLivedMarshalByRefObject(Compilation compilation) =>
		compilation.GetTypeByMetadataName(Constants.Types.Xunit.LongLivedMarshalByRefObject);

	public static INamedTypeSymbol? MemberDataAttribute(Compilation compilation) =>
		compilation.GetTypeByMetadataName("Xunit.MemberDataAttribute");

	public static INamedTypeSymbol NullableOfT(Compilation compilation) =>
		compilation.GetSpecialType(SpecialType.System_Nullable_T);

	public static IArrayTypeSymbol ObjectArray(Compilation compilation) =>
		compilation.CreateArrayTypeSymbol(compilation.ObjectType);

	public static INamedTypeSymbol? ObsoleteAttribute(Compilation compilation) =>
		compilation.GetTypeByMetadataName(Constants.Types.System.ObsoleteAttribute);

	public static INamedTypeSymbol? OptionalAttribute(Compilation compilation) =>
		compilation.GetTypeByMetadataName("System.Runtime.InteropServices.OptionalAttribute");

	public static INamedTypeSymbol? Task(Compilation compilation) =>
		compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");

	public static INamedTypeSymbol? TaskOfT(Compilation compilation) =>
		compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");

	public static INamedTypeSymbol? TheoryAttribute(Compilation compilation) =>
		compilation.GetTypeByMetadataName(Constants.Types.Xunit.TheoryAttribute);

	public static INamedTypeSymbol? ValueTask(Compilation compilation) =>
		compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");

	public static INamedTypeSymbol? ValueTaskOfT(Compilation compilation) =>
		compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

	public static INamedTypeSymbol Void(Compilation compilation) =>
		compilation.GetSpecialType(SpecialType.System_Void);
}
