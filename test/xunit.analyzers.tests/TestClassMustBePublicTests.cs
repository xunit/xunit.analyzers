using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TestClassMustBePublicTests
    {
        readonly DiagnosticAnalyzer analyzer = new TestClassMustBePublic();

        [Fact]
        public async void DoesNotFindErrorForPublicClass()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { [Xunit.Fact] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("Xunit.Fact")]
        [InlineData("Xunit.Theory")]
        public async void FindsErrorForPrivateClass(string attribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "class TestClass { [" + attribute + "] public void TestMethod() { } }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Test classes must be public", d.GetMessage());
                    Assert.Equal("xUnit1000", d.Descriptor.Id);
                });
        }
    }
}
