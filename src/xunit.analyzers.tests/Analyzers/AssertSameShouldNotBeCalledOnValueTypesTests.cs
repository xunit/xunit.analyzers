using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSameShouldNotBeCalledOnValueTypes>;

public class AssertSameShouldNotBeCalledOnValueTypesTests
{
	public static TheoryData<string, string> Methods_WithReplacement = new()
	{
		{ Constants.Asserts.Same, Constants.Asserts.Equal },
		{ Constants.Asserts.NotSame, Constants.Asserts.NotEqual },
	};

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async void FindsWarningForTwoValueParameters(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        int a = 0;
        Xunit.Assert.{method}(0, a);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 28 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", "int", replacement);

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async void FindsWarningForFirstValueParameters(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        object a = 0;
        Xunit.Assert.{method}(0, a);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 28 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", "int", replacement);

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async void FindsWarningForSecondValueParameters(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        object a = 0;
        Xunit.Assert.{method}(a, 0);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(5, 9, 5, 28 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", "int", replacement);

		await Verify.VerifyAnalyzerAsync(source, expected);
	}
}
