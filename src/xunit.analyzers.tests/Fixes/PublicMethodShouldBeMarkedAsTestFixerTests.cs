using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.PublicMethodShouldBeMarkedAsTest>;

public class PublicMethodShouldBeMarkedAsTestFixerTests
{
	const string before = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() { }

    public void [|TestMethod2|]() { }
}";

	[Fact]
	public async void AddsFactToPublicMethod()
	{
		var after = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() { }

    [Fact]
    public void TestMethod2() { }
}";

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}

	[Fact]
	public async void MarksMethodAsInternal()
	{
		var after = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() { }

    internal void TestMethod2() { }
}";

		await Verify.VerifyCodeFixAsyncV2(before, after, 1);
	}
}
