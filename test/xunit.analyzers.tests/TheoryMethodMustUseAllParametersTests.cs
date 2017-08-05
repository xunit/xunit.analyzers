using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TheoryMethodMustUseAllParametersTests
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new TheoryMethodMustUseAllParameters();

        private static void CheckDiagnostics(IEnumerable<Diagnostic> diagnostics, params (string method, string type, string parameter)[] messageArgs)
        {
            var diagnosticArray = diagnostics.ToArray();
            Assert.Equal(messageArgs.Length, diagnosticArray.Length);

            for (int i = 0; i < messageArgs.Length; i++)
            {
                var (method, type, parameter) = messageArgs[i];
                string message = $"Theory method '{method}' on test class '{type}' does not use parameter '{parameter}'.";

                var diagnostic = diagnosticArray[i];
                Assert.Equal(message, diagnostic.GetMessage());
                Assert.Equal("xUnit1026", diagnostic.Id);
                Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            }
        }

        [Fact]
        public async void FindsError_ParameterNotReferenced()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using Xunit;

class TestClass
{
    [Theory]
    void TestMethod(int unused) { }
}");

            CheckDiagnostics(diagnostics,
                (method: "TestMethod", type: "TestClass", parameter: "unused"));
        }

        [Fact]
        public async void FindsError_ParameterUnread()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using System;
using Xunit;

class TestClass
{
    [Theory]
    void TestMethod(int unused)
    {
        unused = 3;
        int.TryParse(""123"", out unused);
    }
}");

            CheckDiagnostics(diagnostics,
                (method: "TestMethod", type: "TestClass", parameter: "unused"));
        }

        [Fact]
        public async void FindsError_MultipleUnreadParameters()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using Xunit;

class TestClass
{
    [Theory]
    void TestMethod(int foo, int bar, int baz) { }
}");

            CheckDiagnostics(diagnostics,
                (method: "TestMethod", type: "TestClass", parameter: "foo"),
                (method: "TestMethod", type: "TestClass", parameter: "bar"),
                (method: "TestMethod", type: "TestClass", parameter: "baz"));
        }

        [Fact]
        public async void FindsError_SomeUnreadParameters()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using System;
using Xunit;

class TestClass
{
    [Theory]
    void TestMethod(int foo, int bar, int baz)
    {
        TestMethod(bar, bar, bar);
        baz = 3;
    }
}");

            CheckDiagnostics(diagnostics,
                (method: "TestMethod", type: "TestClass", parameter: "foo"),
                (method: "TestMethod", type: "TestClass", parameter: "baz"));
        }

        [Fact]
        public async void DoesNotFindError_ParameterRead()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using System;
using Xunit;

class TestClass
{
    [Theory]
    void TestMethod(int unused)
    {
        TestMethod(unused);
    }
}");
        }
    }
}
