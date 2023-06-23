using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassMustBePublic>;

public class TestClassMustBePublicFixerTests
{
	[Theory]
	[InlineData("")]
	[InlineData("internal")]
	public async void MakesClassPublic(string nonPublicAccessModifier)
	{
		var before = $@"
{nonPublicAccessModifier} class [|TestClass|] {{
    [Xunit.Fact]
    public void TestMethod() {{ }}
}}";

		var after = @"
public class TestClass {
    [Xunit.Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}

	[Fact]
	public async void ForPartialClassDeclarations_MakesSingleDeclarationPublic()
	{
		var before = @"
partial class [|TestClass|] {
    [Xunit.Fact]
    public void TestMethod1() {}
}

partial class TestClass {
    [Xunit.Fact]
    public void TestMethod2() {}
}";

		var after = @"
public partial class TestClass {
    [Xunit.Fact]
    public void TestMethod1() {}
}

partial class TestClass {
    [Xunit.Fact]
    public void TestMethod2() {}
}";

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}
}
