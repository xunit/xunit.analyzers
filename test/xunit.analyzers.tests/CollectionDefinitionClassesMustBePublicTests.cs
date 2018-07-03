using System.Collections.Generic;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class CollectionDefinitionClassesMustBePublicTests
    {
        private readonly DiagnosticAnalyzer analyzer = new CollectionDefinitionClassesMustBePublic();

        [Fact]
        public async void ForPublicClass_DoesNotFindError()
        {
            var source = @"
[Xunit.CollectionDefinition(""MyCollection"")]
public class CollectionDefinitionClass { }";

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("")]
        [InlineData("internal")]
        public async void ForFriendOrInternalClass_FindsError(string classAccessModifier)
        {
            var source = @"
[Xunit.CollectionDefinition(""MyCollection"")]
" + classAccessModifier + @" class CollectionDefinitionClass { }";

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Collection definition classes must be public", d.GetMessage());
                    Assert.Equal("xUnit1027", d.Descriptor.Id);
                });
        }

        [Theory]
        [InlineData("")]
        [InlineData("public")]
        public async void ForPartialClassInSameFile_WhenClassIsPublic_DoesNotFindError(string otherPartAccessModifier)
        {
            string source = $@"
[Xunit.CollectionDefinition(""MyCollection"")]
public partial class CollectionDefinitionClass {{ }}
{otherPartAccessModifier} partial class CollectionDefinitionClass {{ }}
";
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("")]
        [InlineData("public")]
        public async void ForPartialClassInOtherFiles_WhenClassIsPublic_DoesNotFindError(string otherPartAccessModifier)
        {
            string source1 = @"
[Xunit.CollectionDefinition(""MyCollection"")]
public partial class CollectionDefinitionClass { }";
            string source2 = @"
" + otherPartAccessModifier + @" partial class CollectionDefinitionClass { }
";
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source1, source2);

            Assert.Empty(diagnostics);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", "internal")]
        [InlineData("internal", "internal")]
        public async void ForPartialClassInSameFile_WhenClassIsNonPublic_FindsError(
            string part1AccessModifier,
            string part2AccessModifier)
        {
            string source = @"
[Xunit.CollectionDefinition(""MyCollection"")]
" + part1AccessModifier + @" partial class CollectionDefinitionClass { }
" + part2AccessModifier + @" partial class CollectionDefinitionClass { }
";
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Collection definition classes must be public", d.GetMessage());
                    Assert.Equal("xUnit1027", d.Descriptor.Id);
                });
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", "internal")]
        [InlineData("internal", "internal")]
        public async void ForPartialClassInOtherFiles_WhenClassIsNonPublic_FindsError(
            string part1AccessModifier,
            string part2AccessModifier)
        {
            string source1 = @"
[Xunit.CollectionDefinition(""MyCollection"")]
" + part1AccessModifier + @" partial class CollectionDefinitionClass { }";
            string source2 = @"
" + part2AccessModifier + @" partial class CollectionDefinitionClass { }
";
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source1, source2);

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Collection definition classes must be public", d.GetMessage());
                    Assert.Equal("xUnit1027", d.Descriptor.Id);
                });
        }
    }
}
