using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;

public class BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllNonBooleanEqualityChecks()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var value = 5;

					{|xUnit2024:Assert.True(value == 5)|};
					{|xUnit2024:Assert.True(value != 5)|};
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var value = 5;

					Assert.Equal(5, value);
					Assert.NotEqual(5, value);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckNonBooleanFixer.Key_UseSuggestedAssert);
	}
}
