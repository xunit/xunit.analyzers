using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyCollectionCheckShouldNotBeUsed>;

public class AssertEmptyCollectionCheckShouldNotBeUsedFixerTests
{
	const string before = /* lang=c#-test */ """
		using Xunit;

		public class TestClass {
		    [Fact]
		    public void TestMethod() {
		        var collection = new[] { 1, 2, 3 };

		        [|Assert.Collection(collection)|];
		    }
		}
		""";

	[Fact]
	public async Task UseEmptyCheck()
	{
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Fact]
			    public void TestMethod() {
			        var collection = new[] { 1, 2, 3 };

			        Assert.Empty(collection);
			    }
			}
			""";

		await Verify.VerifyCodeFix(before, after, AssertEmptyCollectionCheckShouldNotBeUsedFixer.Key_UseAssertEmpty);
	}

	[Fact]
	public async Task AddElementInspector()
	{
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Fact]
			    public void TestMethod() {
			        var collection = new[] { 1, 2, 3 };

			        Assert.Collection(collection, x => { });
			    }
			}
			""";

		await Verify.VerifyCodeFix(before, after, AssertEmptyCollectionCheckShouldNotBeUsedFixer.Key_AddElementInspector);
	}
}
