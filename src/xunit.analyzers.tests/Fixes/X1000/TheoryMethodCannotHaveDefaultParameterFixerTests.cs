using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Xunit.Analyzers.Fixes;
using Verify_v2_Pre220 = CSharpVerifier<TheoryMethodCannotHaveDefaultParameterFixerTests.Analyzer>;

public class TheoryMethodCannotHaveDefaultParameterFixerTests
{
	[Fact]
	public async Task FixAll_RemovesDefaultParameterValues()
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

		await Verify_v2_Pre220.VerifyCodeFixV2FixAll(before, after, TheoryMethodCannotHaveDefaultParameterFixer.Key_RemoveParameterDefault);
	}

	internal class Analyzer : TheoryMethodCannotHaveDefaultParameter
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 1, 999));
	}
}
