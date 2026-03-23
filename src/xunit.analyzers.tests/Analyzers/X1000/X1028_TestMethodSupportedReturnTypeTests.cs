using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodSupportedReturnType>;

public class X1028_TestMethodSupportedReturnTypeTests
{
	const string V2ReturnTypes = "void, Task";
	const string V3ReturnTypes = "void, Task, ValueTask";

	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class NonTestClass {
				public int NonTestMethod_DoesNotTrigger(int x, int y) {
					return x + y;
				}
			}

			public class TestClass {
				[Fact]
				public object {|#0:Fact_InvalidReturnType_Object_Triggers|}() {
					return default(object);
				}

				[Theory]
				public object {|#1:Theory_InvalidReturnType_Object_Triggers|}() {
					return default(object);
				}

				[Fact]
				public Task<int> {|#2:Fact_GenericTask_InvalidReturnType_Object_Triggers|}() {
					return Task.FromResult(42);
				}

				[Theory]
				public Task<int> {|#3:Theory_GenericTask_InvalidReturnType_Object_Triggers|}() {
					return Task.FromResult(42);
				}

				[Fact]
				public ValueTask<int> {|#4:Fact_GenericValueTask_InvalidReturnType_Object_Triggers|}() {
					return default(ValueTask<int>);
				}

				[Theory]
				public ValueTask<int> {|#5:Theory_GenericValueTask_InvalidReturnType_Object_Triggers|}() {
					return default(ValueTask<int>);
				}

				[Fact]
				public ValueTask {|#6:Fact_ValueTask_TriggersInV2_DoesNotTriggerInV3|}() {
					return default(ValueTask);
				}

				[Theory]
				public ValueTask {|#7:Theory_ValueTask_TriggersInV2_DoesNotTriggerInV3|}() {
					return default(ValueTask);
				}
			}
			""";
		var expectedV2 = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments(V2ReturnTypes),
			Verify.Diagnostic().WithLocation(1).WithArguments(V2ReturnTypes),
			Verify.Diagnostic().WithLocation(2).WithArguments(V2ReturnTypes),
			Verify.Diagnostic().WithLocation(3).WithArguments(V2ReturnTypes),
			Verify.Diagnostic().WithLocation(4).WithArguments(V2ReturnTypes),
			Verify.Diagnostic().WithLocation(5).WithArguments(V2ReturnTypes),
			Verify.Diagnostic().WithLocation(6).WithArguments(V2ReturnTypes),
			Verify.Diagnostic().WithLocation(7).WithArguments(V2ReturnTypes),
		};
		var expectedV3 = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments(V3ReturnTypes),
			Verify.Diagnostic().WithLocation(1).WithArguments(V3ReturnTypes),
			Verify.Diagnostic().WithLocation(2).WithArguments(V3ReturnTypes),
			Verify.Diagnostic().WithLocation(3).WithArguments(V3ReturnTypes),
			Verify.Diagnostic().WithLocation(4).WithArguments(V3ReturnTypes),
			Verify.Diagnostic().WithLocation(5).WithArguments(V3ReturnTypes),
		};

		await Verify.VerifyAnalyzerV2(source, expectedV2);
		await Verify.VerifyAnalyzerV3(source, expectedV3);
	}

	[Fact]
	public async ValueTask V2_and_V3_NonAOT()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class MyTestAttribute : FactAttribute { }

			public class TestClass {
				[MyTest]
				public int TestMethod() {
					return 0;
				}
			}
			""";

		await Verify.VerifyAnalyzerNonAot(source);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class NonTestClass {
				public int NonTestMethod_DoesNotTrigger(int x, int y) {
					return x + y;
				}
			}

			public class TestClass {
				[CulturedFact(new[] { "en-US" })]
				public object {|#0:Fact_InvalidReturnType_Object_Triggers|}() {
					return default(object);
				}

				[CulturedTheory(new[] { "en-US" })]
				public object {|#1:Theory_InvalidReturnType_Object_Triggers|}() {
					return default(object);
				}

				[CulturedFact(new[] { "en-US" })]
				public Task<int> {|#2:Fact_GenericTask_InvalidReturnType_Object_Triggers|}() {
					return Task.FromResult(42);
				}

				[CulturedTheory(new[] { "en-US" })]
				public Task<int> {|#3:Theory_GenericTask_InvalidReturnType_Object_Triggers|}() {
					return Task.FromResult(42);
				}

				[CulturedFact(new[] { "en-US" })]
				public ValueTask<int> {|#4:Fact_GenericValueTask_InvalidReturnType_Object_Triggers|}() {
					return default(ValueTask<int>);
				}

				[CulturedTheory(new[] { "en-US" })]
				public ValueTask<int> {|#5:Theory_GenericValueTask_InvalidReturnType_Object_Triggers|}() {
					return default(ValueTask<int>);
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments(V3ReturnTypes),
			Verify.Diagnostic().WithLocation(1).WithArguments(V3ReturnTypes),
			Verify.Diagnostic().WithLocation(2).WithArguments(V3ReturnTypes),
			Verify.Diagnostic().WithLocation(3).WithArguments(V3ReturnTypes),
			Verify.Diagnostic().WithLocation(4).WithArguments(V3ReturnTypes),
			Verify.Diagnostic().WithLocation(5).WithArguments(V3ReturnTypes),
		};

		await Verify.VerifyAnalyzerV3(source, expected);
	}
}
