using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertCollectionContainsShouldNotUseBoolCheck>;

public class X2017_AssertCollectionContainsShouldNotUseBoolCheckFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
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
					[|Assert.True(items.Contains("b", StringComparer.Ordinal))|];
					[|Assert.True(items.Contains("b", null))|];

					[|Assert.False(items.Contains("b"))|];
					[|Assert.False(items.Contains("b", StringComparer.Ordinal))|];
					[|Assert.False(items.Contains("b", null))|];
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
					Assert.Contains("b", items, StringComparer.Ordinal);
					Assert.Contains("b", items);

					Assert.DoesNotContain("b", items);
					Assert.DoesNotContain("b", items, StringComparer.Ordinal);
					Assert.DoesNotContain("b", items);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertCollectionContainsShouldNotUseBoolCheckFixer.Key_UseAlternateAssert);
	}
}
