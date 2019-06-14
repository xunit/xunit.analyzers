namespace Xunit.Analyzers
{
    using Microsoft.CodeAnalysis;
    using Verify = CSharpVerifier<AssertEqualShouldNotBeUsedForNullCheck>;

    public class AssertEqualShouldNotBeUsedForNullCheckTests
    {
        public static TheoryData<string> Methods = new TheoryData<string> { "Equal", "NotEqual", "StrictEqual", "NotStrictEqual", "Same", "NotSame" };

        [Theory]
        [InlineData("Equal")]
        [InlineData("NotEqual")]
        public async void FindsWarning_ForFirstNullLiteral_StringOverload(string method)
        {
            var source =
@"class TestClass { void TestMethod() {
    string val = null;
    Xunit.Assert." + method + @"(null, val);
} }";

            var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 29 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("Equal")]
        [InlineData("NotEqual")]
        public async void FindsWarning_ForFirstNullLiteral_StringOverload_WithCustomComparer(string method)
        {
            var source =
@"class TestClass { void TestMethod() {
    string val = null;
    Xunit.Assert." + method + @"(null, val, System.StringComparer.Ordinal);
} }";

            var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 60 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForFirstNullLiteral_ObjectOverload(string method)
        {
            var source =
@"class TestClass { void TestMethod() {
    object val = null;
    Xunit.Assert." + method + @"(null, val);
} }";

            var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 29 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("Equal")]
        [InlineData("NotEqual")]
        public async void FindsWarning_ForFirstNullLiteral_ObjectOverload_WithCustomComparer(string method)
        {
            var source =
@"class TestClass { void TestMethod() {
    object val = null;
    Xunit.Assert." + method + @"(null, val, System.Collections.Generic.EqualityComparer<object>.Default);
} }";

            var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 90 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("Equal")]
        [InlineData("NotEqual")]
        [InlineData("StrictEqual")]
        [InlineData("NotStrictEqual")]
        public async void FindsWarning_ForFirstNullLiteral_GenericOverload(string method)
        {
            var source =
@"class TestClass { void TestMethod() {
    TestClass val = null;
    Xunit.Assert." + method + @"<TestClass>(null, val);
} }";

            var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 40 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData("Equal")]
        [InlineData("NotEqual")]
        public async void FindsWarning_ForFirstNullLiteral_GenericOverload_WithCustomComparer(string method)
        {
            var source =
@"class TestClass { void TestMethod() {
    TestClass val = null;
    Xunit.Assert." + method + @"<TestClass>(null, val, System.Collections.Generic.EqualityComparer<TestClass>.Default);
} }";

            var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 104 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()");
            await Verify.VerifyAnalyzerAsync(source, expected);
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
        public async void DoesNotFindWarning_ForSecondNullLiteral(string method)
        {
            var source =
@"class TestClass { void TestMethod() {
    string val = null;
    Xunit.Assert." + method + @"(val, null);
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }
    }
}
