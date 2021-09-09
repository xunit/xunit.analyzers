using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSameShouldNotBeCalledOnValueTypes>;

public class AssertSameShouldNotBeCalledOnValueTypesTests
{
	public static TheoryData<string> Methods = new()
	{
		Constants.Asserts.Same,
		Constants.Asserts.NotSame,
	};

	[Theory]
	[MemberData(nameof(Methods))]
	public async void FindsWarningForTwoValueParameters(string method)
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
				.WithArguments($"Assert.{method}()", "int");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void FindsWarningForFirstValueParameters(string method)
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
				.WithArguments($"Assert.{method}()", "int");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void FindsWarningForSecondValueParameters(string method)
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
				.WithArguments($"Assert.{method}()", "int");

		await Verify.VerifyAnalyzerAsync(source, expected);
	}
}
