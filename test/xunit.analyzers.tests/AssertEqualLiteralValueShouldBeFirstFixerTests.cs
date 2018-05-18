using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertEqualLiteralValueShouldBeFirstFixerTests
    {
        readonly DiagnosticAnalyzer analyzer = new AssertEqualLiteralValueShouldBeFirst();
        readonly CodeFixProvider fixer = new AssertEqualLiteralValueShouldBeFirstFixer();

        [Fact]
        public async void MakesClassPublic()
        {
            var source =
@"public class TestClass
{
    [Xunit.Fact]
    public void TestMethod()
    {
        var i = 0;
        Xunit.Assert.Equal(i, 0);
    }
}";
            var expected =
                @"public class TestClass
{
    [Xunit.Fact]
    public void TestMethod()
    {
        var i = 0;
        Xunit.Assert.Equal(0, i);
    }
}";

            var actual = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, source);

            Assert.Equal(expected, actual);
        }
    }
}
