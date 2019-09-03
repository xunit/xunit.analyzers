using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    public class TheoryMethodCannotHaveDefaultParameterTests
    {
        [Fact]
        public async Task FindsErrorForTheoryWithDefaultParameter_WhenDefaultValueNotSupported()
        {
            // 2.1.0 does not support default parameter values
            var analyzer = new TheoryMethodCannotHaveDefaultParameter("2.1.0");

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, string c = \"\") { }" +
                "}");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Theory method 'TestMethod' on test class 'TestClass' parameter 'c' cannot have a default value.", d.GetMessage());
                    Assert.Equal("xUnit1023", d.Descriptor.Id);
                    Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                });
        }

        [Fact]
        public async Task DoesNotFindErrorForTheoryWithDefaultParameter_WhenDefaultValueSupported()
        {
            // 2.2.0 does support default parameter values
            var analyzer = new TheoryMethodCannotHaveDefaultParameter("2.2.0");

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, string c = \"\") { }" +
                "}");

            Assert.Empty(diagnostics);
        }
    }
}
