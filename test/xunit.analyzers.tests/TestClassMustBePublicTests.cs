using System.Collections.Generic;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TestClassMustBePublicTests
    {
        private readonly DiagnosticAnalyzer analyzer = new TestClassMustBePublic();

        private static IEnumerable<object[]> CreateFactsInNonPublicClassCases()
        {
            foreach (var factAttribute in new[] {"Xunit.Fact", "Xunit.Theory"})
            {
                foreach (var nonPublicAccessModifierForClass in new[] {"", "internal"})
                {
                    yield return new object[] {factAttribute, nonPublicAccessModifierForClass};
                }
            }
        }

        [Fact]
        public async void ForPublicClass_DoesNotFindError()
        {
            var source = "public class TestClass { [Xunit.Fact] public void TestMethod() { } }";

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(CreateFactsInNonPublicClassCases))]
        public async void ForFriendOrInternalClass_FindsError(
            string factRelatedAttribute,
            string classAccessModifier)
        {
            var source =   @"
" + classAccessModifier + @" class TestClass 
{ 
    [" + factRelatedAttribute + @"] public void TestMethod() { } 
}";

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Test classes must be public", d.GetMessage());
                    Assert.Equal("xUnit1000", d.Descriptor.Id);
                });
        }

        [Theory]
        [InlineData("")]
        [InlineData("public")]
        public async void ForPartialClassInSameFile_WhenClassIsPublic_DoesNotFindError(string otherPartAccessModifier)
        {
            string source = @"
public partial class TestClass
{
    [Xunit.Fact] public void Test1() {}
}

" + otherPartAccessModifier + @" partial class TestClass
{
    [Xunit.Fact] public void Test2() {}
}
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
public partial class TestClass
{
    [Xunit.Fact] public void Test1() {}
}";
            string source2 = @"
" + otherPartAccessModifier + @" partial class TestClass
{
    [Xunit.Fact] public void Test2() {}
}
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
" + part1AccessModifier + @" partial class TestClass
{
    [Xunit.Fact] public void Test1() {}
}

" + part2AccessModifier + @" partial class TestClass
{
    [Xunit.Fact] public void Test2() {}
}
";
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Test classes must be public", d.GetMessage());
                    Assert.Equal("xUnit1000", d.Descriptor.Id);
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
" + part1AccessModifier + @" partial class TestClass
{
    [Xunit.Fact] public void Test1() {}
}";
            string source2 = @"
" + part2AccessModifier + @" partial class TestClass
{
    [Xunit.Fact] public void Test2() {}
}
";
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source1, source2);

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Test classes must be public", d.GetMessage());
                    Assert.Equal("xUnit1000", d.Descriptor.Id);
                });
        }
    }
}
