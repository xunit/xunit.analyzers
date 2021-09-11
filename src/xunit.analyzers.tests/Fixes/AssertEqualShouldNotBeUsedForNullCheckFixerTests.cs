using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForNullCheck>;

public class AssertEqualShouldNotBeUsedForNullCheckFixerTests
{
	const string template = @"
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        int? data = 1;

        {0};
    }}
}}";

	[Theory]
	[InlineData("[|Assert.Equal(null, data)|]", "Assert.Null(data)")]
	[InlineData("[|Assert.StrictEqual(null, data)|]", "Assert.Null(data)")]
	[InlineData("[|Assert.NotEqual(null, data)|]", "Assert.NotNull(data)")]
	[InlineData("[|Assert.NotStrictEqual(null, data)|]", "Assert.NotNull(data)")]
	public async void ConvertsToAppropriateNullAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFixAsync(before, after);
	}
}
