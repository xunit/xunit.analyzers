using Microsoft.CodeAnalysis;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.AssertRegexMatchShouldNotUseBoolLiteralCheck>;

namespace Xunit.Analyzers
{
    public class AssertRegexMatchShouldNotUseBoolLiteralCheckTests
    {
        public static TheoryData<string> Methods = new TheoryData<string> { "True", "False" };

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForStaticRegexIsMatch(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(System.Text.RegularExpressions.Regex.IsMatch(""abc"", ""\\w*""));
} }";

            var expected = Verify.Diagnostic().WithSpan(2, 5, 2, 79 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForInstanceRegexIsMatchWithInlineConstructedRegex(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(new System.Text.RegularExpressions.Regex(""abc"").IsMatch(""\\w*""));
} }";

            var expected = Verify.Diagnostic().WithSpan(2, 5, 2, 83 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForInstanceRegexIsMatchWithConstructedRegexVariable(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    var regex = new System.Text.RegularExpressions.Regex(""abc"");
    Xunit.Assert." + method + @"(regex.IsMatch(""\\w*""));
} }";

            var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 41 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }
    }
}
