using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.DataAttributeShouldBeUsedOnATheory>;

public class DataAttributeShouldBeUsedOnATheoryFixerTests
{
	[Fact]
	public async void AddsMissingTheoryAttribute()
	{
		var before = @"
using Xunit;

public class TestClass {
    [InlineData]
    public void [|TestMethod|]() { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData]
    public void TestMethod() { }
}";

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}
}
