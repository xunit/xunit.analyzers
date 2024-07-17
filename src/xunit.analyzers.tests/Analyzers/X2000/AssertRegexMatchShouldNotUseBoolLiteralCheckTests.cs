using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertRegexMatchShouldNotUseBoolLiteralCheck>;

public class AssertRegexMatchShouldNotUseBoolLiteralCheckTests
{
	public static TheoryData<string, string> Methods_WithReplacement = new()
	{
		{ Constants.Asserts.True, Constants.Asserts.Matches },
		{ Constants.Asserts.False, Constants.Asserts.DoesNotMatch },
	};

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async Task ForStaticRegexIsMatch_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.{0}(System.Text.RegularExpressions.Regex.IsMatch("abc", "\\w*"))|}};
			    }}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async Task ForInstanceRegexIsMatchWithInlineConstructedRegex_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.{0}(new System.Text.RegularExpressions.Regex("abc").IsMatch("\\w*"))|}};
			    }}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async Task ForInstanceRegexIsMatchWithConstructedRegexVariable_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        var regex = new System.Text.RegularExpressions.Regex("abc");
			        {{|#0:Xunit.Assert.{0}(regex.IsMatch("\\w*"))|}};
			    }}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}
}
