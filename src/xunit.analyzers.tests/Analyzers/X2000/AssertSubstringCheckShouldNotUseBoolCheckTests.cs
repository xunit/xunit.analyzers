using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSubstringCheckShouldNotUseBoolCheck>;

public class AssertSubstringCheckShouldNotUseBoolCheckTests
{
	public static TheoryData<string> Methods =
	[
		Constants.Asserts.True,
		Constants.Asserts.False,
	];

	[Theory]
	[InlineData(Constants.Asserts.True, Constants.Asserts.Contains)]
	[InlineData(Constants.Asserts.False, Constants.Asserts.DoesNotContain)]
	public async Task ForBooleanContainsCheck_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.{0}("abc".Contains("a"))|}};
			    }}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForBooleanContainsCheck_WithUserMessage_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.{0}("abc".Contains("a"), "message");
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task ForBooleanTrueStartsWithCheck_Triggers()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    void TestMethod() {
			        {|#0:Xunit.Assert.True("abc".StartsWith("a"))|};
			    }
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.True()", Constants.Asserts.StartsWith);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task ForBooleanTrueStartsWithCheck_WithStringComparison_Triggers()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    void TestMethod() {
			        {|#0:Xunit.Assert.True("abc".StartsWith("a", System.StringComparison.CurrentCulture))|};
			    }
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.True()", Constants.Asserts.StartsWith);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task ForBooleanFalseStartsWithCheck_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    void TestMethod() {
			        Xunit.Assert.False("abc".StartsWith("a"));
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task ForBooleanFalseStartsWithCheck_WithStringComparison_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    void TestMethod() {
			        Xunit.Assert.False("abc".StartsWith("a", System.StringComparison.CurrentCulture));
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForBooleanStartsWithCheck_WithUserMessage_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.{0}("abc".StartsWith("a"), "message");
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForBooleanStartsWithCheck_WithStringComparison_AndUserMessage_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.{0}("abc".StartsWith("a", System.StringComparison.CurrentCulture), "message");
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForBooleanStartsWithCheck_WithBoolAndCulture_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.{0}("abc".StartsWith("a", true, System.Globalization.CultureInfo.CurrentCulture));
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForBooleanStartsWithCheck_WithBoolAndCulture_AndUserMessage_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.{0}("abc".StartsWith("a", true, System.Globalization.CultureInfo.CurrentCulture), "message");
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task ForBooleanTrueEndsWithCheck_Triggers()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    void TestMethod() {
			        {|#0:Xunit.Assert.True("abc".EndsWith("a"))|};
			    }
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.True()", Constants.Asserts.EndsWith);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task ForBooleanTrueEndsWithCheck_WithStringComparison_Triggers()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    void TestMethod() {
			        {|#0:Xunit.Assert.True("abc".EndsWith("a", System.StringComparison.CurrentCulture))|};
			    }
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.True()", Constants.Asserts.EndsWith);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task ForBooleanFalseEndsWithCheck_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    void TestMethod() {
			        Xunit.Assert.False("abc".EndsWith("a"));
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task ForBooleanFalseEndsWithCheck_WithStringComparison_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    void TestMethod() {
			        Xunit.Assert.False("abc".EndsWith("a", System.StringComparison.CurrentCulture));
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForBooleanEndsWithCheck_WithUserMessage_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.{0}("abc".EndsWith("a"), "message");
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForBooleanEndsWithCheck_WithStringComparison_AndUserMessage_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.{0}("abc".EndsWith("a", System.StringComparison.CurrentCulture), "message");
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForBooleanEndsWithCheck_WithBoolAndCulture_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.{0}("abc".EndsWith("a", true, System.Globalization.CultureInfo.CurrentCulture));
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForBooleanEndsWithCheck_WithBoolAndCulture_AndUserMessage_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.{0}("abc".EndsWith("a", true, System.Globalization.CultureInfo.CurrentCulture), "message");
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}
}
