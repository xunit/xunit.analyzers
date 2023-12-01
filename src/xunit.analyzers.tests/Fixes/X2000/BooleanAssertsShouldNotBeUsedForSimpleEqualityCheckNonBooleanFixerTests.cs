using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;

public class BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixerTests
{
	const string template = @"
using Xunit;

public enum MyEnum {{ None, Bacon, Veggie }}

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        {0} condition = {1};

        {2};
    }}
}}";

	[Theory]
	[InlineData(Constants.Asserts.True, "string", "==", "\"bacon\"")]
	[InlineData(Constants.Asserts.True, "char", "==", "'5'")]
	[InlineData(Constants.Asserts.True, "int", "==", "5")]
	[InlineData(Constants.Asserts.True, "long", "==", "5l")]
	[InlineData(Constants.Asserts.True, "double", "==", "5.0d")]
	[InlineData(Constants.Asserts.True, "float", "==", "5.0f")]
	[InlineData(Constants.Asserts.True, "decimal", "==", "5.0m")]
	[InlineData(Constants.Asserts.False, "string", "!=", "\"bacon\"")]
	[InlineData(Constants.Asserts.False, "char", "!=", "'5'")]
	[InlineData(Constants.Asserts.False, "int", "!=", "5")]
	[InlineData(Constants.Asserts.False, "long", "!=", "5l")]
	[InlineData(Constants.Asserts.False, "double", "!=", "5.0d")]
	[InlineData(Constants.Asserts.False, "float", "!=", "5.0f")]
	[InlineData(Constants.Asserts.False, "decimal", "!=", "5.0m")]
	public async void ReplacesBooleanAssertWithLiteralOnRight(
		string assertion,
		string type,
		string @operation,
		string value)
	{
		var before = string.Format(template,
			type,
			value,
			$"{{|xUnit2024:Assert.{assertion}(condition {@operation + " " + value})|}}");
		var after = string.Format(template,
			type,
			value,
			$"Assert.Equal({value}, condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "string", "==", "\"bacon\"")]
	[InlineData(Constants.Asserts.True, "char", "==", "'5'")]
	[InlineData(Constants.Asserts.True, "int", "==", "5")]
	[InlineData(Constants.Asserts.True, "long", "==", "5l")]
	[InlineData(Constants.Asserts.True, "double", "==", "5.0d")]
	[InlineData(Constants.Asserts.True, "float", "==", "5.0f")]
	[InlineData(Constants.Asserts.True, "decimal", "==", "5.0m")]
	[InlineData(Constants.Asserts.False, "string", "!=", "\"bacon\"")]
	[InlineData(Constants.Asserts.False, "char", "!=", "'5'")]
	[InlineData(Constants.Asserts.False, "int", "!=", "5")]
	[InlineData(Constants.Asserts.False, "long", "!=", "5l")]
	[InlineData(Constants.Asserts.False, "double", "!=", "5.0d")]
	[InlineData(Constants.Asserts.False, "float", "!=", "5.0f")]
	[InlineData(Constants.Asserts.False, "decimal", "!=", "5.0m")]
	public async void ReplacesBooleanAssertWithLiteralOnLeft(
		string assertion,
		string type,
		string @operation,
		string value)
	{
		var before = string.Format(template,
			type,
			value,
			$"{{|xUnit2024:Assert.{assertion}({value + " " + @operation} condition)|}}");
		var after = string.Format(template,
			type,
			value,
			$"Assert.Equal({value}, condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData(Constants.Asserts.False, "string", "==", "\"bacon\"")]
	[InlineData(Constants.Asserts.False, "char", "==", "'5'")]
	[InlineData(Constants.Asserts.False, "int", "==", "5")]
	[InlineData(Constants.Asserts.False, "long", "==", "5l")]
	[InlineData(Constants.Asserts.False, "double", "==", "5.0d")]
	[InlineData(Constants.Asserts.False, "float", "==", "5.0f")]
	[InlineData(Constants.Asserts.False, "decimal", "==", "5.0m")]
	[InlineData(Constants.Asserts.True, "string", "!=", "\"bacon\"")]
	[InlineData(Constants.Asserts.True, "char", "!=", "'5'")]
	[InlineData(Constants.Asserts.True, "int", "!=", "5")]
	[InlineData(Constants.Asserts.True, "long", "!=", "5l")]
	[InlineData(Constants.Asserts.True, "double", "!=", "5.0d")]
	[InlineData(Constants.Asserts.True, "float", "!=", "5.0f")]
	[InlineData(Constants.Asserts.True, "decimal", "!=", "5.0m")]
	public async void ReplacesNegativeBooleanAssertWithLiteralOnRight(
		string assertion,
		string type,
		string @operation,
		string value)
	{
		var before = string.Format(template,
			type,
			value,
			$"{{|xUnit2024:Assert.{assertion}(condition {@operation + " " + value})|}}");
		var after = string.Format(template,
			type,
			value,
			$"Assert.NotEqual({value}, condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData(Constants.Asserts.False, "string", "==", "\"bacon\"")]
	[InlineData(Constants.Asserts.False, "char", "==", "'5'")]
	[InlineData(Constants.Asserts.False, "int", "==", "5")]
	[InlineData(Constants.Asserts.False, "long", "==", "5l")]
	[InlineData(Constants.Asserts.False, "double", "==", "5.0d")]
	[InlineData(Constants.Asserts.False, "float", "==", "5.0f")]
	[InlineData(Constants.Asserts.False, "decimal", "==", "5.0m")]
	[InlineData(Constants.Asserts.True, "string", "!=", "\"bacon\"")]
	[InlineData(Constants.Asserts.True, "char", "!=", "'5'")]
	[InlineData(Constants.Asserts.True, "int", "!=", "5")]
	[InlineData(Constants.Asserts.True, "long", "!=", "5l")]
	[InlineData(Constants.Asserts.True, "double", "!=", "5.0d")]
	[InlineData(Constants.Asserts.True, "float", "!=", "5.0f")]
	[InlineData(Constants.Asserts.True, "decimal", "!=", "5.0m")]
	public async void ReplacesNegativeBooleanAssertWithLiteralOnLeft(
		string assertion,
		string type,
		string @operation,
		string value)
	{
		var before = string.Format(template,
			type,
			value,
			$"{{|xUnit2024:Assert.{assertion}({value + " " + @operation} condition)|}}");
		var after = string.Format(template,
			type,
			value,
			$"Assert.NotEqual({value}, condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "string", "==")]
	[InlineData(Constants.Asserts.True, "int", "==")]
	[InlineData(Constants.Asserts.True, "object", "==")]
	[InlineData(Constants.Asserts.False, "string", "!=")]
	[InlineData(Constants.Asserts.False, "int", "!=")]
	[InlineData(Constants.Asserts.False, "object", "!=")]
	public async void ReplacesBooleanAssertForNullsWithLiteralOnRight(
		string assertion,
		string type,
		string @operation)
	{
		var before = string.Format(template,
			type + "?",
			"null",
			$"{{|xUnit2024:Assert.{assertion}(condition {@operation} null)|}}");
		var after = string.Format(template,
			type + "?",
			"null",
			$"Assert.Null(condition)");

		await Verify.VerifyCodeFix(
			LanguageVersion.CSharp8, before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "string", "==")]
	[InlineData(Constants.Asserts.True, "int", "==")]
	[InlineData(Constants.Asserts.True, "object", "==")]
	[InlineData(Constants.Asserts.False, "string", "!=")]
	[InlineData(Constants.Asserts.False, "int", "!=")]
	[InlineData(Constants.Asserts.False, "object", "!=")]
	public async void ReplacesBooleanAssertForNullsWithLiteralOnLeft(
		string assertion,
		string type,
		string @operation)
	{
		var before = string.Format(template,
			type + "?",
			"null",
			$"{{|xUnit2024:Assert.{assertion}(null {@operation} condition)|}}");
		var after = string.Format(template,
			type + "?",
			"null",
			$"Assert.Null(condition)");

		await Verify.VerifyCodeFix(
			LanguageVersion.CSharp8, before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData(Constants.Asserts.False, "string", "==")]
	[InlineData(Constants.Asserts.False, "int", "==")]
	[InlineData(Constants.Asserts.False, "object", "==")]
	[InlineData(Constants.Asserts.True, "string", "!=")]
	[InlineData(Constants.Asserts.True, "int", "!=")]
	[InlineData(Constants.Asserts.True, "object", "!=")]
	public async void ReplacesNegativeBooleanAssertForNullsWithLiteralOnRight(
		string assertion,
		string type,
		string @operation)
	{
		var before = string.Format(template,
			type + "?",
			"null",
			$"{{|xUnit2024:Assert.{assertion}(condition {@operation} null)|}}");
		var after = string.Format(template,
			type + "?",
			"null",
			$"Assert.NotNull(condition)");

		await Verify.VerifyCodeFix(
			LanguageVersion.CSharp8, before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData(Constants.Asserts.False, "string", "==")]
	[InlineData(Constants.Asserts.False, "int", "==")]
	[InlineData(Constants.Asserts.False, "object", "==")]
	[InlineData(Constants.Asserts.True, "string", "!=")]
	[InlineData(Constants.Asserts.True, "int", "!=")]
	[InlineData(Constants.Asserts.True, "object", "!=")]
	public async void ReplacesNegativeBooleanAssertForNullsWithLiteralOnLeft(
		string assertion,
		string type,
		string @operation)
	{
		var before = string.Format(template,
			type + "?",
			"null",
			$"{{|xUnit2024:Assert.{assertion}(null {@operation} condition)|}}");
		var after = string.Format(template,
			type + "?",
			"null",
			$"Assert.NotNull(condition)");

		await Verify.VerifyCodeFix(
			LanguageVersion.CSharp8, before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "==")]
	[InlineData(Constants.Asserts.False, "!=")]
	public async void ReplacesBooleanAssertWithEnumLiteralOnRight(
		string assertion,
		string @operation)
	{
		var before = string.Format(template,
			"MyEnum",
			"MyEnum.Bacon",
			$"{{|xUnit2024:Assert.{assertion}(condition {@operation} MyEnum.Bacon)|}}");
		var after = string.Format(template,
			"MyEnum",
			"MyEnum.Bacon",
			$"Assert.Equal(MyEnum.Bacon, condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "==")]
	[InlineData(Constants.Asserts.False, "!=")]
	public async void ReplacesBooleanAssertWithEnumLiteralOnLeft(
		string assertion,
		string @operation)
	{
		var before = string.Format(template,
			"MyEnum",
			"MyEnum.Bacon",
			$"{{|xUnit2024:Assert.{assertion}(MyEnum.Bacon {@operation} condition)|}}");
		var after = string.Format(template,
			"MyEnum",
			"MyEnum.Bacon",
			$"Assert.Equal(MyEnum.Bacon, condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData(Constants.Asserts.False, "==")]
	[InlineData(Constants.Asserts.True, "!=")]
	public async void ReplacesBooleanInequalityAssertWithEnumLiteralOnRight(
		string assertion,
		string @operation)
	{
		var before = string.Format(template,
			"MyEnum",
			"MyEnum.Bacon",
			$"{{|xUnit2024:Assert.{assertion}(condition {@operation} MyEnum.Bacon)|}}");
		var after = string.Format(template,
			"MyEnum",
			"MyEnum.Bacon",
			$"Assert.NotEqual(MyEnum.Bacon, condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}

	[Theory]
	[InlineData(Constants.Asserts.False, "==")]
	[InlineData(Constants.Asserts.True, "!=")]
	public async void ReplacesBooleanInequalityAssertWithEnumLiteralOnLeft(
		string assertion,
		string @operation)
	{
		var before = string.Format(template,
			"MyEnum",
			"MyEnum.Bacon",
			$"{{|xUnit2024:Assert.{assertion}(MyEnum.Bacon {@operation} condition)|}}");
		var after = string.Format(template,
			"MyEnum",
			"MyEnum.Bacon",
			$"Assert.NotEqual(MyEnum.Bacon, condition)");

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}
}
