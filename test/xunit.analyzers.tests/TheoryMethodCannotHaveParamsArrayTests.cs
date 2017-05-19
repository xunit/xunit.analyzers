using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute;

namespace Xunit.Analyzers
{
    public class TheoryMethodCannotHaveParamsArrayTests
    {
        readonly XunitCapabilities capabilitiesSub = Substitute.For<XunitCapabilities>();
        readonly DiagnosticAnalyzer analyzer;

        public TheoryMethodCannotHaveParamsArrayTests()
        {
            capabilitiesSub.TheorySupportsParameterArrays.Returns(true);
            analyzer = new TheoryMethodCannotHaveParamsArray(capabilitiesSub);
        }

        [Fact]
        public async Task FindsErrorForTheoryWithParamsArrayAsync_WhenParamsArrayNotSupported()
        {
            capabilitiesSub.TheorySupportsParameterArrays.Returns(false);

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
            capabilitiesSub.TheorySupportsParameterArrays.Returns(true);

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, params string[] c) { }" +
                "}");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DoesNotFindErrorForTheoryWithParamsArrayAsync(bool paramsSupported)
        {
            capabilitiesSub.TheorySupportsParameterArrays.Returns(paramsSupported);

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "class TestClass {" +
                "   [Xunit.Theory] public void TestMethod(int a, string b, string[] c) { }" +
                "}");

            Assert.Empty(diagnostics);
        }
    }
}
