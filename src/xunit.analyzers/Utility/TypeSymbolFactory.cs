using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public static class TypeSymbolFactory
{
	public static INamedTypeSymbol? Action(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Action");

	public static INamedTypeSymbol? Action(
		Compilation compilation,
		int arity = 1) =>
			Guard.ArgumentNotNull(compilation).GetTypeByMetadataName($"System.Action`{ValidateArity(arity, min: 1, max: 16)}");

	public static INamedTypeSymbol? ArraySegmentOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.ArraySegment`1");

	public static INamedTypeSymbol? AssemblyFixtureAttribute_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.AssemblyFixtureAttribute");

	public static INamedTypeSymbol? Assert(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Assert");

	public static INamedTypeSymbol? AttributeUsageAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.AttributeUsageAttribute");

	public static INamedTypeSymbol? BigInteger(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Numerics.BigInteger");

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

	public static INamedTypeSymbol? DateOnly(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.DateOnly");

	public static INamedTypeSymbol? DateTimeOffset(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.DateTimeOffset");

	public static INamedTypeSymbol? DictionaryofTKeyTValue(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Collections.Generic.Dictionary`2");

	public static INamedTypeSymbol? FactAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.FactAttribute);

	public static INamedTypeSymbol? Func(
		Compilation compilation,
		int arity = 1) =>
			Guard.ArgumentNotNull(compilation).GetTypeByMetadataName($"System.Func`{ValidateArity(arity, min: 1, max: 17)}");

	public static INamedTypeSymbol? IAssemblyInfo_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.IAssemblyInfo");

	public static INamedTypeSymbol? IAsyncEnumerableOfITheoryDataRow(Compilation compilation)
	{
		var iAsyncEnumerableOfT = IAsyncEnumerableOfT(compilation);
		if (iAsyncEnumerableOfT is null)
			return null;

		var iTheoryDataRow = ITheoryDataRow(compilation);
		if (iTheoryDataRow is null)
			return null;

		return iAsyncEnumerableOfT.Construct(iTheoryDataRow);
	}

	public static INamedTypeSymbol? IAsyncEnumerableOfObjectArray(Compilation compilation)
	{
		var iAsyncEnumerableOfT = IAsyncEnumerableOfT(compilation);
		if (iAsyncEnumerableOfT is null)
			return null;

		var objectArray = ObjectArray(compilation);
		return iAsyncEnumerableOfT.Construct(objectArray);
	}

	public static INamedTypeSymbol? IAsyncEnumerableOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Collections.Generic.IAsyncEnumerable`1");

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

	public static INamedTypeSymbol? IEnumerableOfITheoryDataRow(Compilation compilation)
	{
		var iTheoryDataRow = ITheoryDataRow(compilation);
		if (iTheoryDataRow is null)
			return null;

		return IEnumerableOfT(compilation).Construct(iTheoryDataRow);
	}

	public static INamedTypeSymbol IEnumerableOfObjectArray(Compilation compilation) =>
		IEnumerableOfT(compilation).Construct(ObjectArray(compilation));

	public static INamedTypeSymbol IEnumerableOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);

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

	public static INamedTypeSymbol? ITypeInfo_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.ITypeInfo");

	public static INamedTypeSymbol? IXunitSerializable_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Abstractions.IXunitSerializable");

	public static INamedTypeSymbol? IXunitSerializable_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.Sdk.IXunitSerializable");

	public static INamedTypeSymbol? ListOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Collections.Generic.List`1");

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

	public static INamedTypeSymbol String(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_String);

	public static INamedTypeSymbol? StringValues(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Microsoft.Extensions.Primitives.StringValues");

	public static INamedTypeSymbol? Task(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Threading.Tasks.Task");

	public static INamedTypeSymbol? TaskOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Threading.Tasks.Task`1");

	public static INamedTypeSymbol? TheoryAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.TheoryAttribute);

	public static INamedTypeSymbol? TheoryData(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.TheoryData");

	// Centralized here so we don't repeat knowledge of how many arities exist
	// (in case we decide to add more later).
	public static Dictionary<int, INamedTypeSymbol> TheoryData_ByGenericArgumentCount(Compilation compilation)
	{
		var result = new Dictionary<int, INamedTypeSymbol>();

		var type = TheoryData(compilation);
		if (type is not null)
			result[0] = type;

		for (int i = 1; i <= 10; i++)
		{
			type = compilation.GetTypeByMetadataName("Xunit.TheoryData`" + i.ToString(CultureInfo.InvariantCulture));
			if (type is not null)
				result[i] = type;
		}

		return result;
	}

	// Namespace fallback for builds before TheoryDataRow was moved from Xunit.Sdk to Xunit, should
	// eventually be able to get rid of this fallback once v3 goes 1.0.
	public static INamedTypeSymbol? TheoryDataRow(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("Xunit.TheoryDataRow")
			?? compilation.GetTypeByMetadataName("Xunit.Sdk.TheoryDataRow");

	// Centralized here so we don't repeat knowledge of how many arities exist
	// (in case we decide to add more later).
	public static Dictionary<int, INamedTypeSymbol> TheoryDataRow_ByGenericArgumentCount(Compilation compilation)
	{
		var result = new Dictionary<int, INamedTypeSymbol>();

		var type = TheoryDataRow(compilation);
		if (type is not null)
			result[0] = type;

		for (int i = 1; i <= 10; i++)
		{
			type = compilation.GetTypeByMetadataName("Xunit.TheoryDataRow`" + i.ToString(CultureInfo.InvariantCulture));
			if (type is not null)
				result[i] = type;
		}

		return result;
	}

	public static INamedTypeSymbol? TimeOnly(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.TimeOnly");

	public static INamedTypeSymbol? TimeSpan(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.TimeSpan");

	public static INamedTypeSymbol? Type(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Type");

	static int ValidateArity(
		int arity,
		int min,
		int max)
	{
		if (arity >= min && arity <= max)
			return arity;

		throw new ArgumentOutOfRangeException(nameof(arity), $"Arity {arity} must be between {min} and {max}.");
	}

	public static INamedTypeSymbol? ValueTask(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Threading.Tasks.ValueTask");

	public static INamedTypeSymbol? ValueTaskOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

	public static INamedTypeSymbol Void(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_Void);
}
