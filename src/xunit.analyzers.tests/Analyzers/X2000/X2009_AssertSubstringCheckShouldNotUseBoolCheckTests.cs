using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSubstringCheckShouldNotUseBoolCheck>;

public class X2009_AssertSubstringCheckShouldNotUseBoolCheckTests
{
	public static TheoryData<string> Methods =
	[
		Constants.Asserts.True,
		Constants.Asserts.False,
	];

	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Globalization;
			using Xunit;

			class TestClass {
				void Contains() {
					{|#0:Assert.True("abc".Contains("a"))|};
					{|#1:Assert.False("abc".Contains("a"))|};
				}

				void Contains_WithUserMessage() {
					Assert.True("abc".Contains("a"), "message");
					Assert.False("abc".Contains("a"), "message");
				}

				void StartsWith() {
					{|#10:Assert.True("abc".StartsWith("a"))|};
					{|#11:Assert.True("abc".StartsWith("a", StringComparison.CurrentCulture))|};
					Assert.True("abc".StartsWith("a", false, CultureInfo.CurrentCulture));

					Assert.False("abc".StartsWith("a"));
					Assert.False("abc".StartsWith("a", StringComparison.CurrentCulture));
					Assert.False("abc".StartsWith("a", true, CultureInfo.CurrentCulture));
				}

				void StartsWith_WithUserMessage() {
					Assert.True("abc".StartsWith("a"), "message");
					Assert.True("abc".StartsWith("a", StringComparison.CurrentCulture), "message");
					Assert.True("abc".StartsWith("a", true, CultureInfo.CurrentCulture), "message");

					Assert.False("abc".StartsWith("a"), "message");
					Assert.False("abc".StartsWith("a", StringComparison.CurrentCulture), "message");
					Assert.False("abc".StartsWith("a", true, CultureInfo.CurrentCulture), "message");
				}

				void EndsWith() {
					{|#20:Assert.True("abc".EndsWith("a"))|};
					{|#21:Assert.True("abc".EndsWith("a", StringComparison.CurrentCulture))|};
					Assert.True("abc".EndsWith("a", true, CultureInfo.CurrentCulture));

					Assert.False("abc".EndsWith("a"));
					Assert.False("abc".EndsWith("a", StringComparison.CurrentCulture));
					Assert.False("abc".EndsWith("a", true, CultureInfo.CurrentCulture));
				}

				void EndsWith_WithUserMessage() {
					Assert.True("abc".EndsWith("a"), "message");
					Assert.True("abc".EndsWith("a", StringComparison.CurrentCulture), "message");
					Assert.True("abc".EndsWith("a", true, CultureInfo.CurrentCulture), "message");

					Assert.False("abc".EndsWith("a"), "message");
					Assert.False("abc".EndsWith("a", StringComparison.CurrentCulture), "message");
					Assert.False("abc".EndsWith("a", true, CultureInfo.CurrentCulture), "message");
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.True()", "Contains"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.False()", "DoesNotContain"),

			Verify.Diagnostic().WithLocation(10).WithArguments("Assert.True()", "StartsWith"),
			Verify.Diagnostic().WithLocation(11).WithArguments("Assert.True()", "StartsWith"),

			Verify.Diagnostic().WithLocation(20).WithArguments("Assert.True()", "EndsWith"),
			Verify.Diagnostic().WithLocation(21).WithArguments("Assert.True()", "EndsWith"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
