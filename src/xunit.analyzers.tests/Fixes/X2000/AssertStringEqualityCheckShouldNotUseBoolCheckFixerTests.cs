using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertStringEqualityCheckShouldNotUseBoolCheck>;

public class AssertStringEqualityCheckShouldNotUseBoolCheckFixerTests
{
	const string template = /* lang=c#-test */ """
		using System;
		using Xunit;

		public class TestClass {{
		    [Fact]
		    public void TestMethod() {{
		        var data = "foo bar baz";

		        {0};
		    }}
		}}
		""";

	[Theory]
	// Instance Equals (true)
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(""foo bar baz"".Equals(data))|]",
		/* lang=c#-test */ @"Assert.Equal(""foo bar baz"", data)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(""foo bar baz"".Equals(data, StringComparison.Ordinal))|]",
		/* lang=c#-test */ @"Assert.Equal(""foo bar baz"", data)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(""foo bar baz"".Equals(data, StringComparison.OrdinalIgnoreCase))|]",
		/* lang=c#-test */ @"Assert.Equal(""foo bar baz"", data, ignoreCase: true)")]
	// Static Equals (true)
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(string.Equals(""foo bar baz"", data))|]",
		/* lang=c#-test */ @"Assert.Equal(""foo bar baz"", data)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(string.Equals(""foo bar baz"", data, StringComparison.Ordinal))|]",
		/* lang=c#-test */ @"Assert.Equal(""foo bar baz"", data)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(string.Equals(""foo bar baz"", data, StringComparison.OrdinalIgnoreCase))|]",
		/* lang=c#-test */ @"Assert.Equal(""foo bar baz"", data, ignoreCase: true)")]
	// Instance Equals (false)
	[InlineData(
		/* lang=c#-test */ @"[|Assert.False(""foo bar baz"".Equals(data))|]",
		/* lang=c#-test */ @"Assert.NotEqual(""foo bar baz"", data)")]
	// Static Equals (false)
	[InlineData(
		/* lang=c#-test */ @"[|Assert.False(string.Equals(""foo bar baz"", data))|]",
		/* lang=c#-test */ @"Assert.NotEqual(""foo bar baz"", data)")]
	public async Task ConvertsBooleanAssertToEqualityAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertStringEqualityCheckShouldNotUseBoolCheckFixer.Key_UseAlternateAssert);
	}
}
