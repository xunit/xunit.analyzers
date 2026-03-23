using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForBoolLiteralCheck>;

public class X2004_AssertEqualShouldNotBeUsedForBoolLiteralCheckFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var actual = true;

					[|Assert.Equal(false, actual)|];
					[|Assert.Equal(true, actual)|];
					[|Assert.NotEqual(false, actual)|];
					[|Assert.NotEqual(true, actual)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var actual = true;

					Assert.False(actual);
					Assert.True(actual);
					Assert.True(actual);
					Assert.False(actual);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEqualShouldNotBeUsedForBoolLiteralCheckFixer.Key_UseAlternateAssert);
	}

	[Fact]
	public async ValueTask V2_and_V3_NonAOT()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var actual = true;

					[|Assert.StrictEqual(false, actual)|];
					[|Assert.StrictEqual(true, actual)|];
					[|Assert.NotStrictEqual(false, actual)|];
					[|Assert.NotStrictEqual(true, actual)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var actual = true;

					Assert.False(actual);
					Assert.True(actual);
					Assert.True(actual);
					Assert.False(actual);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAllNonAot(before, after, AssertEqualShouldNotBeUsedForBoolLiteralCheckFixer.Key_UseAlternateAssert);
	}
}
