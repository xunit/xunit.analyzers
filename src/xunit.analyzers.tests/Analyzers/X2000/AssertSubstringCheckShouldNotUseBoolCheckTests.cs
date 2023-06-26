using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSubstringCheckShouldNotUseBoolCheck>;

public class AssertSubstringCheckShouldNotUseBoolCheckTests
{
	public static TheoryData<string> Methods = new()
	{
		Constants.Asserts.True,
		Constants.Asserts.False,
	};

	[Theory]
	[InlineData(Constants.Asserts.True, Constants.Asserts.Contains)]
	[InlineData(Constants.Asserts.False, Constants.Asserts.DoesNotContain)]
	public async void FindsWarning_ForBooleanContainsCheck(
		string method,
		string replacement)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(""abc"".Contains(""a""));
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 9, 4, 43 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForBooleanContainsCheck_WithUserMessage(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(""abc"".Contains(""a""), ""message"");
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void FindsWarning_ForBooleanTrueStartsWithCheck()
	{
		var source = @"
class TestClass {
    void TestMethod() {
        Xunit.Assert.True(""abc"".StartsWith(""a""));
    }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 9, 4, 49)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.True()", Constants.Asserts.StartsWith);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void FindsWarning_ForBooleanTrueStartsWithCheck_WithStringComparison()
	{
		var source = @"
class TestClass {
    void TestMethod() {
        Xunit.Assert.True(""abc"".StartsWith(""a"", System.StringComparison.CurrentCulture));
    }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 9, 4, 89)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.True()", Constants.Asserts.StartsWith);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void DoesNotFindWarning_ForBooleanFalseStartsWithCheck()
	{
		var source = @"
class TestClass {
    void TestMethod() {
        Xunit.Assert.False(""abc"".StartsWith(""a""));
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotFindWarning_ForBooleanFalseStartsWithCheck_WithStringComparison()
	{
		var source = @"
class TestClass {
    void TestMethod() {
        Xunit.Assert.False(""abc"".StartsWith(""a"", System.StringComparison.CurrentCulture));
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForBooleanStartsWithCheck_WithUserMessage(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(""abc"".StartsWith(""a""), ""message"");
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForBooleanStartsWithCheck_WithStringComparison_AndUserMessage(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(""abc"".StartsWith(""a"", System.StringComparison.CurrentCulture), ""message"");
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForBooleanStartsWithCheck_WithBoolAndCulture(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(""abc"".StartsWith(""a"", true, System.Globalization.CultureInfo.CurrentCulture));
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForBooleanStartsWithCheck_WithBoolAndCulture_AndUserMessage(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(""abc"".StartsWith(""a"", true, System.Globalization.CultureInfo.CurrentCulture), ""message"");
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void FindsWarning_ForBooleanTrueEndsWithCheck()
	{
		var source = @"
class TestClass {
    void TestMethod() {
        Xunit.Assert.True(""abc"".EndsWith(""a""));
    }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 9, 4, 47)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.True()", Constants.Asserts.EndsWith);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void FindsWarning_ForBooleanTrueEndsWithCheck_WithStringComparison()
	{
		var source = @"
class TestClass {
    void TestMethod() {
        Xunit.Assert.True(""abc"".EndsWith(""a"", System.StringComparison.CurrentCulture));
    }
}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 9, 4, 87)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("Assert.True()", Constants.Asserts.EndsWith);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void DoesNotFindWarning_ForBooleanFalseEndsWithCheck()
	{
		var source = @"
class TestClass {
    void TestMethod() {
        Xunit.Assert.False(""abc"".EndsWith(""a""));
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotFindWarning_ForBooleanFalseEndsWithCheck_WithStringComparison()
	{
		var source = @"
class TestClass {
    void TestMethod() {
        Xunit.Assert.False(""abc"".EndsWith(""a"", System.StringComparison.CurrentCulture));
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForBooleanEndsWithCheck_WithUserMessage(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(""abc"".EndsWith(""a""), ""message"");
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForBooleanEndsWithCheck_WithStringComparison_AndUserMessage(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(""abc"".EndsWith(""a"", System.StringComparison.CurrentCulture), ""message"");
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForBooleanEndsWithCheck_WithBoolAndCulture(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(""abc"".EndsWith(""a"", true, System.Globalization.CultureInfo.CurrentCulture));
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async void DoesNotFindWarning_ForBooleanEndsWithCheck_WithBoolAndCulture_AndUserMessage(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(""abc"".EndsWith(""a"", true, System.Globalization.CultureInfo.CurrentCulture), ""message"");
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}
}
