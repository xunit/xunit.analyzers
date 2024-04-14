using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DoNotUseAsyncVoidForTestMethods>;

public class DoNotUseAsyncVoidForTestMethodsTests
{
	[Fact]
	public async Task NonTestMethod_DoesNotTrigger()
	{
		var source = @"
using System.Threading.Tasks;

public class MyClass {
    public async void MyMethod() {
        await Task.Yield();
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task AsyncTaskMethod_DoesNotTrigger()
	{
		var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async Task TestMethod() {
        await Task.Yield();
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task AsyncValueTaskMethod_V3_DoesNotTrigger()
	{
		var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async ValueTask TestMethod() {
        await Task.Yield();
    }
}";

		await Verify.VerifyAnalyzerV3(source);
	}


	[Fact]
	public async Task AsyncVoidMethod_Triggers()
	{
		var source = @"
using System.Threading.Tasks;
using Xunit;

public class TestClass {
    [Fact]
    public async void TestMethod() {
        await Task.Yield();
    }
}";

		var expectedV2 =
			Verify
				.Diagnostic("xUnit1048")
				.WithSpan(7, 23, 7, 33);
		var expectedV3 =
			Verify
				.Diagnostic("xUnit1049")
				.WithSpan(7, 23, 7, 33);

		await Verify.VerifyAnalyzerV2(source, expectedV2);
		await Verify.VerifyAnalyzerV3(source, expectedV3);
	}
}
