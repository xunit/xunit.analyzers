using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSubstringCheckShouldNotUseBoolCheck>;

public class AssertSubstringCheckShouldNotUseBoolCheckFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllBooleanSubstringChecks()
	{
		var before = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = "foo bar baz";

					[|Assert.True(data.Contains("foo"))|];
					[|Assert.False(data.Contains("foo"))|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = "foo bar baz";

					Assert.Contains("foo", data);
					Assert.DoesNotContain("foo", data);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertSubstringCheckShouldNotUseBoolCheckFixer.Key_UseAlternateAssert);
	}
}
