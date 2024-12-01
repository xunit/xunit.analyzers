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
	public async Task RemovesDefaultParameterValue()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1)]
				public void TestMethod(int _ [|= 0|]) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1)]
				public void TestMethod(int _) { }
			}
			""";

		await Verify_v2_Pre220.VerifyCodeFix(before, after, TheoryMethodCannotHaveDefaultParameterFixer.Key_RemoveParameterDefault);
	}

	internal class Analyzer : TheoryMethodCannotHaveDefaultParameter
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 1, 999));
	}
}
