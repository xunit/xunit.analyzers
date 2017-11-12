using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertSameShouldNotBeCalledOnValueTypesTests
    {
        readonly DiagnosticAnalyzer analyzer = new AssertSameShouldNotBeCalledOnValueTypes();

        [Theory]
        [InlineData("Same")]
        [InlineData("NotSame")]
        public async void FindsWarningForTwoValueParameters(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, CompilationReporting.IgnoreErrors,
@"class TestClass { void TestMethod() {
    int a = 0;
    Xunit.Assert." + method + @"(0, a);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}() on value type 'int'.", d.GetMessage());
                Assert.Equal("xUnit2005", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [InlineData("Same")]
        [InlineData("NotSame")]
        public async void FindsWarningForFirstValueParameters(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, CompilationReporting.IgnoreErrors,
@"class TestClass { void TestMethod() {
    object a = 0;
    Xunit.Assert." + method + @"(0, a);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}() on value type 'int'.", d.GetMessage());
                Assert.Equal("xUnit2005", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [InlineData("Same")]
        [InlineData("NotSame")]
        public async void FindsWarningForSecondValueParameters(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, CompilationReporting.IgnoreErrors,
@"class TestClass { void TestMethod() {
    object a = 0;
    Xunit.Assert." + method + @"(a, 0);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}() on value type 'int'.", d.GetMessage());
                Assert.Equal("xUnit2005", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }
    }
}
