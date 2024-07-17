using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodSupportedReturnType>;

public class TestMethodSupportedReturnTypeTests
{
	[Fact]
	public async Task NonTestMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class NonTestClass {
			    public int Add(int x, int y) {
			        return x + y;
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("object")]
	[InlineData("Task<int>")]
	[InlineData("ValueTask<string>")]
	public async Task InvalidReturnType_Triggers(string returnType)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {{
			    [Fact]
			    public {0} {{|#0:TestMethod|}}() {{
			        return default({0});
				}}
			}}
			""", returnType);
		var expectedV2 = Verify.Diagnostic().WithLocation(0).WithArguments("void, Task");
		var expectedV3 = Verify.Diagnostic().WithLocation(0).WithArguments("void, Task, ValueTask");

		await Verify.VerifyAnalyzerV2(source, expectedV2);
		await Verify.VerifyAnalyzerV3(source, expectedV3);
	}

	[Fact]
	public async Task ValueTask_TriggersInV2_DoesNotTriggerInV3()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
			    [Fact]
			    public ValueTask {|#0:TestMethod|}() {
			        return default(ValueTask);
			    }
			}
			""";
		var expectedV2 = Verify.Diagnostic().WithLocation(0).WithArguments("void, Task");

		await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp7, source, expectedV2);
		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7, source);
	}

	[Theory]
	[InlineData("MyTest")]
	[InlineData("MyTestAttribute")]
	public async Task CustomTestAttribute_DoesNotTrigger(string attribute)
	{
		var source = string.Format(/* lang=c#-test */ """
			using Xunit;

			class MyTestAttribute : FactAttribute {{ }}

			public class TestClass {{
			    [{0}]
			    public int TestMethod() {{
			        return 0;
				}}
			}}
			""", attribute);

		await Verify.VerifyAnalyzer(source);
	}
}
