using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.FactMethodMustNotHaveParameters>;

public class X1001_ConvertToTheoryFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void [|TestMethod1|](int a) { }

				[Fact]
				public void [|TestMethod2|](string b) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				public void TestMethod1(int a) { }

				[Theory]
				public void TestMethod2(string b) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, ConvertToTheoryFixer.Key_ConvertToTheory);
	}
}
