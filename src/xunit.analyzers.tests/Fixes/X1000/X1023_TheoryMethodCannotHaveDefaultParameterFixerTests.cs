using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<X1023_TheoryMethodCannotHaveDefaultParameterFixerTests.Analyzer>;

public class X1023_TheoryMethodCannotHaveDefaultParameterFixerTests
{
	[Fact]
	public async ValueTask V2_only()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1, "a")]
				public void TestMethod1(int x [|= 0|], string y [|= "default"|]) { }

				[Theory]
				[InlineData(true)]
				public void TestMethod2(bool b [|= false|]) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1, "a")]
				public void TestMethod1(int x, string y) { }

				[Theory]
				[InlineData(true)]
				public void TestMethod2(bool b) { }
			}
			""";

		await Verify.VerifyCodeFixV2FixAll(before, after, TheoryMethodCannotHaveDefaultParameterFixer.Key_RemoveParameterDefault);
	}

	internal class Analyzer : TheoryMethodCannotHaveDefaultParameter
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 1, 999));
	}
}
