using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSameShouldNotBeCalledOnValueTypes>;

public class AssertSameShouldNotBeCalledOnValueTypesFixerTests
{
	const string template = @"
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var data = 1;

        {0};
    }}
}}";

	[Theory]
	[InlineData("[|Assert.Same(1, data)|]", "Assert.Equal(1, data)")]
	[InlineData("[|Assert.NotSame(1, data)|]", "Assert.NotEqual(1, data)")]
	public async void ConvertsSameToEqual(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}
}
