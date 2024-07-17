using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualLiteralValueShouldBeFirst>;

public class AssertEqualLiteralValueShouldBeFirstFixerTests
{
	const string Template = /* lang=c#-test */ """
		using System.Collections.Generic;

		public class TestClass {{
		    [Xunit.Fact]
		    public void TestMethod() {{
		        var i = 0;
		        [|Xunit.{0}|];
		    }}
		}}
		""";

	[Theory]
	[InlineData(
		/* lang=c#-test */ "Assert.Equal(i, 0)",
		/* lang=c#-test */ "Assert.Equal(0, i)")]
	[InlineData(
		/* lang=c#-test */ "Assert.Equal(actual: 0, expected: i)",
		/* lang=c#-test */ "Assert.Equal(actual: i, expected: 0)")]
	[InlineData(
		/* lang=c#-test */ "Assert.Equal(expected: i, actual: 0)",
		/* lang=c#-test */ "Assert.Equal(expected: 0, actual: i)")]
	[InlineData(
		/* lang=c#-test */ "Assert.Equal(comparer: default(IEqualityComparer<int>), actual: 0, expected: i)",
		/* lang=c#-test */ "Assert.Equal(comparer: default(IEqualityComparer<int>), actual: i, expected: 0)")]
	[InlineData(
		/* lang=c#-test */ "Assert.Equal(comparer: (x, y) => true, actual: 0, expected: i)",
		/* lang=c#-test */ "Assert.Equal(comparer: (x, y) => true, actual: i, expected: 0)")]
	[InlineData(
		/* lang=c#-test */ "Assert.Equal(expected: i, 0)",
		/* lang=c#-test */ "Assert.Equal(expected: 0, i)",
		LanguageVersion.CSharp7_2)]
	public async Task SwapArguments(
		string beforeAssert,
		string afterAssert,
		LanguageVersion? languageVersion = null)
	{
		var before = string.Format(Template, beforeAssert);
		var after = string.Format(Template, afterAssert);

		if (languageVersion.HasValue)
			await Verify.VerifyCodeFix(languageVersion.Value, before, after, AssertEqualLiteralValueShouldBeFirstFixer.Key_SwapArguments);
		else
			await Verify.VerifyCodeFix(before, after, AssertEqualLiteralValueShouldBeFirstFixer.Key_SwapArguments);
	}
}
