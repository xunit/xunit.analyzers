using System.Globalization;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public static class TypeSymbolFactory
{
	public static INamedTypeSymbol? AssemblyFixtureAttribute_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.AssemblyFixtureAttribute");

	public static INamedTypeSymbol? Assert(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Assert");

	public static INamedTypeSymbol? ClassDataAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.ClassDataAttribute");

	public static INamedTypeSymbol? CollectionAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.CollectionAttribute");

	public static INamedTypeSymbol? CollectionDefinitionAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.CollectionDefinitionAttribute");

	public static INamedTypeSymbol? ConfigureAwaitOptions(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Threading.Tasks.ConfigureAwaitOptions");

	public static INamedTypeSymbol? ConfiguredTaskAwaitable(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Runtime.CompilerServices.ConfiguredTaskAwaitable");

	public static INamedTypeSymbol? DataAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.Sdk.DataAttribute);

	public static INamedTypeSymbol? FactAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.FactAttribute);

	public static INamedTypeSymbol? IAssemblyInfo_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.IAssemblyInfo");

	public static INamedTypeSymbol? IAsyncLifetime(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.IAsyncLifetime");

	public static INamedTypeSymbol? IAttributeInfo_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.IAttributeInfo");

	public static INamedTypeSymbol? IClassFixureOfT(Compilation compilation) =>
		 Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.IClassFixture`1");

	public static INamedTypeSymbol? ICollection(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Collections.ICollection");

	public static INamedTypeSymbol? ICollectionFixtureOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.ICollectionFixture`1");

	public static INamedTypeSymbol ICollectionOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T);

	public static INamedTypeSymbol? ICriticalNotifyCompletion(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Runtime.CompilerServices.ICriticalNotifyCompletion");

	public static INamedTypeSymbol IDisposable(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_IDisposable);

	public static INamedTypeSymbol IEnumerableOfObjectArray(Compilation compilation)
	{
		var iEnumerableOfT = Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
		var objectArray = ObjectArray(compilation);
		var iEnumerableOfObjectArray = iEnumerableOfT.Construct(objectArray);

		return iEnumerableOfObjectArray;
	}

	public static INamedTypeSymbol? IEnumerableOfITheoryDataRow(Compilation compilation)
	{
		var iTheoryDataRow = ITheoryDataRow(compilation);
		if (iTheoryDataRow is null)
			return null;

		var iEnumerableOfT = Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
		return iEnumerableOfT.Construct(iTheoryDataRow);
	}

	public static INamedTypeSymbol? IMessageSink_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.IMessageSink");

	public static INamedTypeSymbol? IMessageSinkMessage_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.IMessageSinkMessage");

	public static INamedTypeSymbol? IMethodInfo_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.IMethodInfo");

	public static INamedTypeSymbol? InlineDataAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.InlineDataAttribute");

	public static INamedTypeSymbol? IParameterInfo_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.IParameterInfo");

	public static INamedTypeSymbol? IValueTaskSource(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Threading.Tasks.Sources.IValueTaskSource");

	public static INamedTypeSymbol? IValueTaskSourceOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Threading.Tasks.Sources.IValueTaskSource`1");

	public static INamedTypeSymbol IReadOnlyCollectionOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_Collections_Generic_IReadOnlyCollection_T);

	public static INamedTypeSymbol? IReadOnlySetOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Collections.Generic.IReadOnlySet`1");

	public static INamedTypeSymbol? ISetOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Collections.Generic.ISet`1");

	public static INamedTypeSymbol? ISourceInformation_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ISourceInformation");

	public static INamedTypeSymbol? ISourceInformationProvider_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ISourceInformationProvider");

	public static INamedTypeSymbol? ITest_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ITest");

	public static INamedTypeSymbol? ITestAssembly_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ITestAssembly");

	public static INamedTypeSymbol? ITestCase_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ITestCase");

	public static INamedTypeSymbol? ITestClass_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ITestClass");

	public static INamedTypeSymbol? ITestCollection_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ITestCollection");

	public static INamedTypeSymbol? ITestContextAccessor_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.ITestContextAccessor");

	public static INamedTypeSymbol? ITestFramework_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ITestFramework");

	public static INamedTypeSymbol? ITestFrameworkDiscoverer_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ITestFrameworkDiscoverer");

	public static INamedTypeSymbol? ITestFrameworkExecutor_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ITestFrameworkExecutor");

	public static INamedTypeSymbol? ITestMethod_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ITestMethod");

	public static INamedTypeSymbol? ITestOutputHelper_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ITestOutputHelper");

	// TODO: This will need to be updated when v3 names are finalized
	public static INamedTypeSymbol? ITestOutputHelper_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.v3._ITestOutputHelper");

	public static INamedTypeSymbol? ITheoryDataRow(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.ITheoryDataRow");

	public static INamedTypeSymbol? TheoryDataN(Compilation compilation, int n) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.TheoryData`" + n.ToString(CultureInfo.InvariantCulture));

	public static INamedTypeSymbol? ITypeInfo_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ITypeInfo");

	public static INamedTypeSymbol? IXunitSerializable_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.IXunitSerializable");

	public static INamedTypeSymbol? LongLivedMarshalByRefObject_ExecutionV2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.LongLivedMarshalByRefObject);

	public static INamedTypeSymbol? LongLivedMarshalByRefObject_RunnerUtilityV2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.Sdk.LongLivedMarshalByRefObject);

	public static INamedTypeSymbol? MemberDataAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.MemberDataAttribute");

	public static INamedTypeSymbol NullableOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_Nullable_T);

	public static IArrayTypeSymbol ObjectArray(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).CreateArrayTypeSymbol(Guard.ArgumentNotNull(compilation).ObjectType);

	public static INamedTypeSymbol? ObsoleteAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.System.ObsoleteAttribute);

	public static INamedTypeSymbol? OptionalAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Runtime.InteropServices.OptionalAttribute");

	public static INamedTypeSymbol? SortedSetOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Collections.Generic.SortedSet`1");

	public static INamedTypeSymbol? Task(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Threading.Tasks.Task");

	public static INamedTypeSymbol? TaskOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Threading.Tasks.Task`1");

	public static INamedTypeSymbol? TheoryAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.TheoryAttribute);

	public static INamedTypeSymbol? ValueTask(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Threading.Tasks.ValueTask");

	public static INamedTypeSymbol? ValueTaskOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

	public static INamedTypeSymbol Void(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_Void);
}
