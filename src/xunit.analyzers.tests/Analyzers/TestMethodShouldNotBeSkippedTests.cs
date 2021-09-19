using Microsoft.CodeAnalysis;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodShouldNotBeSkipped>;

public class TestMethodShouldNotBeSkippedTests
{
	[Theory]
	[InlineData("Fact")]
	[InlineData("Theory")]
	public async void DoesNotFindErrorForNotSkippedTest(string attribute)
	{
		var source = $@"
public class TestClass {{
    [Xunit.{attribute}]
    public void TestMethod() {{ }}
}}";

		await Verify.VerifyAnalyzerAsyncV2(source);
	}

	[Theory]
	[InlineData("Fact")]
	[InlineData("Theory")]
	public async void FindsErrorForSkippedTests(string attribute)
	{
		var source = $@"
class TestClass {{
    [Xunit.{attribute}(Skip=""Lazy"")]
    public void TestMethod() {{ }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(3, 13 + attribute.Length, 3, 24 + attribute.Length)
				.WithSeverity(DiagnosticSeverity.Info);

		await Verify.VerifyAnalyzerAsyncV2(source, expected);
	}
}
