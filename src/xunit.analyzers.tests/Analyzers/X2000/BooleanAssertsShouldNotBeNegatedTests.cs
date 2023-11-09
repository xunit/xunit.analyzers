using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeNegated>;

public class BooleanAssertsShouldNotBeNegatedTests
{
	[Theory]
	[InlineData("False", "True")]
	[InlineData("True", "False")]
	public async void NegatedBooleanAssertionTriggers(
		string assertion,
		string replacement)
	{
		var code = $@"
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        bool condition = true;

        Assert.{assertion}(!condition);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(9, 9, 9, 28 + assertion.Length)
				.WithArguments(assertion, replacement);

		await Verify.VerifyAnalyzer(code, expected);
	}
}
