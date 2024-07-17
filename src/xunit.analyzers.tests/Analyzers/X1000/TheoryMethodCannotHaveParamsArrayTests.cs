using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodCannotHaveParamsArray>;
using Verify_v2_Pre220 = CSharpVerifier<TheoryMethodCannotHaveParamsArrayTests.Analyzer_v2_Pre220>;

public class TheoryMethodCannotHaveParamsArrayTests
{
	[Fact]
	public async Task TheoryWithParamsArrayAsync_WhenParamsArrayNotSupported_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    [Xunit.Theory]
			    public void TestMethod(int a, string b, {|#0:params string[] c|}) { }
			}
			""";
		var expected = Verify_v2_Pre220.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass", "c");

		await Verify_v2_Pre220.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task TheoryWithParamsArrayAsync_WhenParamsArraySupported_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    [Xunit.Theory]
			    public void TestMethod(int a, string b, params string[] c) { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task TheoryWithNonParamsArrayAsync_WhenParamsArrayNotSupported_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    [Xunit.Theory]
			    public void TestMethod(int a, string b, string[] c) { }
			}
			""";

		await Verify_v2_Pre220.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task TheoryWithNonParamsArrayAsync_WhenParamsArraySupported_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    [Xunit.Theory]
			    public void TestMethod(int a, string b, string[] c) { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	internal class Analyzer_v2_Pre220 : TheoryMethodCannotHaveParamsArray
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Core(compilation, new Version(2, 1, 999));
	}
}
