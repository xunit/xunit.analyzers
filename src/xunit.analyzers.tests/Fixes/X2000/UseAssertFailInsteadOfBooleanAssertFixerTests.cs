using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.UseAssertFailInsteadOfBooleanAssert>;

public class UseAssertFailInsteadOfBooleanAssertFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllBooleanAssertsWithFail()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					[|Assert.True(false, "message one")|];
					[|Assert.False(true, "message two")|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					Assert.Fail("message one");
					Assert.Fail("message two");
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, UseAssertFailInsteadOfBooleanAssertFixer.Key_UseAssertFail);
	}
}
