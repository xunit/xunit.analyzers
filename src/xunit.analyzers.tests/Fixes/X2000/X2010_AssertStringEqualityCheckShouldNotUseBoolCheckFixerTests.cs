using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertStringEqualityCheckShouldNotUseBoolCheck>;

public class X2010_AssertStringEqualityCheckShouldNotUseBoolCheckFixerTests
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

					[|Assert.True("foo bar baz".Equals(data))|];
					[|Assert.True("foo bar baz".Equals(data, StringComparison.Ordinal))|];
					[|Assert.True("foo bar baz".Equals(data, StringComparison.OrdinalIgnoreCase))|];
					[|Assert.True(string.Equals("foo bar baz", data))|];
					[|Assert.True(string.Equals("foo bar baz", data, StringComparison.Ordinal))|];
					[|Assert.True(string.Equals("foo bar baz", data, StringComparison.OrdinalIgnoreCase))|];

					[|Assert.False("foo bar baz".Equals(data))|];
					[|Assert.False(string.Equals("foo bar baz", data))|];
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
					Assert.Equal("foo bar baz", data);
					Assert.Equal("foo bar baz", data, ignoreCase: true);
					Assert.Equal("foo bar baz", data);
					Assert.Equal("foo bar baz", data);
					Assert.Equal("foo bar baz", data, ignoreCase: true);

					Assert.NotEqual("foo bar baz", data);
					Assert.NotEqual("foo bar baz", data);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertStringEqualityCheckShouldNotUseBoolCheckFixer.Key_UseAlternateAssert);
	}
}
