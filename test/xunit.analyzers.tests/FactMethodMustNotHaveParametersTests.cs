namespace Xunit.Analyzers
{
    using Verify = CSharpVerifier<FactMethodMustNotHaveParameters>;

    public class FactMethodMustNotHaveParametersTests
    {
        [Fact]
        public async void DoesNotFindErrorForFactWithNoParameters()
        {
            var source = "public class TestClass { [Xunit.Fact] public void TestMethod() { } }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindErrorForTheoryWithParameters()
        {
            var source = "public class TestClass { [Xunit.Theory] public void TestMethod(string p) { } }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void FindsErrorForPrivateClass()
        {
            var source = "public class TestClass { [Xunit.Fact] public void TestMethod(string p) { } }";

            var expected = Verify.Diagnostic().WithSpan(1, 51, 1, 61);
            await Verify.VerifyAnalyzerAsync(source, expected);
        }
    }
}
