namespace Xunit.Analyzers
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;

    // 2.1.0 does not support default parameter values
    using Verify_2_1 = CSharpVerifier<TheoryMethodCannotHaveDefaultParameterTests.Analyzer_2_1_0>;

    // 2.2.0 does support default parameter values
    using Verify_2_2 = CSharpVerifier<TheoryMethodCannotHaveDefaultParameterTests.Analyzer_2_2_0>;

    public class TheoryMethodCannotHaveDefaultParameterTests
    {
        [Fact]
        public async Task FindsErrorForTheoryWithDefaultParameter_WhenDefaultValueNotSupported()
        {
            var source =
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, string c = \"\") { }" +
                "}";

            var expected = Verify_2_1.Diagnostic().WithSpan(1, 85, 1, 89).WithSeverity(DiagnosticSeverity.Error).WithArguments("TestMethod", "TestClass", "c");
            await Verify_2_1.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task DoesNotFindErrorForTheoryWithDefaultParameter_WhenDefaultValueSupported()
        {
            var source =
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, string c = \"\") { }" +
                "}";

            await Verify_2_2.VerifyAnalyzerAsync(source);
        }

        internal class Analyzer_2_1_0
            : TheoryMethodCannotHaveDefaultParameter
        {
            public Analyzer_2_1_0()
                : base("2.1.0")
            {
            }
        }

        internal class Analyzer_2_2_0
            : TheoryMethodCannotHaveDefaultParameter
        {
            public Analyzer_2_2_0()
                : base("2.2.0")
            {
            }
        }
    }
}
