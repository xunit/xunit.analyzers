using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;

public class BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixerTests
{
	const string template = /* lang=c#-test */ """
		using Xunit;

		public enum MyEnum {{ None, Bacon, Veggie }}

		public class TestClass {{
			[Fact]
			public void TestMethod() {{
				{0} value = {1};

				{2};
			}}
		}}
		""";

	public static MatrixTheoryData<string, string, string> MethodOperatorValue =
		new(
			[Constants.Asserts.True, Constants.Asserts.False],
			["==", "!="],
			["\"bacon\"", "'5'", "5", "5l", "5.0d", "5.0f", "5.0m", "MyEnum.Bacon"]
		);

	[Theory]
	[MemberData(nameof(MethodOperatorValue))]
	public async Task BooleanAssertAgainstLiteralValue_ReplaceWithEquality(
		string method,
		string @operator,
		string value)
	{
		var replacement =
			(method, @operator) switch
			{
				(Constants.Asserts.True, "==") or (Constants.Asserts.False, "!=") => Constants.Asserts.Equal,
				(_, _) => Constants.Asserts.NotEqual,
			};

		// Literal on the right
		await Verify.VerifyCodeFix(
			string.Format(template, "var", value, $"{{|xUnit2024:Assert.{method}(value {@operator} {value})|}}"),
			string.Format(template, "var", value, $"Assert.{replacement}({value}, value)"),
			BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert
		);

		// Literal on the left
		await Verify.VerifyCodeFix(
			string.Format(template, "var", value, $"{{|xUnit2024:Assert.{method}({value} {@operator} value)|}}"),
			string.Format(template, "var", value, $"Assert.{replacement}({value}, value)"),
			BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert
		);
	}

	public static MatrixTheoryData<string, string, string> MethodOperatorType =
		new(
			[Constants.Asserts.True, Constants.Asserts.False],
			["==", "!="],
			["string", "int", "object", "MyEnum"]
		);

	[Theory]
	[MemberData(nameof(MethodOperatorType))]
	public async Task BooleanAssertAgainstNull_ReplaceWithNull(
		string method,
		string @operator,
		string type)
	{
		var replacement =
			(method, @operator) switch
			{
				(Constants.Asserts.True, "==") or (Constants.Asserts.False, "!=") => Constants.Asserts.Null,
				(_, _) => Constants.Asserts.NotNull,
			};

		// Null on the right
		await Verify.VerifyCodeFix(
			LanguageVersion.CSharp8,
			string.Format(template, type + "?", "null", $"{{|xUnit2024:Assert.{method}(value {@operator} null)|}}"),
			string.Format(template, type + "?", "null", $"Assert.{replacement}(value)"),
			BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert
		);

		// Null on the left
		await Verify.VerifyCodeFix(
			LanguageVersion.CSharp8,
			string.Format(template, type + "?", "null", $"{{|xUnit2024:Assert.{method}(null {@operator} value)|}}"),
			string.Format(template, type + "?", "null", $"Assert.{replacement}(value)"),
			BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert
		);
	}
}
