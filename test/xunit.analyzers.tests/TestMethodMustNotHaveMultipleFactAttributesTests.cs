using VerifyCS = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TestMethodMustNotHaveMultipleFactAttributes>;

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

        [Fact]
        public async void FindsErrorForMethodWithTheoryAndFact_CSharp()
        {
            var source =
                "public class TestClass { [Xunit.Fact, Xunit.Theory] public void TestMethod() { } }";

            var expected = VerifyCS.Diagnostic().WithSpan(1, 65, 1, 75);
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
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
    }
}
