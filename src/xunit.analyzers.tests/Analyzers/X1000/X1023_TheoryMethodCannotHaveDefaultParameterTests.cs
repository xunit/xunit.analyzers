using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodCannotHaveDefaultParameter>;
using Verify_v2_Pre220 = CSharpVerifier<X1023_TheoryMethodCannotHaveDefaultParameterTests.Analyzer_v2_Pre220>;

public class X1023_TheoryMethodCannotHaveDefaultParameterTests
{
	[Fact]
	public async ValueTask V2_Pre220()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				[Theory]
				public void TestMethod(int a, string b, string c {|#0:= ""|}) { }
			}
			""";
		var expected = Verify_v2_Pre220.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass", "c");

		await Verify_v2_Pre220.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				[Theory]
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
