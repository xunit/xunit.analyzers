using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TestClassMustBePublicFixerTests
    {
        readonly DiagnosticAnalyzer analyzer = new TestClassMustBePublic();
        readonly CodeFixProvider fixer = new TestClassMustBePublicFixer();

        [Theory]
        [InlineData("")]
        [InlineData("internal ")]
        public async void MakesClassPublic(string visibility)
        {
            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, $"{visibility}class TestClass {{ [Xunit.Fact] public void TestMethod() {{ }} }}");

            Assert.Equal("public class TestClass { [Xunit.Fact] public void TestMethod() { } }", result);
        }
    }
}
