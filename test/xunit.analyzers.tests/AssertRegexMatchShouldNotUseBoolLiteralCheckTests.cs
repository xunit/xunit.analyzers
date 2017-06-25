using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertRegexMatchShouldNotUseBoolLiteralCheckTests
    {
        readonly DiagnosticAnalyzer analyzer = new AssertRegexMatchShouldNotUseBoolLiteralCheck();

        public static TheoryData<string> Methods = new TheoryData<string> { "True", "False" };

        private static void AssertHasDiagnostic(IEnumerable<Diagnostic> diagnostics, string method)
        {
            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}() to match on regular expressions.", d.GetMessage());
                Assert.Equal("xUnit2008", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForStaticRegexIsMatch(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(System.Text.RegularExpressions.Regex.IsMatch(""abc"", ""\\w*""));
} }");

            AssertHasDiagnostic(diagnostics, method);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForInstaceRegexIsMatchWithInlineConstructedRegex(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(new System.Text.RegularExpressions.Regex(""abc"").IsMatch(""\\w*""));
} }");

            AssertHasDiagnostic(diagnostics, method);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForInstaceRegexIsMatchWithConstructedRegexVariable(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() {
    var regex = new System.Text.RegularExpressions.Regex(""abc"");
    Xunit.Assert." + method + @"(regex.IsMatch(""\\w*""));
} }");

            AssertHasDiagnostic(diagnostics, method);
        }
    }
}
