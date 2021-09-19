using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldHaveParameters>;

public class TheoryMethodShouldHaveParametersTests
{
	[Fact]
	public async void DoesNotFindErrorForFactMethod()
	{
		var source = @"
public class TestClass {
    [Xunit.Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Fact]
	public async void DoesNotFindErrorForTheoryMethodWithParameters()
	{
		var source = @"
public class TestClass {
    [Xunit.Theory]
    public void TestMethod(string s) { }
}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Fact]
	public async void FindsErrorForTheoryMethodWithoutParameters()
	{
		var source = @"
class TestClass {
    [Xunit.Theory]
    public void TestMethod() { }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 17, 4, 27);

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}
}
