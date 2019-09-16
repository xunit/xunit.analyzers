using VerifyCS = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TestClassMustBePublic>;
using VerifyVB = Xunit.Analyzers.VisualBasicVerifier<Xunit.Analyzers.TestClassMustBePublic>;

namespace Xunit.Analyzers
{
    public class TestClassMustBePublicFixerTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("internal")]
        public async void MakesClassPublic_CSharp(string nonPublicAccessModifier)
        {
            var source = $"{nonPublicAccessModifier} class [|TestClass|] {{ [Xunit.Fact] public void TestMethod() {{ }} }}";

            var fixedSource = "public class TestClass { [Xunit.Fact] public void TestMethod() { } }";

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Friend")]
        public async void MakesClassPublic_VisualBasic(string nonPublicAccessModifier)
        {
            var source = $@"
{nonPublicAccessModifier} Class [|TestClass|]
    <Xunit.Fact>
    Public Sub TestMethod()
    End Sub
End Class";

            var fixedSource = @"
Public Class TestClass
    <Xunit.Fact>
    Public Sub TestMethod()
    End Sub
End Class";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async void ForPartialClassDeclarations_MakesSingleDeclarationPublic_CSharp()
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

            await VerifyCS.VerifyCodeFixAsync(source, fixedSource);
        }

        [Fact]
        public async void ForPartialClassDeclarations_MakesSingleDeclarationPublic_VisualBasic()
        {
            var source = @"
Partial Class [|TestClass|]
    <Xunit.Fact>
    Public Sub TestMethod1()
    End Sub
End Class

Partial Class TestClass
    <Xunit.Fact>
    Public Sub TestMethod2()
    End Sub
End Class";

            var fixedSource = @"
Public Partial Class TestClass
    <Xunit.Fact>
    Public Sub TestMethod1()
    End Sub
End Class

Partial Class TestClass
    <Xunit.Fact>
    Public Sub TestMethod2()
    End Sub
End Class";

            await VerifyVB.VerifyCodeFixAsync(source, fixedSource);
        }
    }
}
