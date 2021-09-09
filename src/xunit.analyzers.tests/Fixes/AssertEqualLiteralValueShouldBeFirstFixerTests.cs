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
	// We have to wrap up CS1738 here because we're using a C# 7.2 feature in an older compiler:
	//   error CS1738: Named argument specifications must appear after all fixed arguments have been specified. Please use language version 7.2 or greater to allow non-trailing named arguments.
	[InlineData("Assert.Equal(expected: i, {|CS1738:0|})", "Assert.Equal(expected: 0, {|CS1738:i|})")]
	public async void SwapArguments(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(Template, beforeAssert);
		var after = string.Format(Template, afterAssert);

		await Verify.VerifyCodeFixAsync(before, after);
	}
}
