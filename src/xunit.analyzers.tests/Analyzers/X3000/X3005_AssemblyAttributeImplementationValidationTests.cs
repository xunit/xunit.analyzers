using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssemblyAttributeImplementationValidation>;

public class X3005_AssemblyAttributeImplementationValidationTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;
			using Xunit.Runner.Common;
			using Xunit.v3;

			// xunit.v3.core
			[assembly: AssemblyFixture(typeof(object))]
			[assembly: {|#0:AssemblyFixture(typeof(MyAssemblyFixture_Missing))|}]
			[assembly: {|#1:AssemblyFixture(typeof(MyAssemblyFixture_Obsolete))|}]
			[assembly: {|#2:AssemblyFixture(typeof(MyAssemblyFixture_NonPublic))|}]

			// xunit.v3.runner.common
			[assembly: RegisterConsoleResultWriter("foo", typeof(XmlV2ResultWriter))]
			[assembly: {|#10:RegisterConsoleResultWriter("foo", typeof(MyResultWriter_Missing))|}]
			[assembly: {|#11:RegisterConsoleResultWriter("foo", typeof(MyResultWriter_Obsolete))|}]
			[assembly: {|#12:RegisterConsoleResultWriter("foo", typeof(MyResultWriter_NonPublic))|}]

			[assembly: RegisterMicrosoftTestingPlatformResultWriter("foo", typeof(XmlV2ResultWriter))]
			[assembly: {|#20:RegisterMicrosoftTestingPlatformResultWriter("foo", typeof(MyResultWriter_Missing))|}]
			[assembly: {|#21:RegisterMicrosoftTestingPlatformResultWriter("foo", typeof(MyResultWriter_Obsolete))|}]
			[assembly: {|#22:RegisterMicrosoftTestingPlatformResultWriter("foo", typeof(MyResultWriter_NonPublic))|}]

			[assembly: RegisterResultWriter("foo", typeof(XmlV2ResultWriter))]
			[assembly: {|#30:RegisterResultWriter("foo", typeof(MyResultWriter_Missing))|}]
			[assembly: {|#31:RegisterResultWriter("foo", typeof(MyResultWriter_Obsolete))|}]
			[assembly: {|#32:RegisterResultWriter("foo", typeof(MyResultWriter_NonPublic))|}]

			[assembly: RegisterRunnerReporter(typeof(QuietReporter))]
			[assembly: {|#40:RegisterRunnerReporter(typeof(MyRunnerReporter_Missing))|}]
			[assembly: {|#41:RegisterRunnerReporter(typeof(MyRunnerReporter_Obsolete))|}]
			[assembly: {|#42:RegisterRunnerReporter(typeof(MyRunnerReporter_NonPublic))|}]

			public class MyAssemblyFixture_Missing { public MyAssemblyFixture_Missing(int x) { } }
			public class MyAssemblyFixture_Obsolete	{ [Obsolete] public MyAssemblyFixture_Obsolete() { } }
			public class MyAssemblyFixture_NonPublic { protected MyAssemblyFixture_NonPublic() { } }

			public class MyResultWriter_Missing : {|CS0535:{|CS0535:{|CS0535:IConsoleResultWriter|}|}|}, {|CS0535:{|CS0535:{|CS0535:IMicrosoftTestingPlatformResultWriter|}|}|}
			{ public MyResultWriter_Missing(int x) { } }
			public class MyResultWriter_Obsolete : {|CS0535:{|CS0535:{|CS0535:IConsoleResultWriter|}|}|}, {|CS0535:{|CS0535:{|CS0535:IMicrosoftTestingPlatformResultWriter|}|}|}
			{ [Obsolete] public MyResultWriter_Obsolete() { } }
			public class MyResultWriter_NonPublic : {|CS0535:{|CS0535:{|CS0535:IConsoleResultWriter|}|}|}, {|CS0535:{|CS0535:{|CS0535:IMicrosoftTestingPlatformResultWriter|}|}|}
			{ protected MyResultWriter_NonPublic() { } }

			public class MyRunnerReporter_Missing : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:IRunnerReporter|}|}|}|}|}|}
			{ public MyRunnerReporter_Missing(int x) { } }
			public class MyRunnerReporter_Obsolete : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:IRunnerReporter|}|}|}|}|}|}
			{ [Obsolete] public MyRunnerReporter_Obsolete() { } }
			public class MyRunnerReporter_NonPublic : {|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:{|CS0535:IRunnerReporter|}|}|}|}|}|}
			{ protected MyRunnerReporter_NonPublic() { } }
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit3005").WithLocation(0).WithArguments("MyAssemblyFixture_Missing", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(1).WithArguments("MyAssemblyFixture_Obsolete", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(2).WithArguments("MyAssemblyFixture_NonPublic", string.Empty),

			Verify.Diagnostic("xUnit3005").WithLocation(10).WithArguments("MyResultWriter_Missing", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(11).WithArguments("MyResultWriter_Obsolete", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(12).WithArguments("MyResultWriter_NonPublic", string.Empty),

			Verify.Diagnostic("xUnit3005").WithLocation(20).WithArguments("MyResultWriter_Missing", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(21).WithArguments("MyResultWriter_Obsolete", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(22).WithArguments("MyResultWriter_NonPublic", string.Empty),

			Verify.Diagnostic("xUnit3005").WithLocation(30).WithArguments("MyResultWriter_Missing", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(31).WithArguments("MyResultWriter_Obsolete", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(32).WithArguments("MyResultWriter_NonPublic", string.Empty),

			Verify.Diagnostic("xUnit3005").WithLocation(40).WithArguments("MyRunnerReporter_Missing", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(41).WithArguments("MyRunnerReporter_Obsolete", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(42).WithArguments("MyRunnerReporter_NonPublic", string.Empty),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source, expected);
	}

	[Fact]
	public async ValueTask V3_only_NonAot()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit.Sdk;

			// xunit.v3.common
			[assembly: RegisterXunitSerializer(typeof(MySerializer))]
			[assembly: {|#0:RegisterXunitSerializer(typeof(MySerializer_Missing))|}]
			[assembly: {|#1:RegisterXunitSerializer(typeof(MySerializer_Obsolete))|}]
			[assembly: {|#2:RegisterXunitSerializer(typeof(MySerializer_NonPublic))|}]

			public class MySerializer : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{ }
			public class MySerializer_Missing : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{ public MySerializer_Missing(int x) { } }
			public class MySerializer_Obsolete : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{ [Obsolete] MySerializer_Obsolete() { } }
			public class MySerializer_NonPublic : {|CS0535:{|CS0535:{|CS0535:IXunitSerializer|}|}|}
			{ protected MySerializer_NonPublic() { } }
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit3005").WithLocation(0).WithArguments("MySerializer_Missing", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(1).WithArguments("MySerializer_Obsolete", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(2).WithArguments("MySerializer_NonPublic", string.Empty),
		};

		await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp9, source, expected);
	}

	// These have to be tested individually since [TestXyzOrderer] does not permit duplicates
	[Theory]
	[InlineData("MyOrderer_Empty", false)]
	[InlineData("MyOrderer_WithInstance", false)]
	[InlineData("MyOrderer_Missing", true)]
	[InlineData("MyOrderer_Obsolete", true)]
	[InlineData("MyOrderer_NonPublic", true)]
	public async ValueTask V3_only_Orderers(
		string testOrdererTypePrefix,
		bool expectTrigger)
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;
			using Xunit.v3;

			[assembly: {|#0:TestCaseOrderer(typeof(TEST_ORDERER_TYPE_Case))|}]
			[assembly: {|#1:TestClassOrderer(typeof(TEST_ORDERER_TYPE_Class))|}]
			[assembly: {|#2:TestCollectionOrderer(typeof(TEST_ORDERER_TYPE_Collection))|}]
			[assembly: {|#3:TestMethodOrderer(typeof(TEST_ORDERER_TYPE_Method))|}]

			public class MyOrderer_Empty_Case : {|CS0535:ITestCaseOrderer|}
			{ }
			public class MyOrderer_WithInstance_Case : {|CS0535:ITestCaseOrderer|}
			{
				[Obsolete] public MyOrderer_WithInstance_Case() { }
				public static MyOrderer_WithInstance_Case Instance { get; } = new();
			}
			public class MyOrderer_Missing_Case : {|CS0535:ITestCaseOrderer|}
			{ public MyOrderer_Missing_Case(int x) { } }
			public class MyOrderer_Obsolete_Case : {|CS0535:ITestCaseOrderer|}
			{ [Obsolete] public MyOrderer_Obsolete_Case() { } }
			public class MyOrderer_NonPublic_Case : {|CS0535:ITestCaseOrderer|}
			{ protected MyOrderer_NonPublic_Case() { } }

			public class MyOrderer_Empty_Class : {|CS0535:ITestClassOrderer|}
			{ }
			public class MyOrderer_WithInstance_Class : {|CS0535:ITestClassOrderer|}
			{
				[Obsolete] public MyOrderer_WithInstance_Class() { }
				public static MyOrderer_WithInstance_Class Instance { get; } = new();
			}
			public class MyOrderer_Missing_Class : {|CS0535:ITestClassOrderer|}
			{ public MyOrderer_Missing_Class(int x) { } }
			public class MyOrderer_Obsolete_Class : {|CS0535:ITestClassOrderer|}
			{ [Obsolete] public MyOrderer_Obsolete_Class() { } }
			public class MyOrderer_NonPublic_Class : {|CS0535:ITestClassOrderer|}
			{ protected MyOrderer_NonPublic_Class() { } }

			public class MyOrderer_Empty_Collection : {|CS0535:ITestCollectionOrderer|}
			{ }
			public class MyOrderer_WithInstance_Collection : {|CS0535:ITestCollectionOrderer|}
			{
				[Obsolete] public MyOrderer_WithInstance_Collection() { }
				public static MyOrderer_WithInstance_Collection Instance { get; } = new();
			}
			public class MyOrderer_Missing_Collection : {|CS0535:ITestCollectionOrderer|}
			{ public MyOrderer_Missing_Collection(int x) { } }
			public class MyOrderer_Obsolete_Collection : {|CS0535:ITestCollectionOrderer|}
			{ [Obsolete] public MyOrderer_Obsolete_Collection() { } }
			public class MyOrderer_NonPublic_Collection : {|CS0535:ITestCollectionOrderer|}
			{ protected MyOrderer_NonPublic_Collection() { } }

			public class MyOrderer_Empty_Method : {|CS0535:ITestMethodOrderer|}
			{ }
			public class MyOrderer_WithInstance_Method : {|CS0535:ITestMethodOrderer|}
			{
				[Obsolete] public MyOrderer_WithInstance_Method() { }
				public static MyOrderer_WithInstance_Method Instance { get; } = new();
			}
			public class MyOrderer_Missing_Method : {|CS0535:ITestMethodOrderer|}
			{ public MyOrderer_Missing_Method(int x) { } }
			public class MyOrderer_Obsolete_Method : {|CS0535:ITestMethodOrderer|}
			{ [Obsolete] public MyOrderer_Obsolete_Method() { } }
			public class MyOrderer_NonPublic_Method : {|CS0535:ITestMethodOrderer|}
			{ protected MyOrderer_NonPublic_Method() { } }
			""".Replace("TEST_ORDERER_TYPE", testOrdererTypePrefix);
		var expected = new[] {
			Verify.Diagnostic("xUnit3005").WithLocation(0).WithArguments(testOrdererTypePrefix + "_Case", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(1).WithArguments(testOrdererTypePrefix + "_Class", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(2).WithArguments(testOrdererTypePrefix + "_Collection", string.Empty),
			Verify.Diagnostic("xUnit3005").WithLocation(3).WithArguments(testOrdererTypePrefix + "_Method", string.Empty),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source, expectTrigger ? expected : []);
	}

	// These have to be tested individually since [TestFramework] does not permit duplicates
	[Theory]
	[InlineData("MyTestFramework_Empty", false)]
	[InlineData("MyTestFramework_WithString", false)]
	[InlineData("MyTestFramework_Missing", true)]
	[InlineData("MyTestFramework_Obsolete", true)]
	[InlineData("MyTestFramework_NonPublic", true)]
	public async ValueTask V3_only_TestFrameworks(
		string testFrameworkType,
		bool expectTrigger)
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using System;
			using Xunit;
			using Xunit.v3;

			[assembly: {|#0:TestFramework(typeof(TEST_FRAMEWORK_TYPE))|}]

			public class MyTestFramework_Empty : {|CS0535:{|CS0535:{|CS0535:{|CS0535:ITestFramework|}|}|}|}
			{ }
			public class MyTestFramework_WithString : {|CS0535:{|CS0535:{|CS0535:{|CS0535:ITestFramework|}|}|}|}
			{ public MyTestFramework_WithString(string? configFileName) { } }
			public class MyTestFramework_Missing : {|CS0535:{|CS0535:{|CS0535:{|CS0535:ITestFramework|}|}|}|}
			{ public MyTestFramework_Missing(int x) { } }
			public class MyTestFramework_Obsolete : {|CS0535:{|CS0535:{|CS0535:{|CS0535:ITestFramework|}|}|}|}
			{ [Obsolete] public MyTestFramework_Obsolete(string? configFileName) { } }
			public class MyTestFramework_NonPublic : {|CS0535:{|CS0535:{|CS0535:{|CS0535:ITestFramework|}|}|}|}
			{ protected MyTestFramework_NonPublic() { } }
			""".Replace("TEST_FRAMEWORK_TYPE", testFrameworkType);

		if (expectTrigger)
			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, Verify.Diagnostic("xUnit3005").WithLocation(0).WithArguments(testFrameworkType, "string? configFileName"));
		else
			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

	// These have to be tested individually since [CollectionBehavior] does not permit duplicates
	[Theory]
	[InlineData("CollectionPerClassTestCollectionFactory", false)]
	[InlineData("MyTestCollectionFactory_Missing", true)]
	[InlineData("MyTestCollectionFactory_Obsolete", true)]
	[InlineData("MyTestCollectionFactory_NonPublic", true)]
	public async ValueTask V3_only_TestCollectionFactories(
		string testCollectionFactoryType,
		bool expectTrigger)
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;
			using Xunit.v3;

			[assembly: {|#0:CollectionBehavior(typeof(TEST_COLLECTION_FACTORY_TYPE))|}]

			public class MyTestCollectionFactory_Missing : {|CS0535:{|CS0535:IXunitTestCollectionFactory|}|}
			{ }
			public class MyTestCollectionFactory_Obsolete : {|CS0535:{|CS0535:IXunitTestCollectionFactory|}|}
			{ [Obsolete] public MyTestCollectionFactory_Obsolete(IXunitTestAssembly testAssembly) { } }
			public class MyTestCollectionFactory_NonPublic : {|CS0535:{|CS0535:IXunitTestCollectionFactory|}|}
			{ protected MyTestCollectionFactory_NonPublic(IXunitTestAssembly testAssembly) { } }
			""".Replace("TEST_COLLECTION_FACTORY_TYPE", testCollectionFactoryType);

		if (expectTrigger)
			await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp8, source, Verify.Diagnostic("xUnit3005").WithLocation(0).WithArguments(testCollectionFactoryType, "IXunitTestAssembly testAssembly"));
		else
			await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp8, source);

#if NETCOREAPP && ROSLYN_LATEST
		source =
			source
				.Replace("IXunitTestCollectionFactory", "ICodeGenTestCollectionFactory")
				.Replace("IXunitTestAssembly", "ICodeGenTestAssembly");

		if (expectTrigger)
			await Verify.VerifyAnalyzerV3Aot(LanguageVersion.CSharp8, source, Verify.Diagnostic("xUnit3005").WithLocation(0).WithArguments(testCollectionFactoryType, "ICodeGenTestAssembly testAssembly"));
		else
			await Verify.VerifyAnalyzerV3Aot(LanguageVersion.CSharp8, source);
#endif
	}
}
