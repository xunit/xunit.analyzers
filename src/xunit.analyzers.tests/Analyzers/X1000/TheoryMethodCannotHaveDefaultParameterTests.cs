using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodCannotHaveDefaultParameter>;
using Verify_v2_Pre220 = CSharpVerifier<TheoryMethodCannotHaveDefaultParameterTests.Analyzer_v2_Pre220>;

public class TheoryMethodCannotHaveDefaultParameterTests
{
	[Fact]
	public async Task FindsErrorForTheoryWithDefaultParameter_WhenDefaultValueNotSupported()
	{
		var source = @"
class TestClass {
    [Xunit.Theory]
    public void TestMethod(int a, string b, string c = """") { }
}";
		var expected =
			Verify_v2_Pre220
				.Diagnostic()
				.WithSpan(4, 54, 4, 58)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("TestMethod", "TestClass", "c");

		await Verify_v2_Pre220.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task DoesNotFindErrorForTheoryWithDefaultParameter_WhenDefaultValueSupported()
	{
		var source = @"
class TestClass {
    [Xunit.Theory]
    public void TestMethod(int a, string b, string c = """") { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	internal class Analyzer_v2_Pre220 : TheoryMethodCannotHaveDefaultParameter
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Core(compilation, new Version(2, 1, 999));
	}
}
