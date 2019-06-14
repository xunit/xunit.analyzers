namespace Xunit.Analyzers
{
    using Verify = CSharpVerifier<TheoryMethodShouldHaveParameters>;

    public class TheoryMethodShouldHaveParametersTests
    {
        [Fact]
        public async void DoesNotFindErrorForFactMethod()
        {
            var source = "public class TestClass { [Xunit.Fact] public void TestMethod() { } }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindErrorForTheoryMethodWithParameters()
        {
            var source =
                "public class TestClass { [Xunit.Theory] public void TestMethod(string s) { } }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void FindsErrorForTheoryMethodWithoutParameters()
        {
            var source = "class TestClass { [Xunit.Theory] public void TestMethod() { } }";

            var expected = Verify.Diagnostic().WithSpan(1, 46, 1, 56);
            await Verify.VerifyAnalyzerAsync(source, expected);
        }
    }
}
