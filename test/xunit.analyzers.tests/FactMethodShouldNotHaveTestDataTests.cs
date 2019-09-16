using VerifyCS = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.FactMethodShouldNotHaveTestData>;

namespace Xunit.Analyzers
{
    public class FactMethodShouldNotHaveTestDataTests
    {
        [Fact]
        public async void DoesNotFindErrorForFactMethodWithNoDataAttributes_CSharp()
        {
            var source = "public class TestClass { [Xunit.Fact] public void TestMethod() { } }";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("InlineData")]
        [InlineData("MemberData(\"\")")]
        [InlineData("ClassData(typeof(string))")]
        public async void DoesNotFindErrorForTheoryMethodWithDataAttributes_CSharp(string dataAttribute)
        {
            var source =
                "public class TestClass { [Xunit.Theory, Xunit." + dataAttribute + "] public void TestMethod() { } }";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("InlineData")]
        [InlineData("MemberData(\"\")")]
        [InlineData("ClassData(typeof(string))")]
        public async void DoesNotFindErrorForDerviedFactMethodWithDataAttributes_CSharp(string dataAttribute)
        {
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources =
                    {
                        "public class DerivedFactAttribute: Xunit.FactAttribute {}",
                        "public class TestClass { [DerivedFactAttribute, Xunit." + dataAttribute + "] public void TestMethod() { } }",
                    },
                },
            }.RunAsync();
        }

        [Theory]
        [InlineData("InlineData")]
        [InlineData("MemberData(\"\")")]
        [InlineData("ClassData(typeof(string))")]
        public async void FindsErrorForFactMethodsWithDataAttributes_CSharp(string dataAttribute)
        {
            var source =
                "public class TestClass { [Xunit.Fact, Xunit." + dataAttribute + "] public void TestMethod() { } }";

            var expected = VerifyCS.Diagnostic().WithSpan(1, 59 + dataAttribute.Length, 1, 69 + dataAttribute.Length);
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }
    }
}
