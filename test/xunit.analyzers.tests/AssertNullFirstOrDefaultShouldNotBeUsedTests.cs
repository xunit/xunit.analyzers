using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertNullFirstOrDefaultShouldNotBeUsedTests
    {
        private readonly DiagnosticAnalyzer analyzer = new AssertNullFirstOrDefaultShouldNotBeUsed();

        private static string Template(string expression) => $@"using System.Linq;
using System.Collections.Generic;
using Xunit;

class TestClass
{{
    void TestMethod()
    {{
        var collection = new List<string>();
        {expression};
    }}
}}";

        [Fact]
        public async void FindsWarningForFirstOrDefaultInsideAssertNullWithoutArguments()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, Template("Assert.Null(collection.FirstOrDefault())"));

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use FirstOrDefault within Assert.Null or Assert.NotNull. Use Empty/Contains instead.", d.GetMessage());
                Assert.Equal("xUnit2020", d.Id);
                Assert.Equal(DiagnosticSeverity.Info, d.Severity);
            });
        }

        [Fact]
        public async void FindsWarningForFirstOrDefaultInsideAssertNullWithArguments()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, Template("Assert.Null(collection.FirstOrDefault(x => x == \"test\"))"));

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use FirstOrDefault within Assert.Null or Assert.NotNull. Use Empty/Contains instead.", d.GetMessage());
                Assert.Equal("xUnit2020", d.Id);
                Assert.Equal(DiagnosticSeverity.Info, d.Severity);
            });
        }

        [Fact]
        public async void FindsWarningForFirstOrDefaultInsideAssertNotNullWithoutArguments()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, Template("Assert.NotNull(collection.FirstOrDefault())"));

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use FirstOrDefault within Assert.Null or Assert.NotNull. Use Empty/Contains instead.", d.GetMessage());
                Assert.Equal("xUnit2020", d.Id);
                Assert.Equal(DiagnosticSeverity.Info, d.Severity);
            });
        }

        [Fact]
        public async void FindsWarningForFirstOrDefaultInsideAssertNotNullWithArguments()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, Template("Assert.NotNull(collection.FirstOrDefault(x => x == \"test\"))"));

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use FirstOrDefault within Assert.Null or Assert.NotNull. Use Empty/Contains instead.", d.GetMessage());
                Assert.Equal("xUnit2020", d.Id);
                Assert.Equal(DiagnosticSeverity.Info, d.Severity);
            });
        }
    }
}
