using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class PublicMethodShouldBeMarkedAsTestTests
    {
        public class Analyzer
        {
            readonly DiagnosticAnalyzer analyzer = new PublicMethodShouldBeMarkedAsTest();

            [Fact]
            public async void DoesNotFindErrorForPublicMethodInNonTestClass()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { public void TestMethod() { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("Xunit.Fact")]
            [InlineData("Xunit.Theory")]
            public async void DoesNotFindErrorForTestMethods(string attribute)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { [" + attribute + "] public void TestMethod() { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindErrorForIDisposableDisposeMethod()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, 
@"public class TestClass : System.IDisposable {
    [Xunit.Fact] public void TestMethod() { }
    public void Dispose() { }
}");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("Xunit.Fact")]
            [InlineData("Xunit.Theory")]

            public async void FindsWarningForPublicMethodWithoutParametersInTestClass(string attribute)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [" + attribute + "] public void TestMethod() { } public void Method() {} }");

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("Public method 'Method' on test class 'TestClass' should be marked as a Fact.", d.GetMessage());
                        Assert.Equal("xUnit1013", d.Id);
                        Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                    });
            }

            [Theory]
            [InlineData("Xunit.Fact")]
            [InlineData("Xunit.Theory")]

            public async void FindsWarningForPublicMethodWithParametersInTestClass(string attribute)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [" + attribute + "] public void TestMethod() { } public void Method(int a) {} }");

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("Public method 'Method' on test class 'TestClass' should be marked as a Theory.", d.GetMessage());
                        Assert.Equal("xUnit1013", d.Id);
                        Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                    });
            }
        }
    }
}
