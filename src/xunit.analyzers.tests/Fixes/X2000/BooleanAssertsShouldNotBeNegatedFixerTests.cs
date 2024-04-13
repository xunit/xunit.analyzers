using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeNegated>;

public class BooleanAssertsShouldNotBeNegatedFixerTests
{
	const string template = @"
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        bool condition = true;

        {0};
    }}
}}";

	[Theory]
	[InlineData("False", "True")]
	[InlineData("True", "False")]
	public async Task ReplacesBooleanAssert(
		string assertion,
		string replacement)
	{
		var before = string.Format(template, $"[|Assert.{assertion}(!condition)|]");
		var after = string.Format(template, $"Assert.{replacement}(condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}
}
