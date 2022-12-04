using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck>;

public class AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheckFixerTests
{
	const string template = @"
using System.Linq;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var collection = new[] {{ 1, 2, 3 }};

        {0};
    }}
}}";

	[Theory]
	[InlineData(
		"[|Assert.True(collection.Any(x => x == 2))|]",
		"Assert.Contains(collection, x => x == 2)")]
	[InlineData(
		"[|Assert.False(collection.Any(x => x == 2))|]",
		"Assert.DoesNotContain(collection, x => x == 2)")]
	public async void ReplacesAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}
}
