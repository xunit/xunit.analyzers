using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertThrowsShouldUseGenericOverloadTests
    {
        private readonly DiagnosticAnalyzer analyzer = new AssertThrowsShouldUseGenericOverloadCheck();

        public static TheoryData<string> Methods = new TheoryData<string> { "Throws", "ThrowsAsync" };

        private static void AssertHasDiagnostic(IEnumerable<Diagnostic> diagnostics)
        {
            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use typeof() expression to check the exception type.", d.GetMessage());
                Assert.Equal("xUnit2015", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async Task FindsWarning_ForThrowsCheck_WithExceptionParameter_OnThrowingMethod(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { 
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
}

void TestMethod() {
    Xunit.Assert." + method + @"(typeof(System.NotImplementedException), ThrowingMethod);
} }");

            AssertHasDiagnostic(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async Task FindsWarning_ForThrowsCheck_WithExceptionParameter_OnThrowingLambda(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(typeof(System.NotImplementedException), () => System.Threading.Tasks.Task.Delay(0));
} }");

            AssertHasDiagnostic(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void DoesNotFindWarning_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingMethod(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, CompilationReporting.IgnoreErrors,
                @"class TestClass {
System.Threading.Tasks.Task ThrowingMethod() {
    throw new System.NotImplementedException();
} 

async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert." + method + @"<System.NotImplementedException>(ThrowingMethod);
} }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void DoesNotFindWarning_ForThrowsCheck_WithExceptionTypeArgument_OnThrowingLambda(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, CompilationReporting.IgnoreErrors,
                @"class TestClass { async System.Threading.Tasks.Task TestMethod() {
    await Xunit.Assert." + method + @"<System.NotImplementedException>(() => System.Threading.Tasks.Task.Delay(0));
} }");

            Assert.Empty(diagnostics);
        }
    }
}
