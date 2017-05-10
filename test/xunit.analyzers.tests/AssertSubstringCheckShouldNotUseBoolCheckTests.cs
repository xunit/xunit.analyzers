using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertSubstringCheckShouldNotUseBoolCheckTests
    {
        public class Analyzer
        {
            readonly DiagnosticAnalyzer analyzer = new AssertSubstringCheckShouldNotUseBoolCheck();
            
            public static TheoryData<string> BooleanMethods = new TheoryData<string> { "True", "False" };

            private static void AssertHasDiagnostic(IEnumerable<Diagnostic> diagnostics, string method)
            {
                Assert.Collection(diagnostics, d =>
                {
                    Assert.Equal($"Do not use Assert.{method}() to check for substrings.", d.GetMessage());
                    Assert.Equal("xUnit2009", d.Id);
                    Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                });
            }

            [Theory]
            [MemberData(nameof(BooleanMethods))]
            public async void FindsWarning_ForBooleanContainsCheck(string method)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".Contains(""a""));
} }");

                AssertHasDiagnostic(diagnostics, method);
            }

            [Theory]
            [MemberData(nameof(BooleanMethods))]
            public async void DoesNotFindWarning_ForBooleanContainsCheck_WithUserMessage(string method)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".Contains(""a""), ""message"");
} }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void FindsWarning_ForBooleanTrueStartsWithCheck()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.True(""abc"".StartsWith(""a""));
} }");

                AssertHasDiagnostic(diagnostics, "True");
            }

            [Fact]
            public async void FindsWarning_ForBooleanTrueStartsWithCheck_WithStringComparison()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.True(""abc"".StartsWith(""a"", System.StringComparison.CurrentCulture));
} }");

                AssertHasDiagnostic(diagnostics, "True");
            }

            [Fact]
            public async void DoesNotFindWarning_ForBooleanFalseStartsWithCheck()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.False(""abc"".StartsWith(""a""));
} }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindWarning_ForBooleanFalseStartsWithCheck_WithStringComparison()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.False(""abc"".StartsWith(""a"", System.StringComparison.CurrentCulture));
} }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [MemberData(nameof(BooleanMethods))]
            public async void DoesNotFindWarning_ForBooleanStartsWithCheck_WithUserMessage(string method)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".StartsWith(""a""), ""message"");
} }");
                
                Assert.Empty(diagnostics);
            }

            [Theory]
            [MemberData(nameof(BooleanMethods))]
            public async void DoesNotFindWarning_ForBooleanStartsWithCheck_WithStringComparison_AndUserMessage(string method)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".StartsWith(""a"", System.StringComparison.CurrentCulture), ""message"");
} }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [MemberData(nameof(BooleanMethods))]
            public async void DoesNotFindWarning_ForBooleanStartsWithCheck_WithBoolAndCulture(string method)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".StartsWith(""a"", true, System.Globalization.CultureInfo.CurrentCulture));
} }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [MemberData(nameof(BooleanMethods))]
            public async void DoesNotFindWarning_ForBooleanStartsWithCheck_WithBoolAndCulture_AndUserMessage(string method)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".StartsWith(""a"", true, System.Globalization.CultureInfo.CurrentCulture), ""message"");
} }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void FindsWarning_ForBooleanTrueEndsWithCheck()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.True(""abc"".EndsWith(""a""));
} }");

                AssertHasDiagnostic(diagnostics, "True");
            }

            [Fact]
            public async void FindsWarning_ForBooleanTrueEndsWithCheck_WithStringComparison()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.True(""abc"".EndsWith(""a"", System.StringComparison.CurrentCulture));
} }");

                AssertHasDiagnostic(diagnostics, "True");
            }

            [Fact]
            public async void DoesNotFindWarning_ForBooleanFalseEndsWithCheck()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.False(""abc"".EndsWith(""a""));
} }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindWarning_ForBooleanFalseEndsWithCheck_WithStringComparison()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.False(""abc"".EndsWith(""a"", System.StringComparison.CurrentCulture));
} }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [MemberData(nameof(BooleanMethods))]
            public async void DoesNotFindWarning_ForBooleanEndsWithCheck_WithUserMessage(string method)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".EndsWith(""a""), ""message"");
} }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [MemberData(nameof(BooleanMethods))]
            public async void DoesNotFindWarning_ForBooleanEndsWithCheck_WithStringComparison_AndUserMessage(string method)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".EndsWith(""a"", System.StringComparison.CurrentCulture), ""message"");
} }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [MemberData(nameof(BooleanMethods))]
            public async void DoesNotFindWarning_ForBooleanEndsWithCheck_WithBoolAndCulture(string method)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".EndsWith(""a"", true, System.Globalization.CultureInfo.CurrentCulture));
} }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [MemberData(nameof(BooleanMethods))]
            public async void DoesNotFindWarning_ForBooleanEndsWithCheck_WithBoolAndCulture_AndUserMessage(string method)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".EndsWith(""a"", true, System.Globalization.CultureInfo.CurrentCulture), ""message"");
} }");

                Assert.Empty(diagnostics);
            }
        }
    }
}