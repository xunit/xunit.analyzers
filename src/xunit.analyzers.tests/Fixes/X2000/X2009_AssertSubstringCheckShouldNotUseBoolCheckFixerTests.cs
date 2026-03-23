using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSubstringCheckShouldNotUseBoolCheck>;

public class X2009_AssertSubstringCheckShouldNotUseBoolCheckFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = "foo bar baz";

					[|Assert.True(data.Contains("foo"))|];
					[|Assert.True(data.StartsWith("foo"))|];
					[|Assert.True(data.StartsWith("foo", StringComparison.Ordinal))|];
					[|Assert.True(data.EndsWith("foo"))|];
					[|Assert.True(data.EndsWith("foo", StringComparison.OrdinalIgnoreCase))|];

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
					Assert.StartsWith("foo", data);
					Assert.StartsWith("foo", data, StringComparison.Ordinal);
					Assert.EndsWith("foo", data);
					Assert.EndsWith("foo", data, StringComparison.OrdinalIgnoreCase);

					Assert.DoesNotContain("foo", data);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertSubstringCheckShouldNotUseBoolCheckFixer.Key_UseAlternateAssert);
	}
}
