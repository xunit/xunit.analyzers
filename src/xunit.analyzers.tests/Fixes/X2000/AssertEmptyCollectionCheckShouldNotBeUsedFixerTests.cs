using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyCollectionCheckShouldNotBeUsed>;

public class AssertEmptyCollectionCheckShouldNotBeUsedFixerTests
{
	[Fact]
	public async Task FixAll_UsesEmptyCheck()
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
		var after = /* lang=c#-test */ """
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

		await Verify.VerifyCodeFixFixAll(before, after, AssertEmptyCollectionCheckShouldNotBeUsedFixer.Key_UseAssertEmpty);
	}

	[Fact]
	public async Task FixAll_AddsElementInspector()
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
		var after = /* lang=c#-test */ """
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

		await Verify.VerifyCodeFixFixAll(before, after, AssertEmptyCollectionCheckShouldNotBeUsedFixer.Key_AddElementInspector);
	}
}
