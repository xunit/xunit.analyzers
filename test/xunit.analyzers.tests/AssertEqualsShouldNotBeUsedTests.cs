using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertEqualsShouldNotBeUsedTests
    {
        readonly DiagnosticAnalyzer analyzer = new AssertEqualsShouldNotBeUsed();

        [Theory]
        [InlineData("Equals")]
        [InlineData("ReferenceEquals")]
        public async void FindsHiddenDiagnosticWhenProhibitedMethodIsUsed(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, CompilationReporting.IgnoreErrors,
@"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(null, null);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}().", d.GetMessage());
                Assert.Equal("xUnit2001", d.Id);
                Assert.Equal(DiagnosticSeverity.Hidden, d.Severity);
            });
        }
    }
}
