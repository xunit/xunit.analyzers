using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertNullShouldNotBeCalledOnValueTypes>;

public class AssertNullShouldNotBeCalledOnValueTypesFixerTests
{
	[Fact]
	public async Task FixAll_RemovesAllValueTypeNullAssertions()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					int i = 1;
					bool b = true;

					[|Assert.NotNull(i)|];
					[|Assert.NotNull(b)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					int i = 1;
					bool b = true;
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertNullShouldNotBeCalledOnValueTypesFixer.Key_RemoveAssert);
	}
}
