using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataShouldBeUniqueWithinTheory>;

public class InlineDataShouldBeUniqueWithinTheoryFixerTests
{
	[Fact]
	public async Task FixAll_RemovesDuplicateData()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1)]
				[[|InlineData(1)|]]
				public void TestMethod1(int x) { }

				[Theory]
				[InlineData("a")]
				[[|InlineData("a")|]]
				public void TestMethod2(string s) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1)]
				public void TestMethod1(int x) { }

				[Theory]
				[InlineData("a")]
				public void TestMethod2(string s) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, InlineDataShouldBeUniqueWithinTheoryFixer.Key_RemoveDuplicateInlineData);
	}
}
