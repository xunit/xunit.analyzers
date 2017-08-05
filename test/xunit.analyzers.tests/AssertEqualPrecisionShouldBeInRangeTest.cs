using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertEqualPrecisionShouldBeInRangeTest
    {
        public class Analyzer
        {
            readonly DiagnosticAnalyzer analyzer = new AssertEqualPrecisionShouldBeInRange();

            [Theory]
            [InlineData(0)]
            [InlineData(1)]
            [InlineData(8)]
            [InlineData(14)]
            [InlineData(15)]
            public async void DoesNotFindError_ForDoubleArgumentWithPrecisionProvidedInRange(int precision)
            {
                var diagnostics = await AnalyzeTestMethod(
                    "double num = 0.133d;" +
                    $"Xunit.Assert.Equal(0.13d, num, {precision});");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData(int.MinValue)]
            [InlineData(-2000)]
            [InlineData(-1)]
            [InlineData(16)]
            [InlineData(17000)]
            [InlineData(int.MaxValue)]
            public async void FindsError_ForDoubleArgumentWithPrecisionProvidedOutOfRange(int precision)
            {
                var diagnostics = await AnalyzeTestMethod(
                    "double num = 0.133d;" +
                    $"Xunit.Assert.Equal(0.13d, num, {precision});");

                Assert.Collection(diagnostics, d =>
                {
                    Assert.Equal("Keep precision in range [0..15] when asserting equality of double typed actual value.", d.GetMessage());
                    Assert.Equal("xUnit2016", d.Id);
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
            }

            [Theory]
            [InlineData(0)]
            [InlineData(1)]
            [InlineData(14)]
            [InlineData(27)]
            [InlineData(28)]
            public async void DoesNotFindError_ForDecimalArgumentWithPrecisionProvidedInRange(int precision)
            {
                var diagnostics = await AnalyzeTestMethod(
                    "decimal num = 0.133m;" +
                    $"Xunit.Assert.Equal(0.13m, num, {precision});");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData(int.MinValue)]
            [InlineData(-2000)]
            [InlineData(-1)]
            [InlineData(29)]
            [InlineData(30000)]
            [InlineData(int.MaxValue)]
            public async void FindsError_ForDecimalArgumentWithPrecisionProvidedOutOfRange(int precision)
            {
                var diagnostics = await AnalyzeTestMethod(
                    "decimal num = 0.133m;" +
                    $"Xunit.Assert.Equal(0.13m, num, {precision});");

                Assert.Collection(diagnostics, d =>
                {
                    Assert.Equal("Keep precision in range [0..28] when asserting equality of decimal typed actual value.", d.GetMessage());
                    Assert.Equal("xUnit2016", d.Id);
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
            }

            private async Task<ImmutableArray<Diagnostic>> AnalyzeTestMethod(string methodBody)
            {
                return await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "class TestClass { void TestMethod() {" + methodBody + "}}");
            }
        }
    }
}