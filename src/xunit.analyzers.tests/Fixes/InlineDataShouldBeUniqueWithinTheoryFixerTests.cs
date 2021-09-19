using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataShouldBeUniqueWithinTheory>;

public class InlineDataShouldBeUniqueWithinTheoryFixerTests
{
	[Fact]
	public async void RemovesDuplicateData()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    [[|InlineData(1)|]]
    public void TestMethod(int x) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    public void TestMethod(int x) { }
}";

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}
}
