using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute;

namespace Xunit.Analyzers
{
    public class TheoryMethodCannotHaveDefaultParameterTests
    {
        readonly XunitCapabilities capabilitiesSub = Substitute.For<XunitCapabilities>();
        readonly DiagnosticAnalyzer analyzer;

        public TheoryMethodCannotHaveDefaultParameterTests()
        {
            capabilitiesSub.TheorySupportsParameterArrays.Returns(true);
            analyzer = new TheoryMethodCannotHaveDefaultParameter(capabilitiesSub);
        }

        [Fact]
        public async Task FindsErrorForTheoryWithDefaultParameter_WhenDefaultValueNotSupported()
        {
            capabilitiesSub.TheorySupportsDefaultParameterValues.Returns(false);

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
            capabilitiesSub.TheorySupportsDefaultParameterValues.Returns(true);

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, string c = \"\") { }" +
                "}");

            Assert.Empty(diagnostics);
        }
    }
}
