namespace Xunit.Analyzers
{
    using Verify = CSharpVerifier<DataAttributeShouldBeUsedOnATheory>;

    public class DataAttributeShouldBeUsedOnATheoryTests
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
        public async void DoesNotFindErrorForFactMethodWithDataAttributes(string dataAttribute)
        {
            var source =
                "public class TestClass { [Xunit.Fact, Xunit." + dataAttribute + "] public void TestMethod() { } }";

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
        public async void FindsErrorForMethodsWithDataAttributesButNotFactOrTheory(string dataAttribute)
        {
            var source =
                "public class TestClass { [Xunit." + dataAttribute + "] public void TestMethod() { } }";

            var expected = Verify.Diagnostic().WithSpan(1, 47 + dataAttribute.Length, 1, 57 + dataAttribute.Length);
            await Verify.VerifyAnalyzerAsync(source, expected);
        }
    }
}
