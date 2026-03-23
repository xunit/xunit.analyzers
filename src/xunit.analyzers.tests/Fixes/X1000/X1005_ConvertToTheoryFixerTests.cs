using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.FactMethodShouldNotHaveTestData>;

public class X1005_ConvertToTheoryFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				[InlineData(1)]
				public void [|TestMethod1|]() { }

				[Fact]
				[InlineData("hello")]
				public void [|TestMethod2|]() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1)]
				public void TestMethod1() { }

				[Theory]
				[InlineData("hello")]
				public void TestMethod2() { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, ConvertToTheoryFixer.Key_ConvertToTheory);
	}
}
