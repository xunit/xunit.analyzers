using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;

public class BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixerTests
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

	public static TheoryData<string, string, string> AssertExpressionReplacement = new()
	{
		/* lang=c#-test */ { "True", "condition == true", "True" },
		/* lang=c#-test */ { "True", "condition != true", "False" },
		/* lang=c#-test */ { "True", "true == condition", "True" },
		/* lang=c#-test */ { "True", "true != condition", "False" },

		/* lang=c#-test */ { "True", "condition == false", "False" },
		/* lang=c#-test */ { "True", "condition != false", "True" },
		/* lang=c#-test */ { "True", "false == condition", "False" },
		/* lang=c#-test */ { "True", "false != condition", "True" },

		/* lang=c#-test */ { "False", "condition == true", "False" },
		/* lang=c#-test */ { "False", "condition != true", "True" },
		/* lang=c#-test */ { "False", "true == condition", "False" },
		/* lang=c#-test */ { "False", "true != condition", "True" },

		/* lang=c#-test */ { "False", "condition == false", "True" },
		/* lang=c#-test */ { "False", "condition != false", "False" },
		/* lang=c#-test */ { "False", "false == condition", "True" },
		/* lang=c#-test */ { "False", "false != condition", "False" },
	};

	[Theory]
	[MemberData(nameof(AssertExpressionReplacement))]
	public async Task SimplifiesBooleanAssert(
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
	public async Task SimplifiesBooleanAssertWithMessage(
		string assertion,
		string expression,
		string replacement)
	{
		var before = string.Format(template, $"{{|xUnit2025:Assert.{assertion}({expression}, \"message\")|}}");
		var after = string.Format(template, $"Assert.{replacement}(condition, \"message\")");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixer.Key_UseSuggestedAssert);
	}
}
