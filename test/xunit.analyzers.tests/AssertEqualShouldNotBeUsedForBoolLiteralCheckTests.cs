using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertEqualShouldNotBeUsedForBoolLiteralCheckTests
    {
        readonly DiagnosticAnalyzer analyzer = new AssertEqualShouldNotBeUsedForBoolLiteralCheck();

        public static TheoryData<string> Methods = new TheoryData<string> { "Equal", "NotEqual", "StrictEqual", "NotStrictEqual", };

        private static void AssertHasDiagnostic(IEnumerable<Diagnostic> diagnostics, string method)
        {
            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}() to check for boolean conditions.", d.GetMessage());
                Assert.Equal("xUnit2004", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForFirstBoolLiteral(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    bool val = true;
    Xunit.Assert." + method + @"(true, val);
} }");

            AssertHasDiagnostic(diagnostics, method);
        }

        [Theory]
        [InlineData("Equal")]
        [InlineData("NotEqual")]
        public async void FindsWarning_ForFirstBoolLiteral_WithCustomComparer(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    bool val = false;
    Xunit.Assert." + method + @"(false, val, System.Collections.Generic.EqualityComparer<bool>.Default);
} }");

            AssertHasDiagnostic(diagnostics, method);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForFirstBoolLiteral_ObjectOverload(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    object val = false;
    Xunit.Assert." + method + @"(true, val);
} }");

            AssertHasDiagnostic(diagnostics, method);
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
        public async void DoesNotFindWarning_ForSecondBoolLiteral(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    bool val = false;
    Xunit.Assert." + method + @"(val, true);
} }");

            Assert.Empty(diagnostics);
        }
    }
}
