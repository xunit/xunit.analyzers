using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualsShouldNotBeUsed>;

public class AssertEqualsShouldNotBeUsedFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllEqualsAndReferenceEqualsCalls()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = 1;

					{|CS0619:[|Assert.Equals(1, data)|]|};
					{|CS0619:[|Assert.ReferenceEquals(1, data)|]|};
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
					Assert.Same(1, data);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEqualsShouldNotBeUsedFixer.Key_UseAlternateAssert);
	}
}
