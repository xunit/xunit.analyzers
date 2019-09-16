using VerifyCS = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TestMethodMustNotHaveMultipleFactAttributes>;
using VerifyVB = Xunit.Analyzers.VisualBasicVerifier<Xunit.Analyzers.TestMethodMustNotHaveMultipleFactAttributes>;

namespace Xunit.Analyzers
{
    public class TestMethodMustNotHaveMultipleFactAttributesTests
    {
        [Theory]
        [InlineData("Fact")]
        [InlineData("Theory")]
        public async void DoesNotFindErrorForMethodWithSingleAttribute_CSharp(string attribute)
        {
            var source =
                "public class TestClass { [Xunit." + attribute + "] public void TestMethod() { } }";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("Fact")]
        [InlineData("Theory")]
        public async void DoesNotFindErrorForMethodWithSingleAttribute_VisualBasic(string attribute)
        {
            var source = $@"
Public Class TestClass
    <Xunit.{attribute}>
    Public Sub TestMethod()
    End Sub
End Class";

            await VerifyVB.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void FindsErrorForMethodWithTheoryAndFact_CSharp()
        {
            var source =
                "public class TestClass { [Xunit.Fact, Xunit.Theory] public void TestMethod() { } }";

            var expected = VerifyCS.Diagnostic().WithSpan(1, 65, 1, 75);
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async void FindsErrorForMethodWithTheoryAndFact_VisualBasic()
        {
            var source = @"
Public Class TestClass
    <Xunit.Fact, Xunit.Theory>
    Public Sub TestMethod()
    End Sub
End Class";

            var expected = VerifyVB.Diagnostic().WithSpan(4, 16, 4, 26);
            await VerifyVB.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async void FindsErrorForMethodWithCustomFactAttribute_CSharp()
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        "public class TestClass { [Xunit.Fact, CustomFact] public void TestMethod() { } }",
                        "public class CustomFactAttribute : Xunit.FactAttribute { }",
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyCS.Diagnostic().WithSpan(1, 63, 1, 73),
                    },
                },
            }.RunAsync();
        }

        [Fact]
        public async void FindsErrorForMethodWithCustomFactAttribute_VisualBasic()
        {
            await new VerifyVB.Test
            {
                TestState =
                {
                    Sources =
                    {
                        @"
Public Class TestClass
    <Xunit.Fact, CustomFact>
    Public Sub TestMethod()
    End Sub
End Class",
                        @"
Public Class CustomFactAttribute
    Inherits Xunit.FactAttribute
End Class",
                    },
                    ExpectedDiagnostics =
                    {
                        VerifyVB.Diagnostic().WithSpan(4, 16, 4, 26),
                    },
                },
            }.RunAsync();
        }
    }
}
