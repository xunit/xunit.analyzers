using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;

public class BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixerTests
{
	[Fact]
	public async Task FixAll_SimplifiesAllBooleanAsserts()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					bool condition = true;

					{|xUnit2025:Assert.True(condition == true)|};
					{|xUnit2025:Assert.True(condition == false)|};
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					bool condition = true;

					Assert.True(condition);
					Assert.False(condition);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixer.Key_UseSuggestedAssert);
	}
}
