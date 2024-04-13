using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodShouldNotBeSkipped>;

public class TestMethodShouldNotBeSkippedTests
{
	[Theory]
	[InlineData("Fact")]
	[InlineData("Theory")]
	public async Task DoesNotFindErrorForNotSkippedTest(string attribute)
	{
		var source = $@"
public class TestClass {{
    [Xunit.{attribute}]
    public void TestMethod() {{ }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("Fact")]
	[InlineData("Theory")]
	public async Task FindsErrorForSkippedTests(string attribute)
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

		await Verify.VerifyAnalyzer(source, expected);
	}
}
