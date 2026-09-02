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
		public const string SkipExceptions = nameof(SkipExceptions);
		public const string SkipType = nameof(SkipType);
		public const string SkipUnless = nameof(SkipUnless);
		public const string SkipWhen = nameof(SkipWhen);
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
		public const string IsCtorObsolete = nameof(IsCtorObsolete);
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
		public const string RederivationSpanLength = nameof(RederivationSpanLength);
		public const string RederivationSpanStart = nameof(RederivationSpanStart);
		public const string Replacement = nameof(Replacement);
		public const string SizeValue = nameof(SizeValue);
		public const string SubstringMethodName = nameof(SubstringMethodName);
		public const string TestClassName = nameof(TestClassName);
		public const string TFixtureDisplayName = nameof(TFixtureDisplayName);
		public const string TFixtureName = nameof(TFixtureName);
		public const string TypeName = nameof(TypeName);
		public const string UseExactMatch = nameof(UseExactMatch);
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
			public const string AssemblyFixtureAttributeOfT_V3 = "Xunit.AssemblyFixtureAttribute`1";
			public const string Assert = "Xunit.Assert";
			public const string BeforeAfterTestAttribute_V2 = "Xunit.Sdk.BeforeAfterTestAttribute";
			public const string BeforeAfterTestAttribute_V3 = "Xunit.v3.BeforeAfterTestAttribute";
			public const string ClassDataAttribute = "Xunit.ClassDataAttribute";
			public const string ClassDataAttributeOfT_V3 = "Xunit.ClassDataAttribute`1";
			public const string CollectionAttribute = "Xunit.CollectionAttribute";
			public const string CollectionAttributeOfT_V3 = "Xunit.CollectionAttribute`1";
			public const string CollectionBehaviorAttribute = "Xunit.CollectionBehaviorAttribute";
			public const string CollectionBehaviorAttributeOfT_V3 = "Xunit.CollectionBehaviorAttribute`1";
			public const string CollectionDefinitionAttribute = "Xunit.CollectionDefinitionAttribute";
			public const string DataAttribute_V2 = "Xunit.Sdk.DataAttribute";
			public const string DataAttribute_V3 = "Xunit.v3.DataAttribute";
			public const string FactAttribute = "Xunit.FactAttribute";
			public const string IAssemblyInfo_V2 = "Xunit.Abstractions.IAssemblyInfo";
			public const string IAsyncLifetime = "Xunit.IAsyncLifetime";
			public const string IAttributeInfo_V2 = "Xunit.Abstractions.IAttributeInfo";
			public const string IClassFixtureOfT = "Xunit.IClassFixture`1";
			public const string ICodeGenTestAssembly_V3 = "Xunit.v3.ICodeGenTestAssembly";
			public const string ICodeGenTestCollectionFactory_V3 = "Xunit.v3.ICodeGenTestCollectionFactory";
			public const string ICollectionFixtureOfT = "Xunit.ICollectionFixture`1";
			public const string IConsoleResultWriter_V3 = "Xunit.Runner.Common.IConsoleResultWriter";
			public const string IDataAttribute_V3 = "Xunit.v3.IDataAttribute";
			public const string IFactAttribute_V3 = "Xunit.v3.IFactAttribute";
			public const string IMessageSink_V2 = "Xunit.Abstractions.IMessageSink";
			public const string IMessageSink_V3 = "Xunit.Sdk.IMessageSink";
			public const string IMessageSinkMessage_V2 = "Xunit.Abstractions.IMessageSinkMessage";
			public const string IMethodInfo_V2 = "Xunit.Abstractions.IMethodInfo";
			public const string IMicrosoftTestingPlatformResultWriter_V3 = "Xunit.Runner.Common.IMicrosoftTestingPlatformResultWriter";
			public const string IParameterInfo_V2 = "Xunit.Abstractions.IParameterInfo";
			public const string InlineDataAttribute = "Xunit.InlineDataAttribute";
			public const string IRunnerReporter_V3 = "Xunit.Runner.Common.IRunnerReporter";
			public const string ISourceInformation_V2 = "Xunit.Abstractions.ISourceInformation";
			public const string ISourceInformationProvider_V2 = "Xunit.Abstractions.ISourceInformationProvider";
			public const string ISourceInformationProvider_V3 = "Xunit.Runner.Common.ISourceInformationProvider";
			public const string ITest_V2 = "Xunit.Abstractions.ITest";
			public const string ITest_V3 = "Xunit.Sdk.ITest";
			public const string ITestAssembly_V2 = "Xunit.Abstractions.ITestAssembly";
			public const string ITestAssembly_V3 = "Xunit.Sdk.ITestAssembly";
			public const string ITestCase_V2 = "Xunit.Abstractions.ITestCase";
			public const string ITestCase_V3 = "Xunit.Sdk.ITestCase";
			public const string ITestCaseOrderer_V2 = "Xunit.Sdk.ITestCaseOrderer";
			public const string ITestCaseOrderer_V3 = "Xunit.v3.ITestCaseOrderer";
			public const string ITestClass_V2 = "Xunit.Abstractions.ITestClass";
			public const string ITestClass_V3 = "Xunit.Sdk.ITestClass";
			public const string ITestClassOrderer_V3 = "Xunit.v3.ITestClassOrderer";
			public const string ITestCollection_V2 = "Xunit.Abstractions.ITestCollection";
			public const string ITestCollection_V3 = "Xunit.Sdk.ITestCollection";
			public const string ITestCollectionOrderer_V2 = "Xunit.ITestCollectionOrderer";
			public const string ITestCollectionOrderer_V3 = "Xunit.v3.ITestCollectionOrderer";
			public const string ITestContext_V3 = "Xunit.ITestContext";
			public const string ITestContextAccessor_V3 = "Xunit.ITestContextAccessor";
			public const string ITestFramework_V2 = "Xunit.Abstractions.ITestFramework";
			public const string ITestFramework_V3 = "Xunit.v3.ITestFramework";
			public const string ITestFrameworkDiscoverer_V2 = "Xunit.Abstractions.ITestFrameworkDiscoverer";
			public const string ITestFrameworkDiscoverer_V3 = "Xunit.v3.ITestFrameworkDiscoverer";
			public const string ITestFrameworkExecutor_V2 = "Xunit.Abstractions.ITestFrameworkExecutor";
			public const string ITestFrameworkExecutor_V3 = "Xunit.v3.ITestFrameworkExecutor";
			public const string ITestMethod_V2 = "Xunit.Abstractions.ITestMethod";
			public const string ITestMethod_V3 = "Xunit.Sdk.ITestMethod";
			public const string ITestMethodOrderer_V3 = "Xunit.v3.ITestMethodOrderer";
			public const string ITestOutputHelper_V2 = "Xunit.Abstractions.ITestOutputHelper";
			public const string ITestOutputHelper_V3 = "Xunit.ITestOutputHelper";
			public const string ITestPipelineStartup_V3 = "Xunit.v3.ITestPipelineStartup";
			public const string ITheoryDataRow_V3 = "Xunit.ITheoryDataRow";
			public const string ITypeInfo_V2 = "Xunit.Abstractions.ITypeInfo";
			public const string IXunitSerializable_V2 = "Xunit.Abstractions.IXunitSerializable";
			public const string IXunitSerializable_V3 = "Xunit.Sdk.IXunitSerializable";
			public const string IXunitSerializer_V3 = "Xunit.Sdk.IXunitSerializer";
			public const string IXunitTestAssembly_V3 = "Xunit.v3.IXunitTestAssembly";
			public const string IXunitTestCollectionFactory_V2 = "Xunit.Sdk.IXunitTestCollectionFactory";
			public const string IXunitTestCollectionFactory_V3 = "Xunit.v3.IXunitTestCollectionFactory";
			public const string JsonTypeIDAttribute_V3 = "Xunit.Sdk.JsonTypeIDAttribute";
			public const string LongLivedMarshalByRefObject_Execution_V2 = "Xunit.LongLivedMarshalByRefObject";
			public const string LongLivedMarshalByRefObject_RunnerUtility = "Xunit.Sdk.LongLivedMarshalByRefObject";
			public const string MemberDataAttribute = "Xunit.MemberDataAttribute";
			public const string Record = "Xunit.Record";
			public const string RegisterXunitSerializerAttribute_V3 = "Xunit.Sdk.RegisterXunitSerializerAttribute";
			public const string RegisterConsoleResultWriterAttribute_V3 = "Xunit.Runner.Common.RegisterConsoleResultWriterAttribute";
			public const string RegisterConsoleResultWriterAttributeOfT_V3 = "Xunit.Runner.Common.RegisterConsoleResultWriterAttribute`1";
			public const string RegisterMicrosoftTestingPlatformResultWriterAttribute_V3 = "Xunit.Runner.Common.RegisterMicrosoftTestingPlatformResultWriterAttribute";
			public const string RegisterMicrosoftTestingPlatformResultWriterAttributeOfT_V3 = "Xunit.Runner.Common.RegisterMicrosoftTestingPlatformResultWriterAttribute`1";
			public const string RegisterResultWriterAttribute_V3 = "Xunit.Runner.Common.RegisterResultWriterAttribute";
			public const string RegisterResultWriterAttributeOfT_V3 = "Xunit.Runner.Common.RegisterResultWriterAttribute`1";
			public const string RegisterRunnerReporterAttribute_V3 = "Xunit.Runner.Common.RegisterRunnerReporterAttribute";
			public const string RegisterRunnerReporterAttributeOfT_V3 = "Xunit.Runner.Common.RegisterRunnerReporterAttribute`1";
			public const string TestCaseOrdererAttribute = "Xunit.TestCaseOrdererAttribute";
			public const string TestCaseOrdererAttributeOfT_V3 = "Xunit.TestCaseOrdererAttribute`1";
			public const string TestClassOrdererAttribute_V3 = "Xunit.TestClassOrdererAttribute";
			public const string TestClassOrdererAttributeOfT_V3 = "Xunit.TestClassOrdererAttribute`1";
			public const string TestCollectionOrdererAttribute = "Xunit.TestCollectionOrdererAttribute";
			public const string TestCollectionOrdererAttributeOfT_V3 = "Xunit.TestCollectionOrdererAttribute`1";
			public const string TestContext_V3 = "Xunit.TestContext";
			public const string TestFrameworkAttribute = "Xunit.TestFrameworkAttribute";
			public const string TestFrameworkAttributeOfT_V3 = "Xunit.TestFrameworkAttribute`1";
			public const string TestMethodOrdererAttribute_V3 = "Xunit.TestMethodOrdererAttribute";
			public const string TestMethodOrdererAttributeOfT_V3 = "Xunit.TestMethodOrdererAttribute`1";
			public const string TestPipelineStartupAttribute_V3 = "Xunit.v3.TestPipelineStartupAttribute";
			public const string TestPipelineStartupAttributeOfT_V3 = "Xunit.v3.TestPipelineStartupAttribute`1";
			public const string TheoryAttribute = "Xunit.TheoryAttribute";
			public const string TheoryData = "Xunit.TheoryData";
			public const string TheoryDataRow_V3 = "Xunit.TheoryDataRow";
		}
	}
}
