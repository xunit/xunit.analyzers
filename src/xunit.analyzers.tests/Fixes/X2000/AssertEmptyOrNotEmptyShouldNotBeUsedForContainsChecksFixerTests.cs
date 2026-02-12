using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecks>;

public class AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAssertEmptyWithDoesNotContain()
	{
		var before = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var list = new[] { -1, 0, 1, 2 };

					{|xUnit2029:Assert.Empty(list.Where(f => f > 0))|};
					{|xUnit2029:Assert.Empty(list.Where(n => n == 1))|};
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var list = new[] { -1, 0, 1, 2 };

					Assert.DoesNotContain(list, f => f > 0);
					Assert.DoesNotContain(list, n => n == 1);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixer.Key_UseDoesNotContain);
	}

	[Fact]
	public async Task FixAll_ReplacesAssertNotEmptyWithContains()
	{
		var before = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var list = new[] { -1, 0, 1, 2 };

					{|xUnit2030:Assert.NotEmpty(list.Where(f => f > 0))|};
					{|xUnit2030:Assert.NotEmpty(list.Where(n => n == 1))|};
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var list = new[] { -1, 0, 1, 2 };

					Assert.Contains(list, f => f > 0);
					Assert.Contains(list, n => n == 1);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixer.Key_UseContains);
	}
}
