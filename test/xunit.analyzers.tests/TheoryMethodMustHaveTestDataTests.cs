using VerifyCS = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TheoryMethodMustHaveTestData>;

namespace Xunit.Analyzers
{
    public class TheoryMethodMustHaveTestDataTests
    {
        [Fact]
        public async void DoesNotFindErrorForFactMethod_CSharp()
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

        [Fact]
        public async void FindsErrorForTheoryMethodMissingData_CSharp()
        {
            var source = "class TestClass { [Xunit.Theory] public void TestMethod() { } }";

            var expected = VerifyCS.Diagnostic().WithSpan(1, 46, 1, 56);
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }
    }
}
