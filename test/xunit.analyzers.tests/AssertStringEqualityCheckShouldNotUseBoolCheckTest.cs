using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertStringEqualityCheckShouldNotUseBoolCheckTest
    {
        public class Analyzer
        {
            readonly DiagnosticAnalyzer analyzer = new AssertStringEqualityCheckShouldNotUseBoolCheck();

            public static TheoryData<string> AssertMethods = new TheoryData<string> { "True", "False" };

            public static TheoryData<StringComparison> SupportedStringComparisons = new TheoryData<StringComparison>
            {
                StringComparison.Ordinal,
                StringComparison.OrdinalIgnoreCase
            };

            public static TheoryData<StringComparison> UnsupportedStringComparisons = new TheoryData<StringComparison>
            {
                StringComparison.CurrentCulture,
                StringComparison.CurrentCultureIgnoreCase,
                StringComparison.InvariantCulture,
                StringComparison.InvariantCultureIgnoreCase
            };

            public static TheoryData<StringComparison> AllStringComparisons = new TheoryData<StringComparison>
            {
                StringComparison.Ordinal,
                StringComparison.OrdinalIgnoreCase,
                StringComparison.CurrentCulture,
                StringComparison.CurrentCultureIgnoreCase,
                StringComparison.InvariantCulture,
                StringComparison.InvariantCultureIgnoreCase
            };

            private static void AssertHasDiagnostic(IEnumerable<Diagnostic> diagnostics, string method)
            {
                Assert.Collection(diagnostics, d =>
                {
                    Assert.Equal($"Do not use Assert.{method}() to check for string equality.", d.GetMessage(), ignoreCase: true);
                    Assert.Equal("xUnit2010", d.Id);
                    Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                });
            }

            [Theory]
            [MemberData(nameof(AssertMethods))]
            public async void FindsWarning_ForInstanceEqualsCheck(string method)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".Equals(""a""));
} }");

                AssertHasDiagnostic(diagnostics, method);
            }

            [Theory]
            [MemberData(nameof(SupportedStringComparisons))]
            public async void FindsWarning_ForTrueInstanceEqualsCheck_WithSupportedStringComparison(StringComparison comparison)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.True(""abc"".Equals(""a"", System.StringComparison." + comparison + @"));
} }");

                AssertHasDiagnostic(diagnostics, "True");
            }

            [Theory]
            [MemberData(nameof(UnsupportedStringComparisons))]
            public async void DoesNotFindWarning_ForTrueInstanceEqualsCheck_WithUnsupportedStringComparison(StringComparison comparison)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.True(""abc"".Equals(""a"", System.StringComparison." + comparison + @"));
} }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [MemberData(nameof(AllStringComparisons))]
            public async void DoesNotFindWarning_ForFalseInstanceEqualsCheck_WithStringComparison(StringComparison comparison)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.False(""abc"".Equals(""a"", System.StringComparison." + comparison + @"));
} }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [MemberData(nameof(AssertMethods))]
            public async void FindsWarning_ForStaticEqualsCheck(string method)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(System.String.Equals(""abc"", ""a""));
} }");

                AssertHasDiagnostic(diagnostics, method);
            }

            [Theory]
            [MemberData(nameof(SupportedStringComparisons))]
            public async void FindsWarning_ForTrueStaticEqualsCheck_WithSupportedStringComparison(StringComparison comparison)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.True(System.String.Equals(""abc"", ""a"", System.StringComparison." + comparison + @"));
} }");

                AssertHasDiagnostic(diagnostics, "True");
            }

            [Theory]
            [MemberData(nameof(UnsupportedStringComparisons))]
            public async void DoesNotFindWarning_ForTrueStaticEqualsCheck_WithUnsupportedStringComparison(StringComparison comparison)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.True(System.String.Equals(""abc"", ""a"", System.StringComparison." + comparison + @"));
} }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [MemberData(nameof(AllStringComparisons))]
            public async void DoesNotFindWarning_ForFalseStaticEqualsCheck_WithStringComparison(StringComparison comparison)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() {
    Xunit.Assert.False(System.String.Equals(""abc"", ""a"", System.StringComparison." + comparison + @"));
} }");

                Assert.Empty(diagnostics);
            }
        }
    }
}