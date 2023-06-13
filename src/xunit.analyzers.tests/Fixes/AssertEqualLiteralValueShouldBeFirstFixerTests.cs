using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualLiteralValueShouldBeFirst>;

public class AssertEqualLiteralValueShouldBeFirstFixerTests
{
	static readonly string Template = @"
public class TestClass {{
    [Xunit.Fact]
    public void TestMethod() {{
        var i = 0;
        [|Xunit.{0}|];
    }}
}}";

	[Theory]
	[InlineData("Assert.Equal(i, 0)", "Assert.Equal(0, i)")]
	[InlineData("Assert.Equal(actual: 0, expected: i)", "Assert.Equal(actual: i, expected: 0)")]
	[InlineData("Assert.Equal(expected: i, actual: 0)", "Assert.Equal(expected: 0, actual: i)")]
	[InlineData("Assert.Equal(comparer: null, actual: 0, expected: i)", "Assert.Equal(comparer: null, actual: i, expected: 0)")]
	[InlineData("Assert.Equal(expected: i, 0)", "Assert.Equal(expected: 0, i)", LanguageVersion.CSharp7_2)]
	public async void SwapArguments(
		string beforeAssert,
		string afterAssert,
		LanguageVersion? languageVersion = null)
	{
		var before = string.Format(Template, beforeAssert);
		var after = string.Format(Template, afterAssert);

		if (languageVersion.HasValue)
			await Verify.VerifyCodeFixAsyncV2(languageVersion.Value, before, after);
		else
			await Verify.VerifyCodeFixAsyncV2(before, after);
	}
}
