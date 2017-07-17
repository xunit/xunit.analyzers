using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TestMethodShouldNotBeSkippedTests
    {
        readonly DiagnosticAnalyzer analyzer = new TestMethodShouldNotBeSkipped();

        [Theory]
        [InlineData("Fact")]
        [InlineData("Theory")]
        public async void DoesNotFindErrorForNotSkippedTest(string attribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { [Xunit." + attribute + "] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("Fact")]
        [InlineData("Theory")]
        public async void FindsErrorForSkippedTests(string attribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "class TestClass { [Xunit." + attribute + "(Skip=\"Lazy\")] public void TestMethod() { } }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Test methods should not be skipped", d.GetMessage());
                    Assert.Equal("xUnit1004", d.Descriptor.Id);
                    Assert.Equal(DiagnosticSeverity.Info, d.Severity);
                });
        }
    }
}
