namespace Xunit.Analyzers
{
    using Microsoft.CodeAnalysis;
    using Verify = CSharpVerifier<AssertSubstringCheckShouldNotUseBoolCheck>;

    public class AssertSubstringCheckShouldNotUseBoolCheckTests
    {
        public static TheoryData<string> BooleanMethods = new TheoryData<string> { "True", "False" };

        [Theory]
        [MemberData(nameof(BooleanMethods))]
        public async void FindsWarning_ForBooleanContainsCheck(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".Contains(""a""));
} }";

            var expected = Verify.Diagnostic().WithSpan(2, 5, 2, 39 + method.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments($"Assert.{method}()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [MemberData(nameof(BooleanMethods))]
        public async void DoesNotFindWarning_ForBooleanContainsCheck_WithUserMessage(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".Contains(""a""), ""message"");
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void FindsWarning_ForBooleanTrueStartsWithCheck()
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert.True(""abc"".StartsWith(""a""));
} }";

            var expected = Verify.Diagnostic().WithSpan(2, 5, 2, 45).WithSeverity(DiagnosticSeverity.Warning).WithArguments("Assert.True()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async void FindsWarning_ForBooleanTrueStartsWithCheck_WithStringComparison()
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert.True(""abc"".StartsWith(""a"", System.StringComparison.CurrentCulture));
} }";

            var expected = Verify.Diagnostic().WithSpan(2, 5, 2, 85).WithSeverity(DiagnosticSeverity.Warning).WithArguments("Assert.True()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async void DoesNotFindWarning_ForBooleanFalseStartsWithCheck()
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert.False(""abc"".StartsWith(""a""));
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindWarning_ForBooleanFalseStartsWithCheck_WithStringComparison()
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert.False(""abc"".StartsWith(""a"", System.StringComparison.CurrentCulture));
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(BooleanMethods))]
        public async void DoesNotFindWarning_ForBooleanStartsWithCheck_WithUserMessage(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".StartsWith(""a""), ""message"");
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(BooleanMethods))]
        public async void DoesNotFindWarning_ForBooleanStartsWithCheck_WithStringComparison_AndUserMessage(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".StartsWith(""a"", System.StringComparison.CurrentCulture), ""message"");
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(BooleanMethods))]
        public async void DoesNotFindWarning_ForBooleanStartsWithCheck_WithBoolAndCulture(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".StartsWith(""a"", true, System.Globalization.CultureInfo.CurrentCulture));
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(BooleanMethods))]
        public async void DoesNotFindWarning_ForBooleanStartsWithCheck_WithBoolAndCulture_AndUserMessage(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".StartsWith(""a"", true, System.Globalization.CultureInfo.CurrentCulture), ""message"");
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void FindsWarning_ForBooleanTrueEndsWithCheck()
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert.True(""abc"".EndsWith(""a""));
} }";

            var expected = Verify.Diagnostic().WithSpan(2, 5, 2, 43).WithSeverity(DiagnosticSeverity.Warning).WithArguments("Assert.True()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async void FindsWarning_ForBooleanTrueEndsWithCheck_WithStringComparison()
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert.True(""abc"".EndsWith(""a"", System.StringComparison.CurrentCulture));
} }";

            var expected = Verify.Diagnostic().WithSpan(2, 5, 2, 83).WithSeverity(DiagnosticSeverity.Warning).WithArguments("Assert.True()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async void DoesNotFindWarning_ForBooleanFalseEndsWithCheck()
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert.False(""abc"".EndsWith(""a""));
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotFindWarning_ForBooleanFalseEndsWithCheck_WithStringComparison()
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert.False(""abc"".EndsWith(""a"", System.StringComparison.CurrentCulture));
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(BooleanMethods))]
        public async void DoesNotFindWarning_ForBooleanEndsWithCheck_WithUserMessage(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".EndsWith(""a""), ""message"");
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(BooleanMethods))]
        public async void DoesNotFindWarning_ForBooleanEndsWithCheck_WithStringComparison_AndUserMessage(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".EndsWith(""a"", System.StringComparison.CurrentCulture), ""message"");
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(BooleanMethods))]
        public async void DoesNotFindWarning_ForBooleanEndsWithCheck_WithBoolAndCulture(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".EndsWith(""a"", true, System.Globalization.CultureInfo.CurrentCulture));
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(BooleanMethods))]
        public async void DoesNotFindWarning_ForBooleanEndsWithCheck_WithBoolAndCulture_AndUserMessage(string method)
        {
            var source =
                @"class TestClass { void TestMethod() {
    Xunit.Assert." + method + @"(""abc"".EndsWith(""a"", true, System.Globalization.CultureInfo.CurrentCulture), ""message"");
} }";

            await Verify.VerifyAnalyzerAsync(source);
        }
    }
}
