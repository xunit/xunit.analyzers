using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualGenericShouldNotBeUsedForStringValue>;

public class AssertEqualGenericShouldNotBeUsedForStringValueFixerTests
{
	const string template = @"
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        string result = ""foo"";

        {0};
    }}
}}";

	[Theory]
	[InlineData(Constants.Asserts.Equal)]
	[InlineData(Constants.Asserts.StrictEqual)]
	public async void RemovesGeneric(string assert)
	{
		var before = string.Format(template, $@"[|Assert.{assert}<string>(""foo"", result)|]");
		var after = string.Format(template, @"Assert.Equal(""foo"", result)");

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}
}
