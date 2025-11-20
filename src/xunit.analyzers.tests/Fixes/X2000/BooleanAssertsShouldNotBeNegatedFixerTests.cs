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
	public async Task WithUserMessage_PreservesAdditionalArgument(
	string assertion,
	string replacement)
	{
		var before = string.Format(template, $"[|Assert.{assertion}(!condition, userMessage: \"Custom message\")|]");
		var after = string.Format(template, $"Assert.{replacement}(condition, userMessage: \"Custom message\")");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData("False", "True")]
	[InlineData("True", "False")]
	public async Task WithMultipleArguments_PreservesAllAdditionalArguments(
	string assertion,
	string replacement)
	{
		var before = string.Format(template, $"[|Assert.{assertion}(!condition, \"message {0}\", 42)|]");
		var after = string.Format(template, $"Assert.{replacement}(condition, \"message {0}\", 42)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData("False", "True")]
	[InlineData("True", "False")]
	public async Task WithNamedArguments_PreservesAllArguments(
	string assertion,
	string replacement)
	{
		var before = string.Format(template, $"[|Assert.{assertion}(condition: !condition, userMessage: \"test\")|]");
		var after = string.Format(template, $"Assert.{replacement}(condition: condition, userMessage: \"test\")");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

}
