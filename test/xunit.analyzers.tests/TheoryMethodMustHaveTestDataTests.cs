﻿using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.TheoryMethodMustHaveTestData>;

namespace Xunit.Analyzers
{
    public class TheoryMethodMustHaveTestDataTests
    {
        [Fact]
        public async void DoesNotFindErrorForFactMethod()
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

        [Fact]
        public async void FindsErrorForTheoryMethodMissingData()
        {
            var source = "class TestClass { [Xunit.Theory] public void TestMethod() { } }";

            var expected = Verify.Diagnostic().WithSpan(1, 46, 1, 56);
            await Verify.VerifyAnalyzerAsync(source, expected);
        }
    }
}
