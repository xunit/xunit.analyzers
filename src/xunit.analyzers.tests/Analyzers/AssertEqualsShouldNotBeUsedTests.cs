using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualsShouldNotBeUsed>;

public class AssertEqualsShouldNotBeUsedTests
{
	[Theory]
	[InlineData(nameof(object.Equals), Constants.Asserts.Equal)]
	[InlineData(nameof(object.ReferenceEquals), Constants.Asserts.Same)]
	public async void FindsHiddenDiagnosticWhenProhibitedMethodIsUsed(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(null, null);
    }}
}}";
		var expected = new[]
		{
			Verify
				.CompilerError("CS0619")
				.WithSpan(4, 9, 4, 34 + method.Length)
				.WithMessage($"'Assert.{method}(object, object)' is obsolete: 'This is an override of Object.{method}(). Call Assert.{replacement}() instead.'"),
			Verify
				.Diagnostic()
				.WithSpan(4, 9, 4, 34 + method.Length)
				.WithSeverity(DiagnosticSeverity.Hidden)
				.WithArguments($"Assert.{method}()", replacement),
		};

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}
}
