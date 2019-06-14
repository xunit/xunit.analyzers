namespace Xunit.Analyzers
{
    using Verify = CSharpVerifier<TestClassMustBePublic>;

    public class TestClassMustBePublicFixerTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("internal")]
        public async void MakesClassPublic(string nonPublicAccessModifier)
        {
            var source = $"{nonPublicAccessModifier} class [|TestClass|] {{ [Xunit.Fact] public void TestMethod() {{ }} }}";

            var fixedSource = "public class TestClass { [Xunit.Fact] public void TestMethod() { } }";

            await Verify.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async void ForPartialClassDeclarations_MakesSingleDeclarationPublic()
        {
            var source = @"
partial class [|TestClass|]
{
    [Xunit.Fact]
    public void TestMethod1() {}
}

partial class TestClass
{
    [Xunit.Fact]
    public void TestMethod2() {}
}";

            var fixedSource = @"
public partial class TestClass
{
    [Xunit.Fact]
    public void TestMethod1() {}
}

partial class TestClass
{
    [Xunit.Fact]
    public void TestMethod2() {}
}";

            await Verify.VerifyCodeFixAsync(source, fixedSource);
        }
    }
}
