using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertEqualLiteralValueShouldBeFirstFixerTests
    {
        readonly DiagnosticAnalyzer analyzer = new AssertEqualLiteralValueShouldBeFirst();
        readonly CodeFixProvider fixer = new AssertEqualLiteralValueShouldBeFirstFixer();

        static readonly string Template = @"
public class TestClass
{{
    [Xunit.Fact]
    public void TestMethod()
    {{
        var i = 0;
        Xunit.{0};
    }}
}}";

        [Fact]
        public async void SwapArguments()
        {
            var source = string.Format(Template, "Assert.Equal(i, 0)");
            var expected = string.Format(Template, "Assert.Equal(0, i)");

            var actual = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, source);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void NamedArgumentsOnlySwapsArgumentValues()
        {
            var source = string.Format(Template, "Assert.Equal(actual: 0, expected: i)");
            var expected = string.Format(Template, "Assert.Equal(actual: i, expected: 0)");

            var actual = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, source);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void NamedArgumentsInCorrectPositionOnlySwapsArgumentValues()
        {
            var source = string.Format(Template, "Assert.Equal(expected: i, actual: 0)");
            var expected = string.Format(Template, "Assert.Equal(expected: 0, actual: i)");

            var actual = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, source);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void NamedArgumentsTakePossibleThirdParameterIntoAccount()
        {
            var source = string.Format(Template, "Assert.Equal(comparer: null, actual: 0, expected: i)");
            var expected = string.Format(Template, "Assert.Equal(comparer: null, actual: i, expected: 0)");

            var actual = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, source);

            Assert.Equal(expected, actual);
        }
    }
}
