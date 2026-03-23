using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyCollectionCheckShouldNotBeUsed>;

public class X2011_AssertEmptyCollectionCheckShouldNotBeUsedFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var collection1 = new[] { 1, 2, 3 };
					var collection2 = new[] { 4, 5, 6 };

					[|Assert.Collection(collection1)|];
					[|Assert.Collection(collection2)|];
				}
			}
			""";
		var afterAssertEmpty = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var collection1 = new[] { 1, 2, 3 };
					var collection2 = new[] { 4, 5, 6 };

					Assert.Empty(collection1);
					Assert.Empty(collection2);
				}
			}
			""";
		var afterElementInspector = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var collection1 = new[] { 1, 2, 3 };
					var collection2 = new[] { 4, 5, 6 };

					Assert.Collection(collection1, x => { });
					Assert.Collection(collection2, x => { });
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, afterAssertEmpty, AssertEmptyCollectionCheckShouldNotBeUsedFixer.Key_UseAssertEmpty);
		await Verify.VerifyCodeFixFixAll(before, afterElementInspector, AssertEmptyCollectionCheckShouldNotBeUsedFixer.Key_AddElementInspector);
	}
}
