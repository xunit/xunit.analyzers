using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyCollectionCheckShouldNotBeUsed>;

public class AssertEmptyCollectionCheckShouldNotBeUsedFixerTests
{
	readonly string before = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var collection = new[] { 1, 2, 3 };

        [|Assert.Collection(collection)|];
    }
}";

	[Fact]
	public async void UseEmptyCheck()
	{
		var after = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var collection = new[] { 1, 2, 3 };

        Assert.Empty(collection);
    }
}";

		await Verify.VerifyCodeFixAsync(before, after, 0);
	}

	[Fact]
	public async void AddElementInspector()
	{
		var after = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() {
        var collection = new[] { 1, 2, 3 };

        Assert.Collection(collection, x => { });
    }
}";

		await Verify.VerifyCodeFixAsync(before, after, 1);
	}
}
