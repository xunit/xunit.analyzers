using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssemblyAttributeImplementationValidation>;

public class X3004_AssemblyAttributeImplementationValidationTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;
			using Xunit.Runner.Common;
			using Xunit.Sdk;
			using Xunit.v3;

			// xunit.v3.core
			[assembly: AssemblyFixture(typeof(object))]
			[assembly: {|#0:CollectionBehavior(typeof(MyCollectionFactory))|}]
			[assembly: {|#1:TestCaseOrderer(typeof(object))|}]
			[assembly: {|#2:TestClassOrderer(typeof(object))|}]
			[assembly: {|#3:TestCollectionOrderer(typeof(object))|}]
			[assembly: {|#4:TestFramework(typeof(object))|}]
			[assembly: {|#5:TestMethodOrderer(typeof(object))|}]
			[assembly: {|#6:TestPipelineStartup(typeof(object))|}]

			// xunit.v3.runner.common
			[assembly: {|#10:RegisterConsoleResultWriter("foo", typeof(object))|}]
			[assembly: {|#11:RegisterMicrosoftTestingPlatformResultWriter("foo", typeof(object))|}]
			[assembly: {|#12:RegisterResultWriter("foo", typeof(object))|}]
			[assembly: {|#13:RegisterResultWriter("foo", typeof(MyConsoleResultWriter))|}]
			[assembly: {|#14:RegisterResultWriter("foo", typeof(MyMTPResultWriter))|}]
			[assembly: {|#15:RegisterRunnerReporter(typeof(object))|}]

			class MyConsoleResultWriter : {|CS0535:{|CS0535:{|CS0535:IConsoleResultWriter|}|}|} { }
			class MyMTPResultWriter : {|CS0535:{|CS0535:{|CS0535:{|CS0535:IMicrosoftTestingPlatformResultWriter|}|}|}|} { }
			class MyCollectionFactory { public MyCollectionFactory(IXunitTestAssembly testAssembly) { } }  // Don't trigger xUnit3005
			""";
		var expectedNonAot = new[] {
			Verify.Diagnostic("xUnit3004").WithLocation(0).WithArguments("MyCollectionFactory", "Xunit.v3.IXunitTestCollectionFactory"),
			Verify.Diagnostic("xUnit3004").WithLocation(1).WithArguments("object", "Xunit.v3.ITestCaseOrderer"),
			Verify.Diagnostic("xUnit3004").WithLocation(2).WithArguments("object", "Xunit.v3.ITestClassOrderer"),
			Verify.Diagnostic("xUnit3004").WithLocation(3).WithArguments("object", "Xunit.v3.ITestCollectionOrderer"),
			Verify.Diagnostic("xUnit3004").WithLocation(4).WithArguments("object", "Xunit.v3.ITestFramework"),
			Verify.Diagnostic("xUnit3004").WithLocation(5).WithArguments("object", "Xunit.v3.ITestMethodOrderer"),
			Verify.Diagnostic("xUnit3004").WithLocation(6).WithArguments("object", "Xunit.v3.ITestPipelineStartup"),

			Verify.Diagnostic("xUnit3004").WithLocation(10).WithArguments("object", "Xunit.Runner.Common.IConsoleResultWriter"),
			Verify.Diagnostic("xUnit3004").WithLocation(11).WithArguments("object", "Xunit.Runner.Common.IMicrosoftTestingPlatformResultWriter"),
			Verify.Diagnostic("xUnit3004").WithLocation(12).WithArguments("object", "Xunit.Runner.Common.IConsoleResultWriter"),
			Verify.Diagnostic("xUnit3004").WithLocation(12).WithArguments("object", "Xunit.Runner.Common.IMicrosoftTestingPlatformResultWriter"),
			Verify.Diagnostic("xUnit3004").WithLocation(13).WithArguments("MyConsoleResultWriter", "Xunit.Runner.Common.IMicrosoftTestingPlatformResultWriter"),
			Verify.Diagnostic("xUnit3004").WithLocation(14).WithArguments("MyMTPResultWriter", "Xunit.Runner.Common.IConsoleResultWriter"),
			Verify.Diagnostic("xUnit3004").WithLocation(15).WithArguments("object", "Xunit.Runner.Common.IRunnerReporter"),
		};

		await Verify.VerifyAnalyzerV3NonAot(source, expectedNonAot);

#if NETCOREAPP && ROSLYN_LATEST
		var expectedAot = new[] {
			Verify.Diagnostic("xUnit3004").WithLocation(0).WithArguments("MyCollectionFactory", "Xunit.v3.ICodeGenTestCollectionFactory"),
			Verify.Diagnostic("xUnit3004").WithLocation(1).WithArguments("object", "Xunit.v3.ITestCaseOrderer"),
			Verify.Diagnostic("xUnit3004").WithLocation(2).WithArguments("object", "Xunit.v3.ITestClassOrderer"),
			Verify.Diagnostic("xUnit3004").WithLocation(3).WithArguments("object", "Xunit.v3.ITestCollectionOrderer"),
			Verify.Diagnostic("xUnit3004").WithLocation(4).WithArguments("object", "Xunit.v3.ITestFramework"),
			Verify.Diagnostic("xUnit3004").WithLocation(5).WithArguments("object", "Xunit.v3.ITestMethodOrderer"),
			Verify.Diagnostic("xUnit3004").WithLocation(6).WithArguments("object", "Xunit.v3.ITestPipelineStartup"),

			Verify.Diagnostic("xUnit3004").WithLocation(10).WithArguments("object", "Xunit.Runner.Common.IConsoleResultWriter"),
			Verify.Diagnostic("xUnit3004").WithLocation(11).WithArguments("object", "Xunit.Runner.Common.IMicrosoftTestingPlatformResultWriter"),
			Verify.Diagnostic("xUnit3004").WithLocation(12).WithArguments("object", "Xunit.Runner.Common.IConsoleResultWriter"),
			Verify.Diagnostic("xUnit3004").WithLocation(12).WithArguments("object", "Xunit.Runner.Common.IMicrosoftTestingPlatformResultWriter"),
			Verify.Diagnostic("xUnit3004").WithLocation(13).WithArguments("MyConsoleResultWriter", "Xunit.Runner.Common.IMicrosoftTestingPlatformResultWriter"),
			Verify.Diagnostic("xUnit3004").WithLocation(14).WithArguments("MyMTPResultWriter", "Xunit.Runner.Common.IConsoleResultWriter"),
			Verify.Diagnostic("xUnit3004").WithLocation(15).WithArguments("object", "Xunit.Runner.Common.IRunnerReporter"),
		};

		await Verify.VerifyAnalyzerV3Aot(source.Replace("IXunitTestAssembly", "ICodeGenTestAssembly"), expectedAot);
#endif
	}

	[Fact]
	public async ValueTask V3_only_NonAOT()
	{
		var source = /* lang=c#-test */ """
			using Xunit;
			using Xunit.Runner.Common;
			using Xunit.Sdk;

			// xunit.v3.common
			[assembly: {|#0:RegisterXunitSerializer(typeof(object))|}]
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit3004").WithLocation(0).WithArguments("object", "Xunit.Sdk.IXunitSerializer"),
		};

		await Verify.VerifyAnalyzerV3NonAot(source, expected);
	}
}
