using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertRegexMatchShouldNotUseBoolLiteralCheck>;

public class AssertRegexMatchShouldNotUseBoolLiteralCheckTests
{
	public static TheoryData<string, string> Methods_WithReplacement = new()
	{
		{ Constants.Asserts.True, Constants.Asserts.Matches },
		{ Constants.Asserts.False, Constants.Asserts.DoesNotMatch },
	};

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async Task FindsWarning_ForStaticRegexIsMatch(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(System.Text.RegularExpressions.Regex.IsMatch(""abc"", ""\\w*""));
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 9, 4, 83 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async Task FindsWarning_ForInstanceRegexIsMatchWithInlineConstructedRegex(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(new System.Text.RegularExpressions.Regex(""abc"").IsMatch(""\\w*""));
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 9, 4, 87 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async Task FindsWarning_ForInstanceRegexIsMatchWithConstructedRegexVariable(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        var regex = new System.Text.RegularExpressions.Regex(""abc"");
        Xunit.Assert.{method}(regex.IsMatch(""\\w*""));
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 45 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}
}
