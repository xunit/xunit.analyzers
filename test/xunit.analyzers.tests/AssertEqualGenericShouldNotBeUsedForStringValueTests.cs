using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertEqualGenericShouldNotBeUsedForStringValueTests
    {
        public class Analyzer
        {
            readonly DiagnosticAnalyzer analyzer = new AssertEqualGenericShouldNotBeUsedForStringValue();

            public static TheoryData<string, string> Data { get; } = new TheoryData<string, string>
            {
                {"true.ToString()", "\"True\""},
                {"1.ToString()", "\"1\""},
                {"\"\"", "null"},
                {"null", "\"\""},
                {"\"\"", "\"\""},
                {"\"abc\"", "\"abc\""},
                {"\"TestMethod\"", "nameof(TestMethod)"}
            };

            [Theory]
            [MemberData(nameof(Data))]
            public async void DoesNotFindWarningForStringEqualityCheckWithoutGenericType(string expected, string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() { 
    Xunit.Assert.Equal(" + expected + @", " + value + @");
} }");
                Assert.Empty(diagnostics);
            }

            [Theory]
            [MemberData(nameof(Data))]
            public async void FindsWarningForStringEqualityCheckWithGenericType(string expected, string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() { 
    Xunit.Assert.Equal<string>(" + expected + @", " + value + @");
} }");

                Assert.Collection(diagnostics, d =>
                {
                    Assert.Equal("Do not use generic Assert.Equal overload to test for string equality.", d.GetMessage());
                    Assert.Equal("xUnit2006", d.Id);
                    Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                });
            }

            [Theory]
            [MemberData(nameof(Data))]
            public async void FindsWarningForStrictStringEqualityCheck(string expected, string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() { 
    Xunit.Assert.StrictEqual(" + expected + @", " + value + @");
} }");

                Assert.Collection(diagnostics, d =>
                {
                    Assert.Equal("Do not use Assert.StrictEqual to test for string equality.", d.GetMessage());
                    Assert.Equal("xUnit2006", d.Id);
                    Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                });
            }

            [Theory]
            [MemberData(nameof(Data))]
            public async void FindsWarningForStrictStringEqualityCheckWithGenericType(string expected, string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() { 
    Xunit.Assert.StrictEqual<string>(" + expected + @", " + value + @");
} }");

                Assert.Collection(diagnostics, d =>
                {
                    Assert.Equal("Do not use Assert.StrictEqual to test for string equality.", d.GetMessage());
                    Assert.Equal("xUnit2006", d.Id);
                    Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                });
            }
        }
    }
}