using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForCollectionSizeCheck>;

public class AssertEqualShouldNotBeUsedForCollectionSizeCheckFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllCollectionSizeChecks()
	{
		var before = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = new[] { 1, 2, 3 };

					[|Assert.Equal(1, data.Count())|];
					[|Assert.Equal(0, data.Count())|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = new[] { 1, 2, 3 };

					Assert.Single(data);
					Assert.Empty(data);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEqualShouldNotBeUsedForCollectionSizeCheckFixer.Key_UseAlternateAssert);
	}
}
