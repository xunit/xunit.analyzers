using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.FactMethodMustNotHaveParameters>;

public class FactMethodMustNotHaveParametersTests
{
	[Fact]
	public async void DoesNotFindErrorForFactWithNoParameters()
	{
		var source = @"
public class TestClass {
    [Xunit.Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Fact]
	public async void DoesNotFindErrorForTheoryWithParameters()
	{
		var source = @"
public class TestClass {
    [Xunit.Theory]
    public void TestMethod(string p) { }
}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Fact]
	public async void FindsErrorForFactWithParameter()
	{
		var source = @"
public class TestClass {
    [Xunit.Fact]
    public void TestMethod(string p) { }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 17, 4, 27);

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}
}
