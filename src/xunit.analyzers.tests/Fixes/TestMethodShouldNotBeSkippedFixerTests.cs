using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodShouldNotBeSkipped>;

public class TestMethodShouldNotBeSkippedFixerTests
{
	[Fact]
	public async void RemovesSkipProperty()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Fact([|Skip = ""Don't run this""|])]
    public void TestMethod() { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyCodeFixAsync(before, after);
	}
}
