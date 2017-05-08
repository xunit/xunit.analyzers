using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertEqualLiteralValueShouldBeFirstTests
    {
        public class Analyzer
        {
            readonly DiagnosticAnalyzer analyzer = new AssertEqualLiteralValueShouldBeFirst();

            [Fact]
            public async void DoesNotFindWarningWhenConstantOrLiteralUsedForBothArguments()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    Xunit.Assert.Equal(""TestMethod"", nameof(TestMethod));
} }");

                Assert.Empty(diagnostics);
            }

            public static TheoryData<string, string> TypesAndValues { get; } = new TheoryData<string, string>
            {
                {"int", "0"},
                {"int", "0.0"},
                {"int", "sizeof(int)"},
                {"int", "default(int)"},
                {"string", "null"},
                {"string", "\"\""},
                {"string", "nameof(TestMethod)"},
                {"System.Type", "typeof(string)"},
                {"System.AttributeTargets", "System.AttributeTargets.Constructor"},
            };

            [Theory]
            [MemberData(nameof(TypesAndValues))]
            public async void DoesNotFindWarningForExpectedConstantOrLiteralValueAsFirstArgument(string type, string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() { 
    var v = default(" + type + @");
    Xunit.Assert.Equal(" + value + @", v);
} }");
                Assert.Empty(diagnostics);
            }

            [Theory]
            [MemberData(nameof(TypesAndValues))]
            public async void FindsWarningForExpectedConstantOrLiteralValueAsSecondArgument(string type, string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() { 
    var v = default(" + type + @");
    Xunit.Assert.Equal(v, " + value + @");
} }");

                Assert.Collection(diagnostics, d =>
                {
                    Assert.Equal($"The literal or constant value {value} should be the first argument in the call to 'Assert.Equal(expected, actual)' in method 'TestMethod' on type 'TestClass'.", d.GetMessage());
                    Assert.Equal("xUnit2000", d.Id);
                    Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                });
            }
        }
    }
}
