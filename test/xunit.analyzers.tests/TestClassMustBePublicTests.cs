namespace Xunit.Analyzers
{
    using System.Collections.Generic;
    using Verify = CSharpVerifier<TestClassMustBePublic>;

    public class TestClassMustBePublicTests
    {
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

            await Verify.VerifyAnalyzerAsync(source);
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

            var expected = Verify.Diagnostic().WithSpan(2, 8 + classAccessModifier.Length, 2, 17 + classAccessModifier.Length);
            await Verify.VerifyAnalyzerAsync(source, expected);
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

            await Verify.VerifyAnalyzerAsync(source);
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

            await new Verify.Test
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

            var expected = Verify.Diagnostic().WithSpan(2, 16 + part1AccessModifier.Length, 2, 25 + part1AccessModifier.Length).WithSpan(7, 16 + part2AccessModifier.Length, 7, 25 + part2AccessModifier.Length);
            await Verify.VerifyAnalyzerAsync(source, expected);
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

            await new Verify.Test
            {
                TestState =
                {
                    Sources = { source1, source2 },
                    ExpectedDiagnostics =
                    {
                        Verify.Diagnostic().WithSpan(2, 16 + part1AccessModifier.Length, 2, 25 + part1AccessModifier.Length).WithSpan("Test1.cs", 2, 16 + part2AccessModifier.Length, 2, 25 + part2AccessModifier.Length),
                    },
                },
            }.RunAsync();
        }
    }
}
