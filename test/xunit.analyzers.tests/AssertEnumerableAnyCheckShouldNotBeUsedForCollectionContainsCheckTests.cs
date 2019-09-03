using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheckTests
    {
        private readonly DiagnosticAnalyzer analyzer = new AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck();

        public static TheoryData<string> BooleanMethods = new TheoryData<string> { "True", "False" };

        [Theory]
        [MemberData(nameof(BooleanMethods))]
        public async void FindsWarning_ForLinqAnyCheck(string method)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(new [] { 1 }.Any(i => true));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Enumerable.Any() to check if a value exists in a collection.", d.GetMessage());
                Assert.Equal("xUnit2012", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }
    }
}