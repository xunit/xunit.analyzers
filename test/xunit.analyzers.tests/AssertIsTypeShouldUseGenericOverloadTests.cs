using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertIsTypeShouldUseGenericOverloadTests
    {
        readonly DiagnosticAnalyzer analyzer = new AssertIsTypeShouldUseGenericOverloadType();

        public static TheoryData<string> Methods = new TheoryData<string> { "IsType", "IsNotType", "IsAssignableFrom" };

        private static void AssertHasDiagnostic(IEnumerable<Diagnostic> diagnostics, string type)
        {
            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use typeof({type}) expression to check the type.", d.GetMessage());
                Assert.Equal("xUnit2007", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForNonGenericCall(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(typeof(int), 1);
} }");

            AssertHasDiagnostic(diagnostics, "int");
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void DoesNotFindWarning_ForGenericCall(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"<int>(1);
} }");

            Assert.Empty(diagnostics);
        }
    }
}
