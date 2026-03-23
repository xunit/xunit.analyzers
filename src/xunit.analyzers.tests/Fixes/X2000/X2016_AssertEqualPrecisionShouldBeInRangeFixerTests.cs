using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualPrecisionShouldBeInRange>;

public class X2016_AssertEqualPrecisionShouldBeInRangeFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					Assert.Equal(10.1d, 10.2d, [|-1|]);
					Assert.Equal(10.1m, 10.2m, [|29|]);
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					Assert.Equal(10.1d, 10.2d, 0);
					Assert.Equal(10.1m, 10.2m, 28);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEqualPrecisionShouldBeInRangeFixer.Key_UsePrecision);
	}
}
