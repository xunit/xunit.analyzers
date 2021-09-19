using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForBoolLiteralCheck>;

public class AssertEqualShouldNotBeUsedForBoolLiteralCheckFixerTests
{
	const string template = @"
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var actual = true;

        {0};
    }}
}}";

	[Theory]
	[InlineData("[|Assert.Equal(false, actual)|]", "Assert.False(actual)")]
	[InlineData("[|Assert.Equal(true, actual)|]", "Assert.True(actual)")]
	[InlineData("[|Assert.StrictEqual(false, actual)|]", "Assert.False(actual)")]
	[InlineData("[|Assert.StrictEqual(true, actual)|]", "Assert.True(actual)")]
	[InlineData("[|Assert.NotEqual(false, actual)|]", "Assert.True(actual)")]
	[InlineData("[|Assert.NotEqual(true, actual)|]", "Assert.False(actual)")]
	[InlineData("[|Assert.NotStrictEqual(false, actual)|]", "Assert.True(actual)")]
	[InlineData("[|Assert.NotStrictEqual(true, actual)|]", "Assert.False(actual)")]
	public async void ConvertsToBooleanAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}
}
