using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class CollectionDefinitionClassesMustBePublicFixerTests
    {
        readonly DiagnosticAnalyzer analyzer = new CollectionDefinitionClassesMustBePublic();
        readonly CodeFixProvider fixer = new CollectionDefinitionClassesMustBePublicFixer();

        [Theory]
        [InlineData("")]
        [InlineData("internal ")]
        public async void MakesClassPublic(string nonPublicAccessModifier)
        {
            var source = $@"
[Xunit.CollectionDefinition(""MyCollection"")]
{ nonPublicAccessModifier} class CollectionDefinitionClass {{ }}";

            var expected = @"
[Xunit.CollectionDefinition(""MyCollection"")]
public class CollectionDefinitionClass { }";

            var actual = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, source);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void ForPartialClassDeclarations_MakesSingleDeclarationPublic()
        {
            var source = @"
[Xunit.CollectionDefinition(""MyCollection"")]
partial class CollectionDefinitionClass
{

}

partial class CollectionDefinitionClass
{

}";

            var expected = @"
[Xunit.CollectionDefinition(""MyCollection"")]
public partial class CollectionDefinitionClass
{

}

partial class CollectionDefinitionClass
{

}";

            var actual = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, source);

            Assert.Equal(expected, actual);
        }

    }
}
