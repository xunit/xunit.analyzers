using System.Collections.Generic;
using VerifyCS = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TestClassMustBePublic>;

namespace Xunit.Analyzers
{
    public class TestClassMustBePublicTests
    {
        private static IEnumerable<object[]> CreateFactsInNonPublicClassCases_CSharp()
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
        public async void ForPublicClass_DoesNotFindError_CSharp()
        {
            var source = "public class TestClass { [Xunit.Fact] public void TestMethod() { } }";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(CreateFactsInNonPublicClassCases_CSharp))]
        public async void ForFriendOrInternalClass_FindsError_CSharp(
            string factRelatedAttribute,
            string classAccessModifier)
        {
            var source =   @"
" + classAccessModifier + @" class TestClass 
{ 
    [" + factRelatedAttribute + @"] public void TestMethod() { } 
}";

            var expected = VerifyCS.Diagnostic().WithSpan(2, 8 + classAccessModifier.Length, 2, 17 + classAccessModifier.Length);
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData("public")]
        public async void ForPartialClassInSameFile_WhenClassIsPublic_DoesNotFindError_CSharp(string otherPartAccessModifier)
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

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("")]
        [InlineData("public")]
        public async void ForPartialClassInOtherFiles_WhenClassIsPublic_DoesNotFindError_CSharp(string otherPartAccessModifier)
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

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source1, source2 },
                },
            }.RunAsync();
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", "internal")]
        [InlineData("internal", "internal")]
        public async void ForPartialClassInSameFile_WhenClassIsNonPublic_FindsError_CSharp(
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

            var expected = VerifyCS.Diagnostic().WithSpan(2, 16 + part1AccessModifier.Length, 2, 25 + part1AccessModifier.Length).WithSpan(7, 16 + part2AccessModifier.Length, 7, 25 + part2AccessModifier.Length);
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", "internal")]
        [InlineData("internal", "internal")]
        public async void ForPartialClassInOtherFiles_WhenClassIsNonPublic_FindsError_CSharp(
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

            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { source1, source2 },
                    ExpectedDiagnostics =
                    {
                        VerifyCS.Diagnostic().WithSpan(2, 16 + part1AccessModifier.Length, 2, 25 + part1AccessModifier.Length).WithSpan("Test1.cs", 2, 16 + part2AccessModifier.Length, 2, 25 + part2AccessModifier.Length),
                    },
                },
            }.RunAsync();
        }
    }
}
