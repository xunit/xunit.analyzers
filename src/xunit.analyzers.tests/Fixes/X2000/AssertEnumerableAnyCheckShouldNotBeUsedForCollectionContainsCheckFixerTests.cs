using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck>;

public class AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheckFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllAsserts()
	{
		var before = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var collection = new[] { 1, 2, 3 };

					[|Assert.True(collection.Any(x => x == 2))|];
					[|Assert.False(collection.Any(x => x == 2))|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var collection = new[] { 1, 2, 3 };

					Assert.Contains(collection, x => x == 2);
					Assert.DoesNotContain(collection, x => x == 2);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheckFixer.Key_UseAlternateAssert);
	}
}
