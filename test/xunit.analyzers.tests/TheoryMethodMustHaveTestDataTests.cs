using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TheoryMethodMustHaveTestDataTests
    {
        readonly DiagnosticAnalyzer analyzer = new TheoryMethodMustHaveTestData();

        [Fact]
        public async void DoesNotFindErrorForFactMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { [Xunit.Fact] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("InlineData")]
        [InlineData("MemberData(\"\")")]
        [InlineData("ClassData(typeof(string))")]
        public async void DoesNotFindErrorForTheoryMethodWithDataAttributes(string dataAttribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                "public class TestClass { [Xunit.Theory, Xunit." + dataAttribute + "] public void TestMethod() { } }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async void FindsErrorForTheoryMethodMissingData()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "class TestClass { [Xunit.Theory] public void TestMethod() { } }");

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Theory methods must have test data", d.GetMessage());
                    Assert.Equal("xUnit1003", d.Descriptor.Id);
                });
        }
    }
}
