using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertRegexMatchShouldNotUseBoolLiteralCheck>;

public class AssertRegexMatchShouldNotUseBoolLiteralCheckFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllBooleanRegexChecks()
	{
		var before = /* lang=c#-test */ """
			using System.Text.RegularExpressions;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var result = "foo bar baz";

					[|Assert.True(Regex.IsMatch(result, "foo (.*?) baz"))|];
					[|Assert.False(Regex.IsMatch(result, "foo (.*?) baz"))|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Text.RegularExpressions;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var result = "foo bar baz";

					Assert.Matches("foo (.*?) baz", result);
					Assert.DoesNotMatch("foo (.*?) baz", result);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertRegexMatchShouldNotUseBoolLiteralCheckFixer.Key_UseAlternateAssert);
	}
}
