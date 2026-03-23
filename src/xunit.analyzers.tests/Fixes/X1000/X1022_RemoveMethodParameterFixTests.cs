using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<X1022_RemoveMethodParameterFixTests.Analyzer_X1022>;

public class X1022_RemoveMethodParameterFixTests
{
	[Fact]
	public async ValueTask V2_only()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1, 2, 3)]
				public void TestMethod([|params int[] values|]) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1, 2, 3)]
				public void TestMethod() { }
			}
			""";

		await Verify.VerifyCodeFixV2(before, after, RemoveMethodParameterFix.Key_RemoveParameter);
	}

	internal class Analyzer_X1022 : TheoryMethodCannotHaveParamsArray
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 1, 999));
	}
}
