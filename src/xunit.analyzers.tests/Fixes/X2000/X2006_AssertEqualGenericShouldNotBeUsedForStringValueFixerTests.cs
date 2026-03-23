using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualGenericShouldNotBeUsedForStringValue>;

public class X2006_AssertEqualGenericShouldNotBeUsedForStringValueFixerTests
{
	[Fact]
	public async Task V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					string result = "foo";

					[|Assert.Equal<string>("foo", result)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					string result = "foo";

					Assert.Equal("foo", result);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEqualGenericShouldNotBeUsedForStringValueFixer.Key_UseStringAssertEqual);
	}

	[Fact]
	public async Task V2_and_V3_NonAOT()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					string result = "foo";

					[|Assert.StrictEqual<string>("foo", result)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					string result = "foo";

					Assert.Equal("foo", result);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAllNonAot(before, after, AssertEqualGenericShouldNotBeUsedForStringValueFixer.Key_UseStringAssertEqual);
	}
}
