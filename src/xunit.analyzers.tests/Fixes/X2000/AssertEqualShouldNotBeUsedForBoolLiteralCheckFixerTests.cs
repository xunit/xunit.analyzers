using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForBoolLiteralCheck>;

public class AssertEqualShouldNotBeUsedForBoolLiteralCheckFixerTests
{
	[Fact]
	public async Task FixAll_ConvertsAllToBooleanAsserts()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var actual = true;

					[|Assert.Equal(false, actual)|];
					[|Assert.Equal(true, actual)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var actual = true;

					Assert.False(actual);
					Assert.True(actual);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEqualShouldNotBeUsedForBoolLiteralCheckFixer.Key_UseAlternateAssert);
	}
}
