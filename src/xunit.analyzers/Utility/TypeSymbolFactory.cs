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
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.AssemblyFixtureAttribute_V3);

	public static INamedTypeSymbol? Assert(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.Assert);

	public static INamedTypeSymbol? AttributeUsageAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.AttributeUsageAttribute");

	public static INamedTypeSymbol? BigInteger(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Numerics.BigInteger");

	public static INamedTypeSymbol? CancellationToken(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Threading.CancellationToken");

	public static INamedTypeSymbol? ClassDataAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ClassDataAttribute);

	public static INamedTypeSymbol? CollectionAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.CollectionAttribute);

	public static INamedTypeSymbol? CollectionAttributeOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.CollectionAttributeOfT);

	public static INamedTypeSymbol? CollectionDefinitionAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.CollectionDefinitionAttribute);

	public static INamedTypeSymbol? ConfigureAwaitOptions(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Threading.Tasks.ConfigureAwaitOptions");

	public static INamedTypeSymbol? ConfiguredTaskAwaitable(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Runtime.CompilerServices.ConfiguredTaskAwaitable");

	public static INamedTypeSymbol? DataAttribute_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.DataAttribute_V2);

	public static INamedTypeSymbol? DataAttribute_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.DataAttribute_V3);

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
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.IAssemblyInfo_V2);

	public static INamedTypeSymbol? IAsyncEnumerableOfITheoryDataRow(Compilation compilation)
	{
		var iAsyncEnumerableOfT = IAsyncEnumerableOfT(compilation);
		if (iAsyncEnumerableOfT is null)
			return null;

		var iTheoryDataRow = ITheoryDataRow_V3(compilation);
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
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.IAsyncLifetime);

	public static INamedTypeSymbol? IAttributeInfo_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.IAttributeInfo_V2);

	public static INamedTypeSymbol? IClassFixureOfT(Compilation compilation) =>
		 Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.IClassFixtureOfT);

	public static INamedTypeSymbol? ICollection(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Collections.ICollection");

	public static INamedTypeSymbol? ICollectionFixtureOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ICollectionFixtureOfT);

	public static INamedTypeSymbol ICollectionOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_Collections_Generic_ICollection_T);

	public static INamedTypeSymbol? ICriticalNotifyCompletion(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Runtime.CompilerServices.ICriticalNotifyCompletion");

	public static INamedTypeSymbol IDisposable(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_IDisposable);

	public static INamedTypeSymbol? IEnumerableOfITheoryDataRow(Compilation compilation)
	{
		var iTheoryDataRow = ITheoryDataRow_V3(compilation);
		if (iTheoryDataRow is null)
			return null;

		return IEnumerableOfT(compilation).Construct(iTheoryDataRow);
	}

	public static INamedTypeSymbol IEnumerableOfObjectArray(Compilation compilation) =>
		IEnumerableOfT(compilation).Construct(ObjectArray(compilation));

	public static INamedTypeSymbol IEnumerableOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);

	public static INamedTypeSymbol? IMessageSink_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.IMessageSink_V2);

	public static INamedTypeSymbol? IMessageSink_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.IMessageSink_V3);

	public static INamedTypeSymbol? IMessageSinkMessage_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.IMessageSinkMessage_V2);

	public static INamedTypeSymbol? IMethodInfo_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.IMethodInfo_V2);

	public static INamedTypeSymbol? InlineDataAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.InlineDataAttribute);

	public static INamedTypeSymbol? IParameterInfo_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.IParameterInfo_V2);

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
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ISourceInformation_V2);

	public static INamedTypeSymbol? ISourceInformationProvider_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ISourceInformationProvider_V2);

	public static INamedTypeSymbol? ISourceInformationProvider_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ISourceInformationProvider_V3);

	public static INamedTypeSymbol? ITest_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITest_V2);

	public static INamedTypeSymbol? ITest_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITest_V3);

	public static INamedTypeSymbol? ITestAssembly_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestAssembly_V2);

	public static INamedTypeSymbol? ITestAssembly_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestAssembly_V3);

	public static INamedTypeSymbol? ITestCase_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestCase_V2);

	public static INamedTypeSymbol? ITestCase_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestCase_V3);

	public static INamedTypeSymbol? ITestClass_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestClass_V2);

	public static INamedTypeSymbol? ITestClass_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestClass_V3);

	public static INamedTypeSymbol? ITestCollection_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestCollection_V2);

	public static INamedTypeSymbol? ITestCollection_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestCollection_V3);

	public static INamedTypeSymbol? ITestContextAccessor_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestContextAccessor_V3);

	public static INamedTypeSymbol? ITestFramework_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestFramework_V2);

	public static INamedTypeSymbol? ITestFramework_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestFramework_V3);

	public static INamedTypeSymbol? ITestFrameworkDiscoverer_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestFrameworkDiscoverer_V2);

	public static INamedTypeSymbol? ITestFrameworkDiscoverer_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestFrameworkDiscoverer_V3);

	public static INamedTypeSymbol? ITestFrameworkExecutor_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestFrameworkExecutor_V2);

	public static INamedTypeSymbol? ITestFrameworkExecutor_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestFrameworkExecutor_V3);

	public static INamedTypeSymbol? ITestMethod_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestMethod_V2);

	public static INamedTypeSymbol? ITestMethod_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestMethod_V3);

	public static INamedTypeSymbol? ITestOutputHelper_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestOutputHelper_V2);

	public static INamedTypeSymbol? ITestOutputHelper_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITestOutputHelper_V3);

	public static INamedTypeSymbol? ITheoryDataRow_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITheoryDataRow_V3);

	public static INamedTypeSymbol? ITypeInfo_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.ITypeInfo_V2);

	public static INamedTypeSymbol? IXunitSerializable_V2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.IXunitSerializable_V2);

	public static INamedTypeSymbol? IXunitSerializable_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.IXunitSerializable_V3);

	public static INamedTypeSymbol? JsonTypeIDAttribute_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.JsonTypeIDAttribute_V3);

	public static INamedTypeSymbol? ListOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Collections.Generic.List`1");

	public static INamedTypeSymbol? LongLivedMarshalByRefObject_ExecutionV2(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.LongLivedMarshalByRefObject_Execution_V2);

	public static INamedTypeSymbol? LongLivedMarshalByRefObject_RunnerUtility(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.LongLivedMarshalByRefObject_RunnerUtility);

	public static INamedTypeSymbol? MemberDataAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.MemberDataAttribute);

	public static INamedTypeSymbol NullableOfT(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetSpecialType(SpecialType.System_Nullable_T);

	public static IArrayTypeSymbol ObjectArray(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).CreateArrayTypeSymbol(Guard.ArgumentNotNull(compilation).ObjectType);

	public static INamedTypeSymbol? ObsoleteAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.System.ObsoleteAttribute);

	public static INamedTypeSymbol? OptionalAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName("System.Runtime.InteropServices.OptionalAttribute");

	public static INamedTypeSymbol? RegisterXunitSerializerAttribute_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.RegisterXunitSerializerAttribute_V3);

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

	public static INamedTypeSymbol? TestContext_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.TestContext_V3);

	public static INamedTypeSymbol? TheoryAttribute(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.TheoryAttribute);

	public static INamedTypeSymbol? TheoryData(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.TheoryData);

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
			type = compilation.GetTypeByMetadataName(Constants.Types.Xunit.TheoryData + "`" + i.ToString(CultureInfo.InvariantCulture));
			if (type is not null)
				result[i] = type;
		}

		return result;
	}

	public static INamedTypeSymbol? TheoryDataRow_V3(Compilation compilation) =>
		Guard.ArgumentNotNull(compilation).GetTypeByMetadataName(Constants.Types.Xunit.TheoryDataRow_V3);

	// Centralized here so we don't repeat knowledge of how many arities exist
	// (in case we decide to add more later).
	public static Dictionary<int, INamedTypeSymbol> TheoryDataRow_ByGenericArgumentCount_V3(Compilation compilation)
	{
		var result = new Dictionary<int, INamedTypeSymbol>();

		var type = TheoryDataRow_V3(compilation);
		if (type is not null)
			result[0] = type;

		for (int i = 1; i <= 10; i++)
		{
			type = compilation.GetTypeByMetadataName(Constants.Types.Xunit.TheoryDataRow_V3 + "`" + i.ToString(CultureInfo.InvariantCulture));
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
