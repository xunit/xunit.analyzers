namespace Xunit.Analyzers
{
    using Microsoft.CodeAnalysis;
    using Verify = CSharpVerifier<AssertEqualPrecisionShouldBeInRange>;

    public class AssertEqualPrecisionShouldBeInRangeTest
    {
        static readonly string Template = "class TestClass {{ void TestMethod() {{{0}}}}}";

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(8)]
        [InlineData(14)]
        [InlineData(15)]
        public async void DoesNotFindError_ForDoubleArgumentWithPrecisionProvidedInRange(int precision)
        {
            var source = string.Format(Template,
                "double num = 0.133d;" +
                $"Xunit.Assert.Equal(0.13d, num, {precision});");

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-2000)]
        [InlineData(-1)]
        [InlineData(16)]
        [InlineData(17000)]
        [InlineData(int.MaxValue)]
        public async void FindsError_ForDoubleArgumentWithPrecisionProvidedOutOfRange(int precision)
        {
            var source = string.Format(Template,
                "double num = 0.133d;" +
                $"Xunit.Assert.Equal(0.13d, num, {precision});");

            var expected = Verify.Diagnostic().WithLocation(1, 89).WithSeverity(DiagnosticSeverity.Error)
                .WithArguments("[0..15]", "double");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(14)]
        [InlineData(27)]
        [InlineData(28)]
        public async void DoesNotFindError_ForDecimalArgumentWithPrecisionProvidedInRange(int precision)
        {
            var source = string.Format(Template,
                "decimal num = 0.133m;" +
                $"Xunit.Assert.Equal(0.13m, num, {precision});");

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-2000)]
        [InlineData(-1)]
        [InlineData(29)]
        [InlineData(30000)]
        [InlineData(int.MaxValue)]
        public async void FindsError_ForDecimalArgumentWithPrecisionProvidedOutOfRange(int precision)
        {
            var source = string.Format(Template,
                "decimal num = 0.133m;" +
                $"Xunit.Assert.Equal(0.13m, num, {precision});");

            var expected = Verify.Diagnostic().WithLocation(1, 90).WithSeverity(DiagnosticSeverity.Error)
                .WithArguments("[0..28]", "decimal");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }
    }
}
