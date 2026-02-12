using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.FactMethodShouldNotHaveTestData>;

public class FactMethodShouldNotHaveTestDataFixerTests
{
	[Fact]
	public async Task FixAll_RemovesDataAttributesFromAllMethods()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				[InlineData(1)]
				public void [|TestMethod1|](int x) { }

				[Fact]
				[InlineData("a")]
				public void [|TestMethod2|](string s) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod1(int x) { }

				[Fact]
				public void TestMethod2(string s) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, FactMethodShouldNotHaveTestDataFixer.Key_RemoveDataAttributes);
	}
}
