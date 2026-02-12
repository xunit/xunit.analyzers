using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSameShouldNotBeCalledOnValueTypes>;

public class AssertSameShouldNotBeCalledOnValueTypesFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllSameCallsWithEqual()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = 1;

					[|Assert.Same(1, data)|];
					[|Assert.NotSame(1, data)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = 1;

					Assert.Equal(1, data);
					Assert.NotEqual(1, data);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertSameShouldNotBeCalledOnValueTypesFixer.Key_UseAlternateAssert);
	}
}
