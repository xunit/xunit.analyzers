using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertStringEqualityCheckShouldNotUseBoolCheck>;

public class AssertStringEqualityCheckShouldNotUseBoolCheckFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllBooleanStringEqualityChecks()
	{
		var before = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = "foo bar baz";

					[|Assert.True("foo bar baz".Equals(data))|];
					[|Assert.False("foo bar baz".Equals(data))|];
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

					Assert.Equal("foo bar baz", data);
					Assert.NotEqual("foo bar baz", data);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertStringEqualityCheckShouldNotUseBoolCheckFixer.Key_UseAlternateAssert);
	}
}
