using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertNullShouldNotBeCalledOnValueTypesTests
    {
        readonly DiagnosticAnalyzer analyzer = new AssertNullShouldNotBeCalledOnValueTypes();

        public static TheoryData<string> Methods = new TheoryData<string> { "Null", "NotNull" };

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForValueType(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    int val = 1;
    Xunit.Assert." + method + @"(val);
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}() on value type 'int'.", d.GetMessage());
                Assert.Equal("xUnit2002", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void DoesNotFindWarning_ForNullableValueType(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    int? val = 1;
    Xunit.Assert." + method + @"(val);
} }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void DoesNotFindWarning_ForNullableReferenceType(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
@"class TestClass { void TestMethod() {
    string val = null;
    Xunit.Assert." + method + @"(val);
} }");

            Assert.Empty(diagnostics);
        }
    }
}
