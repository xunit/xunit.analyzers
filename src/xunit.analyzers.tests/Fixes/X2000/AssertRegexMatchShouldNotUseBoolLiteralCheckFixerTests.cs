using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertRegexMatchShouldNotUseBoolLiteralCheck>;

public class AssertRegexMatchShouldNotUseBoolLiteralCheckFixerTests
{
	const string template = @"
using System.Text.RegularExpressions;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var result = ""foo bar baz"";

        {0};
    }}
}}";

	[Theory]
	[InlineData(@"[|Assert.True(Regex.IsMatch(result, ""foo (.*?) baz""))|]", @"Assert.Matches(""foo (.*?) baz"", result)")]
	[InlineData(@"[|Assert.False(Regex.IsMatch(result, ""foo (.*?) baz""))|]", @"Assert.DoesNotMatch(""foo (.*?) baz"", result)")]
	public async Task ConvertsBooleanAssertToRegexAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertRegexMatchShouldNotUseBoolLiteralCheckFixer.Key_UseAlternateAssert);
	}
}
