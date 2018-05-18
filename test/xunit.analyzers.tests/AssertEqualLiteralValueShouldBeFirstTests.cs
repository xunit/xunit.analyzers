using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertEqualLiteralValueShouldBeFirstTests
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
                Assert.Equal($"The literal or constant value {value} should be passed as the 'expected' argument in the call to 'Assert.Equal(expected, actual)' in method 'TestMethod' on type 'TestClass'.", d.GetMessage());
                Assert.Equal("xUnit2000", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(TypesAndValues))]
        public async void DoesNotFindWarningForExpectedConstantOrLiteralValueAsNamedExpectedArgument(string type, string value)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() { 
    var v = default(" + type + @");
    Xunit.Assert.Equal(actual: v, expected: " + value + @");
} }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(TypesAndValues))]
        public async void FindsWarningForExpectedConstantOrLiteralValueAsNamedExpectedArgument(string type, string value)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() { 
    var v = default(" + type + @");
    Xunit.Assert.Equal(actual: " + value + @",expected: v);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"The literal or constant value {value} should be passed as the 'expected' argument in the call to 'Assert.Equal(expected, actual)' in method 'TestMethod' on type 'TestClass'.", d.GetMessage());
                Assert.Equal("xUnit2000", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [InlineData("act", "exp")]
        [InlineData("expected", "expected")]
        [InlineData("actual", "actual")]
        [InlineData("foo", "bar")]
        public async void DoesNotFindWarningWhenArgumentsAreNotNamedCorrectly(string firstArgumentName, string secondArgumentName)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, CompilationReporting.IgnoreErrors,
@"class TestClass { void TestMethod() {
    var v = default(int);
    Xunit.Assert.Equal(" + firstArgumentName + @": 1, " + secondArgumentName + @": v);
} }");

            Assert.Empty(diagnostics);
        }
    }
}
