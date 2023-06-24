using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualPrecisionShouldBeInRange>;

public class AssertEqualPrecisionShouldBeInRangeFixerTests
{
	const string template = @"
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        {0};
    }}
}}";

	[Theory]
	// double = [0..15]
	[InlineData("Assert.Equal(10.1d, 10.2d, [|-1|])", "Assert.Equal(10.1d, 10.2d, 0)")]
	[InlineData("Assert.Equal(10.1d, 10.2d, [|16|])", "Assert.Equal(10.1d, 10.2d, 15)")]
	// decimal = [0..28]
	[InlineData("Assert.Equal(10.1m, 10.2m, [|-1|])", "Assert.Equal(10.1m, 10.2m, 0)")]
	[InlineData("Assert.Equal(10.1m, 10.2m, [|29|])", "Assert.Equal(10.1m, 10.2m, 28)")]
	public async void ChangesPrecisionToZero(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFixAsyncV2(before, after, AssertEqualPrecisionShouldBeInRangeFixer.Key_UsePrecision);
	}
}
