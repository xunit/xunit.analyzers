using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForNullCheck>;

public class AssertEqualShouldNotBeUsedForNullCheckTests
{
	public static TheoryData<string> Methods_All = new()
	{
		Constants.Asserts.Equal,
		Constants.Asserts.NotEqual,
		Constants.Asserts.StrictEqual,
		Constants.Asserts.NotStrictEqual,
		Constants.Asserts.Same,
		Constants.Asserts.NotSame,
	};
	public static TheoryData<string, string> Methods_Equal_WithReplacement = new()
	{
		{ Constants.Asserts.Equal, Constants.Asserts.Null },
		{ Constants.Asserts.NotEqual, Constants.Asserts.NotNull }
	};

	[Theory]
	[MemberData(nameof(Methods_Equal_WithReplacement))]
	public async void FindsWarning_ForFirstNullLiteral_StringOverload(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        string val = null;
        Xunit.Assert.{method}(null, val);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 33 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_Equal_WithReplacement))]
	public async void FindsWarning_ForFirstNullLiteral_StringOverload_WithCustomComparer(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        string val = null;
        Xunit.Assert.{method}(null, val, System.StringComparer.Ordinal);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 64 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.Equal, Constants.Asserts.Null)]
	[InlineData(Constants.Asserts.StrictEqual, Constants.Asserts.Null)]
	[InlineData(Constants.Asserts.Same, Constants.Asserts.Null)]
	[InlineData(Constants.Asserts.NotEqual, Constants.Asserts.NotNull)]
	[InlineData(Constants.Asserts.NotStrictEqual, Constants.Asserts.NotNull)]
	[InlineData(Constants.Asserts.NotSame, Constants.Asserts.NotNull)]
	public async void FindsWarning_ForFirstNullLiteral_ObjectOverload(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        object val = null;
        Xunit.Assert.{method}(null, val);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 33 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_Equal_WithReplacement))]
	public async void FindsWarning_ForFirstNullLiteral_ObjectOverload_WithCustomComparer(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        object val = null;
        Xunit.Assert.{method}(null, val, System.Collections.Generic.EqualityComparer<object>.Default);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 94 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.Equal, Constants.Asserts.Null)]
	[InlineData(Constants.Asserts.NotEqual, Constants.Asserts.NotNull)]
	[InlineData(Constants.Asserts.StrictEqual, Constants.Asserts.Null)]
	[InlineData(Constants.Asserts.NotStrictEqual, Constants.Asserts.NotNull)]
	public async void FindsWarning_ForFirstNullLiteral_GenericOverload(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        TestClass val = null;
        Xunit.Assert.{method}<TestClass>(null, val);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 44 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_Equal_WithReplacement))]
	public async void FindsWarning_ForFirstNullLiteral_GenericOverload_WithCustomComparer(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        TestClass val = null;
        Xunit.Assert.{method}<TestClass>(null, val, System.Collections.Generic.EqualityComparer<TestClass>.Default);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 108 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_All))]
	public async void DoesNotFindWarning_ForOtherLiteral(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        int val = 1;
        Xunit.Assert.{method}(1, val);
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods_All))]
	public async void DoesNotFindWarning_ForSecondNullLiteral(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        string val = null;
        Xunit.Assert.{method}(val, null);
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}
}
