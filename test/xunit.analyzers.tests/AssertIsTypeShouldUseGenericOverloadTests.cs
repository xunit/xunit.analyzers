namespace Xunit.Analyzers
{
    using Microsoft.CodeAnalysis;
    using Verify = CSharpVerifier<AssertIsTypeShouldUseGenericOverloadType>;

    public class AssertIsTypeShouldUseGenericOverloadTests
    {
        public static TheoryData<string> Methods = new TheoryData<string> { "IsType", "IsNotType", "IsAssignableFrom" };

        [Theory]
        [MemberData(nameof(Methods))]
        public async void FindsWarning_ForNonGenericCall(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(typeof(int), 1);
} }";

            var expected = Verify.Diagnostic().WithSpan(2, 5, 2, 34 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments("int");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [MemberData(nameof(Methods))]
        public async void DoesNotFindWarning_ForGenericCall(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"<int>(1);
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }
    }
}
