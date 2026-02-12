using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualLiteralValueShouldBeFirst>;

public class AssertEqualLiteralValueShouldBeFirstFixerTests
{
	[Fact]
	public async Task FixAll_SwapsAllArguments()
	{
		var before = /* lang=c#-test */ """
			public class TestClass {
				[Xunit.Fact]
				public void TestMethod() {
					var i = 0;
					var j = 1;

					[|Xunit.Assert.Equal(i, 0)|];
					[|Xunit.Assert.Equal(j, 1)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			public class TestClass {
				[Xunit.Fact]
				public void TestMethod() {
					var i = 0;
					var j = 1;

					Xunit.Assert.Equal(0, i);
					Xunit.Assert.Equal(1, j);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEqualLiteralValueShouldBeFirstFixer.Key_SwapArguments);
	}
}
