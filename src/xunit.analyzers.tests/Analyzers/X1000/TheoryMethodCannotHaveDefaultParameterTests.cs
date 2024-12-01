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
	public async Task TheoryWithDefaultParameter_WhenDefaultValueNotSupported_Triggers()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
				[Xunit.Theory]
				public void TestMethod(int a, string b, string c {|#0:= ""|}) { }
			}
			""";
		var expected = Verify_v2_Pre220.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass", "c");

		await Verify_v2_Pre220.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task TheoryWithDefaultParameter_WhenDefaultValueSupported_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
				[Xunit.Theory]
				public void TestMethod(int a, string b, string c = "") { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	internal class Analyzer_v2_Pre220 : TheoryMethodCannotHaveDefaultParameter
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 1, 999));
	}
}
