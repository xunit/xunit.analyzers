using VerifyCS = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldHaveParameters>;

namespace Xunit.Analyzers
{
    public class TheoryMethodShouldHaveParametersTests
    {
        [Fact]
        public async void DoesNotFindErrorForFactMethod_CSharp()
        {
            var source = "public class TestClass { [Xunit.Fact] public void TestMethod() { } }";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindErrorForTheoryMethodWithParameters_CSharp()
        {
            var source =
                "public class TestClass { [Xunit.Theory] public void TestMethod(string s) { } }";

            await VerifyCS.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void FindsErrorForTheoryMethodWithoutParameters_CSharp()
        {
            var source = "class TestClass { [Xunit.Theory] public void TestMethod() { } }";

            var expected = VerifyCS.Diagnostic().WithSpan(1, 46, 1, 56);
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }
    }
}
