using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TheoryMethodShouldUseAllParametersTests
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new TheoryMethodShouldUseAllParameters();

        private static void CheckDiagnostics(IEnumerable<Diagnostic> diagnostics, params (string method, string type, string parameter)[] messageArgs)
        {
            var diagnosticArray = diagnostics.ToArray();
            Assert.Equal(messageArgs.Length, diagnosticArray.Length);

            for (var i = 0; i < messageArgs.Length; i++)
            {
                var (method, type, parameter) = messageArgs[i];
                var message = $"Theory method '{method}' on test class '{type}' does not use parameter '{parameter}'.";

                var diagnostic = diagnosticArray[i];
                Assert.Equal(message, diagnostic.GetMessage());
                Assert.Equal("xUnit1026", diagnostic.Id);
                Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
            }
        }

        [Fact]
        public async void FindsWarning_ParameterNotReferenced()
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
        public async void FindsWarning_ParameterUnread()
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
        public async void FindsWarning_MultipleUnreadParameters()
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
        public async void FindsWarning_SomeUnreadParameters()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using System;
using Xunit;

class TestClass
{
    [Theory]
    void TestMethod(int foo, int bar, int baz)
    {
        Console.WriteLine(bar);
        baz = 3;
    }
}");

            CheckDiagnostics(diagnostics,
                (method: "TestMethod", type: "TestClass", parameter: "foo"),
                (method: "TestMethod", type: "TestClass", parameter: "baz"));
        }

        [Fact]
        public async void FindsWarning_ExpressionBodiedMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using Xunit;

class TestClass
{
    [Theory]
    void TestMethod(int unused) => Assert.Equal(5, 2 + 2);
}");

            CheckDiagnostics(diagnostics,
                (method: "TestMethod", type: "TestClass", parameter: "unused"));
        }

        [Fact]
        public async void DoesNotFindWarning_ParameterRead()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using System;
using Xunit;

class TestClass
{
    [Theory]
    void TestMethod(int used)
    {
        Console.WriteLine(used);
    }
}");

            CheckDiagnostics(diagnostics);
        }

        [Fact]
        public async void DoesNotFindWarning_ExpressionBodiedMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using Xunit;

class TestClass
{
    [Theory]
    void TestMethod(int used) => Assert.Equal(used, 2 + 2);
}");

            CheckDiagnostics(diagnostics);
        }

        [Fact]
        public async void DoesNotCrash_MethodWithoutBody()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using Xunit;

class TestClass
{
    [Theory]
    extern void TestMethod(int foo);
}");

            CheckDiagnostics(diagnostics);
        }

        [Fact]
        public async void DoesNotFindWarning_ParameterStartingWithUnderscore()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
using Xunit;

class TestClass
{
    [Theory]
    void TestMethod(string _, int _discarded) { }
}");
            CheckDiagnostics(diagnostics);
        }
    }
}
