using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertEqualShouldNotBeUsedForNullCheckTests
    {
        readonly DiagnosticAnalyzer analyzer = new AssertEqualShouldNotBeUsedForNullCheck();

        public static TheoryData<string> Methods = new TheoryData<string> { "Equal", "NotEqual", "StrictEqual", "NotStrictEqual", "Same", "NotSame" };

        [Theory]
        [InlineData("Equal")]
        [InlineData("NotEqual")]
        public async void FindsWarning_ForFirstNullLiteral_StringOverload(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    string val = null;
    Xunit.Assert." + method + @"(null, val);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}() to check for null value.", d.GetMessage());
                Assert.Equal("xUnit2003", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [InlineData("Equal")]
        [InlineData("NotEqual")]
        public async void FindsWarning_ForFirstNullLiteral_StringOverload_WithCustomComparer(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    string val = null;
    Xunit.Assert." + method + @"(null, val, System.StringComparer.Ordinal);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}() to check for null value.", d.GetMessage());
                Assert.Equal("xUnit2003", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForFirstNullLiteral_ObjectOverload(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    object val = null;
    Xunit.Assert." + method + @"(null, val);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}() to check for null value.", d.GetMessage());
                Assert.Equal("xUnit2003", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [InlineData("Equal")]
        [InlineData("NotEqual")]
        public async void FindsWarning_ForFirstNullLiteral_ObjectOverload_WithCustomComparer(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    object val = null;
    Xunit.Assert." + method + @"(null, val, System.Collections.Generic.EqualityComparer<object>.Default);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}() to check for null value.", d.GetMessage());
                Assert.Equal("xUnit2003", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [InlineData("Equal")]
        [InlineData("NotEqual")]
        [InlineData("StrictEqual")]
        [InlineData("NotStrictEqual")]
        public async void FindsWarning_ForFirstNullLiteral_GenericOverload(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    TestClass val = null;
    Xunit.Assert." + method + @"<TestClass>(null, val);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}() to check for null value.", d.GetMessage());
                Assert.Equal("xUnit2003", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [InlineData("Equal")]
        [InlineData("NotEqual")]
        public async void FindsWarning_ForFirstNullLiteral_GenericOverload_WithCustomComparer(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    TestClass val = null;
    Xunit.Assert." + method + @"<TestClass>(null, val, System.Collections.Generic.EqualityComparer<TestClass>.Default);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}() to check for null value.", d.GetMessage());
                Assert.Equal("xUnit2003", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void DoesNotFindWarning_ForOtherLiteral(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    int val = 1;
    Xunit.Assert." + method + @"(1, val);
} }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void DoesNotFindWarning_ForSecondNullLiteral(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    string val = null;
    Xunit.Assert." + method + @"(val, null);
} }");

            Assert.Empty(diagnostics);
        }
    }
}
