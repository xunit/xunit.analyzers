using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseAsyncVoidForTestMethods>;

public class DoNotUseAsyncVoidForTestMethodsTests
{
	[Fact]
	public async Task NonTestMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;

			public class MyClass {
			    public async void MyMethod() {
			        await Task.Yield();
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task NonAsyncTestMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Fact]
			    public void TestMethod() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task AsyncTaskMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
			    [Fact]
			    public async Task TestMethod() {
			        await Task.Yield();
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task AsyncValueTaskMethod_V3_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
			    [Fact]
			    public async ValueTask TestMethod() {
			        await Task.Yield();
			    }
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async Task AsyncVoidMethod_Triggers()
	{
		var source = /* lang=c#-test */ """
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
			    [Fact]
			    public async void {|#0:TestMethod|}() {
			        await Task.Yield();
			    }
			}
			""";
		var expectedV2 = Verify.Diagnostic("xUnit1048").WithLocation(0);
		var expectedV3 = Verify.Diagnostic("xUnit1049").WithLocation(0);

		await Verify.VerifyAnalyzerV2(source, expectedV2);
		await Verify.VerifyAnalyzerV3(source, expectedV3);
	}
}
