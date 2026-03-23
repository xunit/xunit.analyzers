using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldHaveParameters>;

public class X1006_ConvertToFactFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				public void [|TestMethod1|]() { }

				[Theory]
				public void [|TestMethod2|]() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod1() { }

				[Fact]
				public void TestMethod2() { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, ConvertToFactFixer.Key_ConvertToFact);
	}
}
