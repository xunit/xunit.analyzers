using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForCollectionSizeCheck>;

public class X2013_AssertEqualShouldNotBeUsedForCollectionSizeCheckFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = new[] { 1, 2, 3 };

					[|Assert.Equal(1, data.Count())|];
					[|Assert.Equal(0, data.Count())|];
					[|Assert.NotEqual(0, data.Count())|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = new[] { 1, 2, 3 };

					Assert.Single(data);
					Assert.Empty(data);
					Assert.NotEmpty(data);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEqualShouldNotBeUsedForCollectionSizeCheckFixer.Key_UseAlternateAssert);
	}
}
