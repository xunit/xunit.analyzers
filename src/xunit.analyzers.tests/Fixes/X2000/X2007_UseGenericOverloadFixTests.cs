using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldUseGenericOverloadType>;

public class X2007_UseGenericOverloadFixTests
{
	[Fact]
	public async Task V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var result = 123;

					[|Assert.IsType(typeof(int), result)|];
					[|Assert.IsType(typeof(int), result, false)|];
					[|Assert.IsType(typeof(int), result, true)|];

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
					Assert.IsType<int>(result, false);
					Assert.IsType<int>(result, true);

					Assert.IsAssignableFrom<int>(result);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, UseGenericOverloadFix.Key_UseAlternateAssert);
	}
}
