namespace Xunit.Analyzers
{
    using Microsoft.CodeAnalysis;
    using Verify = CSharpVerifier<AssertEqualShouldNotBeUsedForBoolLiteralCheck>;

    public class AssertEqualShouldNotBeUsedForBoolLiteralCheckTests
    {
        public static TheoryData<string> Methods = new TheoryData<string> { "Equal", "NotEqual", "StrictEqual", "NotStrictEqual", };

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForFirstBoolLiteral(string method)
        {
            var source =
@"class TestClass { void TestMethod() {
    bool val = true;
    Xunit.Assert." + method + @"(true, val);
} }";

            var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 29 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("Equal")]
        [InlineData("NotEqual")]
        public async void FindsWarning_ForFirstBoolLiteral_WithCustomComparer(string method)
        {
            var source =
@"class TestClass { void TestMethod() {
    bool val = false;
    Xunit.Assert." + method + @"(false, val, System.Collections.Generic.EqualityComparer<bool>.Default);
} }";

            var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 89 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void DoesNotFindWarning_ForFirstBoolLiteral_ObjectOverload(string method)
        {
            var source =
@"class TestClass { void TestMethod() {
    object val = false;
    Xunit.Assert." + method + @"(true, val);
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void DoesNotFindWarning_ForOtherLiteral(string method)
        {
            var source =
@"class TestClass { void TestMethod() {
    int val = 1;
    Xunit.Assert." + method + @"(1, val);
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void DoesNotFindWarning_ForSecondBoolLiteral(string method)
        {
            var source =
@"class TestClass { void TestMethod() {
    bool val = false;
    Xunit.Assert." + method + @"(val, true);
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }
    }
}
