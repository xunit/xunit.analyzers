using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertRegexMatchShouldNotUseBoolLiteralCheck>;

public class X2008_AssertRegexMatchShouldNotUseBoolLiteralCheckTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System.Text.RegularExpressions;
			using Xunit;

			class TestClass {
				void ForStaticRegexIsMatch_Triggers() {
					{|#0:Assert.True(Regex.IsMatch("abc", "\\w*"))|};
					{|#1:Assert.False(Regex.IsMatch("abc", "\\w*"))|};
				}

				void ForInstanceRegexIsMatchWithInlineConstructedRegex_Triggers() {
					{|#10:Assert.True(new Regex("abc").IsMatch("\\w*"))|};
					{|#11:Assert.False(new Regex("abc").IsMatch("\\w*"))|};
				}

				void ForInstanceRegexIsMatchWithConstructedRegexVariable_Triggers() {
					var regex = new Regex("abc");

					{|#20:Assert.True(regex.IsMatch("\\w*"))|};
					{|#21:Assert.False(regex.IsMatch("\\w*"))|};
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.True()", "Matches"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.False()", "DoesNotMatch"),

			Verify.Diagnostic().WithLocation(10).WithArguments("Assert.True()", "Matches"),
			Verify.Diagnostic().WithLocation(11).WithArguments("Assert.False()", "DoesNotMatch"),

			Verify.Diagnostic().WithLocation(20).WithArguments("Assert.True()", "Matches"),
			Verify.Diagnostic().WithLocation(21).WithArguments("Assert.False()", "DoesNotMatch"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
