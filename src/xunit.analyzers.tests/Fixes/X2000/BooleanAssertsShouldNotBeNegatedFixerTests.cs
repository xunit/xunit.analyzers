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
	public async Task PreservesComments()
	{
		var before = string.Format(template, "[|Assert.True(/*a*/condition/*b*/, userMessage: \"msg\")|]");
		var after = string.Format(template, "Assert.False(condition/*b*/, userMessage: \"msg\")");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}
}
