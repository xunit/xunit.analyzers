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

	public static TheoryData<string, string, string> AssertExpressionReplacement = new()
	{
		{ "True", "condition == true", "True" },
		{ "True", "condition != true", "False" },
		{ "True", "true == condition", "True" },
		{ "True", "true != condition", "False" },

		{ "True", "condition == false", "False" },
		{ "True", "condition != false", "True" },
		{ "True", "false == condition", "False" },
		{ "True", "false != condition", "True" },

		{ "False", "condition == true", "False" },
		{ "False", "condition != true", "True" },
		{ "False", "true == condition", "False" },
		{ "False", "true != condition", "True" },

		{ "False", "condition == false", "True" },
		{ "False", "condition != false", "False" },
		{ "False", "false == condition", "True" },
		{ "False", "false != condition", "False" },
	};

	[Theory]
	[MemberData(nameof(AssertExpressionReplacement))]
	public async void SimplifiesBooleanAssert(
		string assertion,
		string expression,
		string replacement)
	{
		var before = string.Format(template, $"{{|xUnit2025:Assert.{assertion}({expression})|}}");
		var after = string.Format(template, $"Assert.{replacement}(condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[MemberData(nameof(AssertExpressionReplacement))]
	public async void SimplifiesBooleanAssertWithMessage(
		string assertion,
		string expression,
		string replacement)
	{
		var before = string.Format(template, $"{{|xUnit2025:Assert.{assertion}({expression}, \"message\")|}}");
		var after = string.Format(template, $"Assert.{replacement}(condition, \"message\")");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixer.Key_UseSuggestedAssert);
	}
}
