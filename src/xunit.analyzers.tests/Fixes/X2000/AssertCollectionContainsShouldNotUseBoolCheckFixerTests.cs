using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertCollectionContainsShouldNotUseBoolCheck>;

public class AssertCollectionContainsShouldNotUseBoolCheckFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllBooleanAsserts()
	{
		var before = /* lang=c#-test */ """
			using System;
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var items = new[] { "a", "b", "c" };

					[|Assert.True(items.Contains("b"))|];
					[|Assert.False(items.Contains("b"))|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var items = new[] { "a", "b", "c" };

					Assert.Contains("b", items);
					Assert.DoesNotContain("b", items);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertCollectionContainsShouldNotUseBoolCheckFixer.Key_UseAlternateAssert);
	}
}
