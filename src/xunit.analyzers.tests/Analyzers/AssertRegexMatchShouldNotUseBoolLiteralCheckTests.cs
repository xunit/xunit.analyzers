using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertRegexMatchShouldNotUseBoolLiteralCheck>;

public class AssertRegexMatchShouldNotUseBoolLiteralCheckTests
{
	public static TheoryData<string> Methods = new()
	{
		Constants.Asserts.True,
		Constants.Asserts.False,
	};

	[Theory]
	[MemberData(nameof(Methods))]
	public async void FindsWarning_ForStaticRegexIsMatch(string method)
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
				.WithArguments($"Assert.{method}()");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void FindsWarning_ForInstanceRegexIsMatchWithInlineConstructedRegex(string method)
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
				.WithArguments($"Assert.{method}()");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void FindsWarning_ForInstanceRegexIsMatchWithConstructedRegexVariable(string method)
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
				.WithArguments($"Assert.{method}()");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}
}
