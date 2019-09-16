using VerifyCS = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TheoryMethodMustHaveTestData>;
using VerifyVB = Xunit.Analyzers.VisualBasicVerifier<Xunit.Analyzers.TheoryMethodMustHaveTestData>;

namespace Xunit.Analyzers
{
    public class TheoryMethodMustHaveTestDataTests
    {
        [Fact]
        public async void DoesNotFindErrorForFactMethod_CSharp()
        {
            var source = "public class TestClass { [Xunit.Fact] public void TestMethod() { } }";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindErrorForFactMethod_VisualBasic()
        {
            var source = @"
Public Class TestClass
    <Xunit.Fact>
    Public Sub TestMethod()
    End Sub
End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("InlineData")]
        [InlineData("MemberData(\"\")")]
        [InlineData("ClassData(typeof(string))")]
        public async void DoesNotFindErrorForTheoryMethodWithDataAttributes_CSharp(string dataAttribute)
        {
            var source =
                "public class TestClass { [Xunit.Theory, Xunit." + dataAttribute + "] public void TestMethod() { } }";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("InlineData")]
        [InlineData("MemberData(\"\")")]
        [InlineData("ClassData(GetType(String))")]
        public async void DoesNotFindErrorForTheoryMethodWithDataAttributes_VisualBasic(string dataAttribute)
        {
            var source = $@"
Public Class TestClass
    <Xunit.Theory, Xunit.{dataAttribute}>
    Public Sub TestMethod()
    End Sub
End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void FindsErrorForTheoryMethodMissingData_CSharp()
        {
            var source = "class TestClass { [Xunit.Theory] public void TestMethod() { } }";

            var expected = VerifyCS.Diagnostic().WithSpan(1, 46, 1, 56);
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async void FindsErrorForTheoryMethodMissingData_VisualBasic()
        {
            var source = @"
Class TestClass
    <Xunit.Theory>
    Public Sub TestMethod()
    End Sub
End Class";

            var expected = VerifyVB.Diagnostic().WithSpan(4, 16, 4, 26);
            await VerifyVB.VerifyAnalyzerAsync(source, expected);
        }
    }
}
