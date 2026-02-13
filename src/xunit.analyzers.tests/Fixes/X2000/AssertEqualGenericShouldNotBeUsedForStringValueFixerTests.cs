using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualGenericShouldNotBeUsedForStringValue>;

public class AssertEqualGenericShouldNotBeUsedForStringValueFixerTests
{
	[Fact]
	public async Task FixAll_RemovesGenericFromAllAsserts()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					string result1 = "foo";
					string result2 = "bar";

					[|Assert.Equal<string>("foo", result1)|];
					[|Assert.StrictEqual<string>("bar", result2)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					string result1 = "foo";
					string result2 = "bar";

					Assert.Equal("foo", result1);
					Assert.Equal("bar", result2);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEqualGenericShouldNotBeUsedForStringValueFixer.Key_UseStringAssertEqual);
	}
}
