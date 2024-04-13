using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassMustBePublic>;

public class TestClassMustBePublicFixerTests
{
	[Theory]
	[InlineData("")]
	[InlineData("internal")]
	public async Task MakesClassPublic(string nonPublicAccessModifier)
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

		await Verify.VerifyCodeFix(before, after, TestClassMustBePublicFixer.Key_MakeTestClassPublic);
	}

	[Fact]
	public async Task ForPartialClassDeclarations_MakesSingleDeclarationPublic()
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

		await Verify.VerifyCodeFix(before, after, TestClassMustBePublicFixer.Key_MakeTestClassPublic);
	}
}
