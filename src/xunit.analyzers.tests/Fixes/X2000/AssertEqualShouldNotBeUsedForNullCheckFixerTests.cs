using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForNullCheck>;

public class AssertEqualShouldNotBeUsedForNullCheckFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllNullChecks()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					int? data = 1;

					[|Assert.Equal(null, data)|];
					[|Assert.NotEqual(null, data)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					int? data = 1;

					Assert.Null(data);
					Assert.NotNull(data);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEqualShouldNotBeUsedForNullCheckFixer.Key_UseAlternateAssert);
	}
}
