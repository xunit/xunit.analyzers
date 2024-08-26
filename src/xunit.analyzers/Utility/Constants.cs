namespace Xunit.Analyzers;

public static class Constants
{
	/// <summary>
	/// Argument names for Assert methods
	/// </summary>
	public static class AssertArguments
	{
		public const string Actual = "actual";
		public const string Expected = "expected";
		public const string IgnoreCase = "ignoreCase";
	}

	/// <summary>
	/// Method names from Assert
	/// </summary>
	public static class Asserts
	{
		public const string All = nameof(All);
		public const string AllAsync = nameof(AllAsync);
		public const string Collection = nameof(Collection);
		public const string CollectionAsync = nameof(CollectionAsync);
		public const string Contains = nameof(Contains);
		public const string Distinct = nameof(Distinct);
		public const string DoesNotContain = nameof(DoesNotContain);
		public const string DoesNotMatch = nameof(DoesNotMatch);
		public const string Empty = nameof(Empty);
		public const string EndsWith = nameof(EndsWith);
		public const string Equal = nameof(Equal);
		public const string Equivalent = nameof(Equivalent);
		public const string Fail = nameof(Fail);
		public const string False = nameof(False);
		public const string InRange = nameof(InRange);
		public const string IsAssignableFrom = nameof(IsAssignableFrom);
		public const string IsNotAssignableFrom = nameof(IsNotAssignableFrom);
		public const string IsNotType = nameof(IsNotType);
		public const string IsType = nameof(IsType);
		public const string Matches = nameof(Matches);
		public const string Multiple = nameof(Multiple);
		public const string NotEmpty = nameof(NotEmpty);
		public const string NotEqual = nameof(NotEqual);
		public const string NotInRange = nameof(NotInRange);
		public const string NotNull = nameof(NotNull);
		public const string NotSame = nameof(NotSame);
		public const string NotStrictEqual = nameof(NotStrictEqual);
		public const string Null = nameof(Null);
		public const string ProperSubset = nameof(ProperSubset);
		public const string ProperSuperset = nameof(ProperSuperset);
		public const string PropertyChanged = nameof(PropertyChanged);
		public const string PropertyChangedAsync = nameof(PropertyChangedAsync);
		public const string Raises = nameof(Raises);
		public const string RaisesAny = nameof(RaisesAny);
		public const string RaisesAnyAsync = nameof(RaisesAnyAsync);
		public const string RaisesAsync = nameof(RaisesAsync);
		public const string Same = nameof(Same);
		public const string Single = nameof(Single);
		public const string StartsWith = nameof(StartsWith);
		public const string StrictEqual = nameof(StrictEqual);
		public const string Subset = nameof(Subset);
		public const string Superset = nameof(Superset);
		public const string Throws = nameof(Throws);
		public const string ThrowsAny = nameof(ThrowsAny);
		public const string ThrowsAnyAsync = nameof(ThrowsAnyAsync);
		public const string ThrowsAsync = nameof(ThrowsAsync);
		public const string True = nameof(True);
	}

	/// <summary>
	/// Attribute names (without the Attribute suffix unless otherwise noted)
	/// </summary>
	public static class Attributes
	{
		public const string Fact = nameof(Fact);
		public const string Theory = nameof(Theory);
	}

	/// <summary>
	/// Property names from xUnit.net attributes
	/// </summary>
	public static class AttributeProperties
	{
		public const string DeclaringType = nameof(DeclaringType);
		public const string MemberName = nameof(MemberName);
		public const string MemberType = nameof(MemberType);
	}

	/// <summary>
	/// Properties placed into diagnostics to be picked up by fixes
	/// </summary>
	public static class Properties
	{
		public const string ArgumentValue = nameof(ArgumentValue);
		public const string AssertMethodName = nameof(AssertMethodName);
		public const string DataAttributeTypeName = nameof(DataAttributeTypeName);
		public const string DeclaringType = nameof(DeclaringType);
		public const string IgnoreCase = nameof(IgnoreCase);
		public const string IsStatic = nameof(IsStatic);
		public const string IsStaticMethodCall = nameof(IsStaticMethodCall);
		public const string LiteralValue = nameof(LiteralValue);
		public const string MemberName = nameof(MemberName);
		public const string MethodName = nameof(MethodName);
		public const string NewBaseType = nameof(NewBaseType);
		public const string ParameterArrayStyle = nameof(ParameterArrayStyle);
		public const string ParameterIndex = nameof(ParameterIndex);
		public const string ParameterName = nameof(ParameterName);
		public const string ParameterSpecialType = nameof(ParameterSpecialType);
		public const string Replacement = nameof(Replacement);
		public const string SizeValue = nameof(SizeValue);
		public const string SubstringMethodName = nameof(SubstringMethodName);
		public const string TestClassName = nameof(TestClassName);
		public const string TFixtureDisplayName = nameof(TFixtureDisplayName);
		public const string TFixtureName = nameof(TFixtureName);
		public const string TypeName = nameof(TypeName);
	}

	/// <summary>
	/// Type names as strings for runtime lookup
	/// </summary>
	public static class Types
	{
		public static class System
		{
			public const string ObsoleteAttribute = "System.ObsoleteAttribute";
		}

		public static class Xunit
		{
			public const string AssemblyFixtureAttribute_V3 = "Xunit.AssemblyFixtureAttribute";
			public const string Assert = "Xunit.Assert";
			public const string ClassDataAttribute = "Xunit.ClassDataAttribute";
			public const string CollectionAttribute = "Xunit.CollectionAttribute";
			public const string CollectionAttributeOfT = "Xunit.CollectionAttribute`1";
			public const string CollectionDefinitionAttribute = "Xunit.CollectionDefinitionAttribute";
			public const string DataAttribute_V2 = "Xunit.Sdk.DataAttribute";
			public const string DataAttribute_V3 = "Xunit.v3.DataAttribute";
			public const string FactAttribute = "Xunit.FactAttribute";
			public const string IAssemblyInfo_V2 = "Xunit.Abstractions.IAssemblyInfo";
			public const string IAsyncLifetime = "Xunit.IAsyncLifetime";
			public const string IAttributeInfo_V2 = "Xunit.Abstractions.IAttributeInfo";
			public const string IClassFixtureOfT = "Xunit.IClassFixture`1";
			public const string ICollectionFixtureOfT = "Xunit.ICollectionFixture`1";
			public const string IMessageSink_V2 = "Xunit.Abstractions.IMessageSink";
			public const string IMessageSink_V3 = "Xunit.Sdk.IMessageSink";
			public const string IMessageSinkMessage_V2 = "Xunit.Abstractions.IMessageSinkMessage";
			public const string IMethodInfo_V2 = "Xunit.Abstractions.IMethodInfo";
			public const string IParameterInfo_V2 = "Xunit.Abstractions.IParameterInfo";
			public const string InlineDataAttribute = "Xunit.InlineDataAttribute";
			public const string ISourceInformation_V2 = "Xunit.Abstractions.ISourceInformation";
			public const string ISourceInformationProvider_V2 = "Xunit.Abstractions.ISourceInformationProvider";
			public const string ISourceInformationProvider_V3 = "Xunit.Runner.Common.ISourceInformationProvider";
			public const string ITest_V2 = "Xunit.Abstractions.ITest";
			public const string ITest_V3 = "Xunit.Sdk.ITest";
			public const string ITestAssembly_V2 = "Xunit.Abstractions.ITestAssembly";
			public const string ITestAssembly_V3 = "Xunit.Sdk.ITestAssembly";
			public const string ITestCase_V2 = "Xunit.Abstractions.ITestCase";
			public const string ITestCase_V3 = "Xunit.Sdk.ITestCase";
			public const string ITestClass_V2 = "Xunit.Abstractions.ITestClass";
			public const string ITestClass_V3 = "Xunit.Sdk.ITestClass";
			public const string ITestCollection_V2 = "Xunit.Abstractions.ITestCollection";
			public const string ITestCollection_V3 = "Xunit.Sdk.ITestCollection";
			public const string ITestContextAccessor_V3 = "Xunit.ITestContextAccessor";
			public const string ITestFramework_V2 = "Xunit.Abstractions.ITestFramework";
			public const string ITestFramework_V3 = "Xunit.v3.ITestFramework";
			public const string ITestFrameworkDiscoverer_V2 = "Xunit.Abstractions.ITestFrameworkDiscoverer";
			public const string ITestFrameworkDiscoverer_V3 = "Xunit.v3.ITestFrameworkDiscoverer";
			public const string ITestFrameworkExecutor_V2 = "Xunit.Abstractions.ITestFrameworkExecutor";
			public const string ITestFrameworkExecutor_V3 = "Xunit.v3.ITestFrameworkExecutor";
			public const string ITestMethod_V2 = "Xunit.Abstractions.ITestMethod";
			public const string ITestMethod_V3 = "Xunit.Sdk.ITestMethod";
			public const string ITestOutputHelper_V2 = "Xunit.Abstractions.ITestOutputHelper";
			public const string ITestOutputHelper_V3 = "Xunit.ITestOutputHelper";
			public const string ITheoryDataRow_V3 = "Xunit.ITheoryDataRow";
			public const string ITypeInfo_V2 = "Xunit.Abstractions.ITypeInfo";
			public const string IXunitSerializable_V2 = "Xunit.Abstractions.IXunitSerializable";
			public const string IXunitSerializable_V3 = "Xunit.Sdk.IXunitSerializable";
			public const string JsonTypeIDAttribute_V3 = "Xunit.Sdk.JsonTypeIDAttribute";
			public const string LongLivedMarshalByRefObject_Execution_V2 = "Xunit.LongLivedMarshalByRefObject";
			public const string LongLivedMarshalByRefObject_RunnerUtility = "Xunit.Sdk.LongLivedMarshalByRefObject";
			public const string MemberDataAttribute = "Xunit.MemberDataAttribute";
			public const string TestContext_V3 = "Xunit.TestContext";
			public const string TheoryAttribute = "Xunit.TheoryAttribute";
			public const string TheoryData = "Xunit.TheoryData";
			public const string TheoryDataRow_V3 = "Xunit.TheoryDataRow";
		}
	}
}
