using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;

public class BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixerTests
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
	[InlineData("True", "condition", "==", "true", "True")]
	[InlineData("True", "condition", "!=", "true", "False")]
	[InlineData("True", "true", "==", "condition", "True")]
	[InlineData("True", "true", "!=", "condition", "False")]
	[InlineData("False", "condition", "==", "true", "False")]
	[InlineData("False", "condition", "!=", "true", "True")]
	[InlineData("False", "true", "==", "condition", "False")]
	[InlineData("False", "true", "!=", "condition", "True")]
	public async void SimplifiesBooleanAssert(
		string assertion,
		string first,
		string @operation,
		string second,
		string replacement)
	{
		var before = string.Format(template, $"{{|xUnit2025:Assert.{assertion}({first + @operation + second})|}}");
		var after = string.Format(template, $"Assert.{replacement}(condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData("True", "condition", "==", "true", "True")]
	[InlineData("True", "condition", "!=", "true", "False")]
	[InlineData("True", "true", "==", "condition", "True")]
	[InlineData("True", "true", "!=", "condition", "False")]
	[InlineData("False", "condition", "==", "true", "False")]
	[InlineData("False", "condition", "!=", "true", "True")]
	[InlineData("False", "true", "==", "condition", "False")]
	[InlineData("False", "true", "!=", "condition", "True")]
	public async void SimplifiesBooleanAssertWithMessage(
		string assertion,
		string first,
		string @operation,
		string second,
		string replacement)
	{
		var before = string.Format(template, $"{{|xUnit2025:Assert.{assertion}({first + @operation + second}, \"message\")|}}");
		var after = string.Format(template, $"Assert.{replacement}(condition, \"message\")");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixer.Key_UseSuggestedAssert);
	}
}
