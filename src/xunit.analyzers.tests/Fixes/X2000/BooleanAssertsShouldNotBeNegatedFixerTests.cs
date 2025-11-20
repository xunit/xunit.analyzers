using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeNegated>;

public class BooleanAssertsShouldNotBeNegatedFixerTests
{
	const string template = /* lang=c#-test */ """
        using Xunit;

        public class TestClass {{
            [Fact]
            public void TestMethod() {{
                bool condition = true;
                bool a = true;
                bool b = false;

                {0};
            }}
        }}
        """;

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

	[Theory]
	[InlineData("False", "True")]
	[InlineData("True", "False")]
	public async Task PreservesUserMessageNamedParameter(
		string assertion,
		string replacement)
	{
		var before = string.Format(template, $"[|Assert.{assertion}(!condition, userMessage: \"test message\")|]");
		var after = string.Format(template, $"Assert.{replacement}(condition, userMessage: \"test message\")");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData("False", "True")]
	[InlineData("True", "False")]
	public async Task PreservesUserMessagePositionalParameter(
		string assertion,
		string replacement)
	{
		var before = string.Format(template, $"[|Assert.{assertion}(!condition, \"test message\")|]");
		var after = string.Format(template, $"Assert.{replacement}(condition, \"test message\")");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

	[Fact]
	public async Task ParenthesizedExpression()
	{
		var before = string.Format(template, "[|Assert.True(!(a && b))|]");
		var after = string.Format(template, "Assert.False(a && b)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

	[Fact]
	public async Task ComplexExpression()
	{
		var before = string.Format(template, "[|Assert.True(!(a || b && condition))|]");
		var after = string.Format(template, "Assert.False(a || b && condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

	[Fact]
	public async Task PreservesWhitespace()
	{
		var before = string.Format(template, "[|Assert.True(   !condition   )|]");
		var after = string.Format(template, "Assert.False(condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

	[Fact]
	public async Task NegatedLiteral()
	{
		var before = string.Format(template, "[|Assert.True(!false)|]");
		var after = string.Format(template, "Assert.False(false)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

	[Fact]
	public async Task NegatedMethodCall()
	{
		var before = string.Format(template, "[|Assert.True(!IsValid())|]");
		var after = string.Format(template, "Assert.False(IsValid())");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

	[Fact]
	public async Task NegatedPropertyAccess()
	{
		var before = string.Format(template, "[|Assert.True(!condition)|]");
		var after = string.Format(template, "Assert.False(condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

	[Fact]
	public async Task PreservesUserMessageWithComplexExpression()
	{
		var before = string.Format(template, "[|Assert.True(!false, userMessage: \"test\")|]");
		var after = string.Format(template, "Assert.False(false, userMessage: \"test\")");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

	[Fact]
	public async Task PreservesEmptyUserMessage()
	{
		var before = string.Format(template, "[|Assert.True(!condition, userMessage: \"\")|]");
		var after = string.Format(template, "Assert.False(condition, userMessage: \"\")");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

	[Fact]
	public async Task PreservesComments()
	{
		var before = string.Format(template, "[|Assert.True(/*a*/!condition/*b*/, userMessage: \"msg\")|]");
		var after = string.Format(template, "Assert.False(/*a*/condition/*b*/, userMessage: \"msg\")");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}
}
