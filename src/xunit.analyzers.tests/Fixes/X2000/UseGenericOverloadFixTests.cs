using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify_X2007 = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldUseGenericOverloadType>;

public class UseGenericOverloadFixTests
{
	[Fact]
	public async Task FixAll_SwitchesAllToGenericOverloads()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var result = 123;

					[|Assert.IsType(typeof(int), result)|];
					[|Assert.IsAssignableFrom(typeof(int), result)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var result = 123;

					Assert.IsType<int>(result);
					Assert.IsAssignableFrom<int>(result);
				}
			}
			""";

		await Verify_X2007.VerifyCodeFixFixAll(before, after, UseGenericOverloadFix.Key_UseAlternateAssert);
	}
}
