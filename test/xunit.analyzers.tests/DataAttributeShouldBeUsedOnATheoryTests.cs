using VerifyCS = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.DataAttributeShouldBeUsedOnATheory>;

namespace Xunit.Analyzers
{
    public class DataAttributeShouldBeUsedOnATheoryTests
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
        public async void DoesNotFindErrorForFactMethodWithDataAttributes_CSharp(string dataAttribute)
        {
            var source =
                "public class TestClass { [Xunit.Fact, Xunit." + dataAttribute + "] public void TestMethod() { } }";

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
        public async void FindsErrorForMethodsWithDataAttributesButNotFactOrTheory_CSharp(string dataAttribute)
        {
            var source =
                "public class TestClass { [Xunit." + dataAttribute + "] public void TestMethod() { } }";

            var expected = VerifyCS.Diagnostic().WithSpan(1, 47 + dataAttribute.Length, 1, 57 + dataAttribute.Length);
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }
    }
}
