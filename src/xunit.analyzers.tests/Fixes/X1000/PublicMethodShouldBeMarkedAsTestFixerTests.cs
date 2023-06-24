using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.PublicMethodShouldBeMarkedAsTest>;

public class PublicMethodShouldBeMarkedAsTestFixerTests
{
	const string beforeNoParams = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() { }

    public void [|TestMethod2|]() { }
}";
	const string beforeWithParams = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() { }

    public void [|TestMethod2|](int _) { }
}";

	[Fact]
	public async void AddsFactToPublicMethodWithoutParameters()
	{
		var after = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() { }

    [Fact]
    public void TestMethod2() { }
}";

		await Verify.VerifyCodeFixAsyncV2(beforeNoParams, after, PublicMethodShouldBeMarkedAsTestFixer.Key_ConvertToFact);
	}

	[Fact]
	public async void AddsFactToPublicMethodWithParameters()
	{
		var after = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() { }

    [Theory]
    public void TestMethod2(int _) { }
}";

		await Verify.VerifyCodeFixAsyncV2(beforeWithParams, after, PublicMethodShouldBeMarkedAsTestFixer.Key_ConvertToTheory);
	}

	[Theory]
	[InlineData(beforeNoParams)]
	[InlineData(beforeWithParams)]
	public async void MarksMethodAsInternal(string before)
	{
		var after = before.Replace("public void [|TestMethod2|]", "internal void TestMethod2");

		await Verify.VerifyCodeFixAsyncV2(before, after, PublicMethodShouldBeMarkedAsTestFixer.Key_MakeMethodInternal);
	}
}
