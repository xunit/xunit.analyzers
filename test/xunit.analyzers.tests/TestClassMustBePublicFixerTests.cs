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
        [InlineData("internal")]
        public async void MakesClassPublic(string nonPublicAccessModifier)
        {
            var source = $"{nonPublicAccessModifier} class TestClass {{ [Xunit.Fact] public void TestMethod() {{ }} }}";

            var expected = "public class TestClass { [Xunit.Fact] public void TestMethod() { } }";

            var actual = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, source);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void ForPartialClassDeclarations_MakesSingleDeclarationPublic()
        {
            var source = @"
partial class TestClass
{
    [Xunit.Fact]
    public void TestMethod1() {}
}

partial class TestClass
{
    [Xunit.Fact]
    public void TestMethod2() {}
}";

            var expected = @"
public partial class TestClass
{
    [Xunit.Fact]
    public void TestMethod1() {}
}

partial class TestClass
{
    [Xunit.Fact]
    public void TestMethod2() {}
}";

            var actual = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, source);

            Assert.Equal(expected, actual);
        }

    }
}
