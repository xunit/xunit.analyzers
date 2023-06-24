using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForCollectionSizeCheck>;

public class AssertEqualShouldNotBeUsedForCollectionSizeCheckFixerTests
{
	const string template = @"
using System.Linq;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var data = new[] {{ 1, 2, 3 }};

        {0};
    }}
}}";

	[Theory]
	[InlineData("[|Assert.Equal(1, data.Count())|]", "Assert.Single(data)")]
	[InlineData("[|Assert.Equal(0, data.Count())|]", "Assert.Empty(data)")]
	[InlineData("[|Assert.NotEqual(0, data.Count())|]", "Assert.NotEmpty(data)")]
	public async void ReplacesCollectionCountWithAppropriateAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFixAsyncV2(before, after, AssertEqualShouldNotBeUsedForCollectionSizeCheckFixer.Key_UseAlternateAssert);
	}
}
