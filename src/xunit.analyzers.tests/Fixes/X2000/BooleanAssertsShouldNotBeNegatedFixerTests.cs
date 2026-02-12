using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeNegated>;

public class BooleanAssertsShouldNotBeNegatedFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllNegatedBooleanAsserts()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					bool condition = true;

					[|Assert.True(!condition)|];
					[|Assert.False(!condition)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					bool condition = true;

					Assert.False(condition);
					Assert.True(condition);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}
}
