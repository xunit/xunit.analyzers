using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualsShouldNotBeUsed>;

public class AssertEqualsShouldNotBeUsedFixerTests
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
	[InlineData("{|CS0619:[|Assert.Equals(1, data)|]|}", "Assert.Equal(1, data)")]
	[InlineData("{|CS0619:[|Assert.ReferenceEquals(1, data)|]|}", "Assert.Same(1, data)")]
	public async void ConvertsObjectCallToAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}
}
