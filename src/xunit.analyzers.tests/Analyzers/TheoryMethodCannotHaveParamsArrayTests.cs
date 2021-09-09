using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodCannotHaveParamsArray>;
using Verify_Pre220 = CSharpVerifier<TheoryMethodCannotHaveParamsArrayTests.Analyzer_Pre220>;

public class TheoryMethodCannotHaveParamsArrayTests
{
	[Fact]
	public async Task FindsErrorForTheoryWithParamsArrayAsync_WhenParamsArrayNotSupported()
	{
		var source = @"
class TestClass {
    [Xunit.Theory]
    public void TestMethod(int a, string b, params string[] c) { }
}";
		var expected =
			Verify_Pre220
				.Diagnostic()
				.WithSpan(4, 45, 4, 62)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("TestMethod", "TestClass", "c");

		await Verify_Pre220.VerifyAnalyzerAsync(source, expected);
	}

	[Fact]
	public async Task DoesNotFindErrorForTheoryWithParamsArrayAsync_WhenParamsArraySupported()
	{
		var source = @"
class TestClass {
    [Xunit.Theory]
    public void TestMethod(int a, string b, params string[] c) { }
}";

		await Verify.VerifyAnalyzerAsync(source);
	}

	[Fact]
	public async Task DoesNotFindErrorForTheoryWithNonParamsArrayAsync_WhenParamsArrayNotSupported()
	{
		var source = @"
class TestClass {
    [Xunit.Theory]
    public void TestMethod(int a, string b, string[] c) { }
}";

		await Verify_Pre220.VerifyAnalyzerAsync(source);
	}

	[Fact]
	public async Task DoesNotFindErrorForTheoryWithNonParamsArrayAsync_WhenParamsArraySupported()
	{
		var source = @"
class TestClass {
    [Xunit.Theory]
    public void TestMethod(int a, string b, string[] c) { }
}";

		await Verify.VerifyAnalyzerAsync(source);
	}

	internal class Analyzer_Pre220 : TheoryMethodCannotHaveParamsArray
	{
		public Analyzer_Pre220()
			: base("2.1.999")
		{ }
	}
}
