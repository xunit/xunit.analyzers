using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.FactMethodShouldNotHaveTestData>;

namespace Xunit.Analyzers
{
    public class FactMethodShouldNotHaveTestDataTests
    {
        [Fact]
        public async void DoesNotFindErrorForFactMethodWithNoDataAttributes()
        {
            var source = "public class TestClass { [Xunit.Fact] public void TestMethod() { } }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("InlineData")]
        [InlineData("MemberData(\"\")")]
        [InlineData("ClassData(typeof(string))")]
        public async void DoesNotFindErrorForTheoryMethodWithDataAttributes(string dataAttribute)
        {
            var source =
                "public class TestClass { [Xunit.Theory, Xunit." + dataAttribute + "] public void TestMethod() { } }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData("InlineData")]
        [InlineData("MemberData(\"\")")]
        [InlineData("ClassData(typeof(string))")]
        public async void DoesNotFindErrorForDerivedFactMethodWithDataAttributes(string dataAttribute)
        {
            await new Verify.Test
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
        public async void FindsErrorForFactMethodsWithDataAttributes(string dataAttribute)
        {
            var source =
                "public class TestClass { [Xunit.Fact, Xunit." + dataAttribute + "] public void TestMethod() { } }";

            var expected = Verify.Diagnostic().WithSpan(1, 59 + dataAttribute.Length, 1, 69 + dataAttribute.Length);
            await Verify.VerifyAnalyzerAsync(source, expected);
        }
    }
}
