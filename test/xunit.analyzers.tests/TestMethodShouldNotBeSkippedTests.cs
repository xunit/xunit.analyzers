using Microsoft.CodeAnalysis;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TestMethodShouldNotBeSkipped>;

namespace Xunit.Analyzers
{
    public class TestMethodShouldNotBeSkippedTests
    {
        [Theory]
        [InlineData("Fact")]
        [InlineData("Theory")]
        public async void DoesNotFindErrorForNotSkippedTest(string attribute)
        {
            var source = "public class TestClass { [Xunit." + attribute + "] public void TestMethod() { } }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("Fact")]
        [InlineData("Theory")]
        public async void FindsErrorForSkippedTests(string attribute)
        {
            var source = "class TestClass { [Xunit." + attribute + "(Skip=\"Lazy\")] public void TestMethod() { } }";

            var expected = Verify.Diagnostic().WithSpan(1, 27 + attribute.Length, 1, 38 + attribute.Length).WithSeverity(DiagnosticSeverity.Info);
            await Verify.VerifyAnalyzerAsync(source, expected);
        }
    }
}
