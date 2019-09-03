using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    public class TheoryMethodCannotHaveParamsArrayTests
    {
        [Fact]
        public async Task FindsErrorForTheoryWithParamsArrayAsync_WhenParamsArrayNotSupported()
        {
            // 2.1.0 does not support params arrays
            var analyzer = new TheoryMethodCannotHaveParamsArray("2.1.0");

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, params string[] c) { }" +
                "}");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Theory method 'TestMethod' on test class 'TestClass' cannot have a parameter array 'c'.", d.GetMessage());
                    Assert.Equal("xUnit1022", d.Descriptor.Id);
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
        }

        [Fact]
        public async Task DoesNotFindErrorForTheoryWithParamsArrayAsync_WhenParamsArraySupported()
        {
            // 2.2.0 does support params arrays
            var analyzer = new TheoryMethodCannotHaveParamsArray("2.2.0");

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, params string[] c) { }" +
                "}");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("2.1.0")]
        [InlineData("2.2.0")]
        public async Task DoesNotFindErrorForTheoryWithNonParamsArrayAsync(string versionString)
        {
            var analyzer = new TheoryMethodCannotHaveParamsArray(versionString);

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, string[] c) { }" +
                "}");

            Assert.Empty(diagnostics);
        }
    }
}
