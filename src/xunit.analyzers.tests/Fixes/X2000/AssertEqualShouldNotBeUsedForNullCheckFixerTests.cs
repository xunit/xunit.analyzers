using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForNullCheck>;

public class AssertEqualShouldNotBeUsedForNullCheckFixerTests
{
	const string template = /* lang=c#-test */ """
		using Xunit;

		public class TestClass {{
		    [Fact]
		    public void TestMethod() {{
		        int? data = 1;

		        {0};
		    }}
		}}
		""";

	[Theory]
	[InlineData(
		/* lang=c#-test */ "[|Assert.Equal(null, data)|]",
		/* lang=c#-test */ "Assert.Null(data)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.StrictEqual(null, data)|]",
		/* lang=c#-test */ "Assert.Null(data)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.NotEqual(null, data)|]",
		/* lang=c#-test */ "Assert.NotNull(data)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.NotStrictEqual(null, data)|]",
		/* lang=c#-test */ "Assert.NotNull(data)")]
	public async Task ConvertsToAppropriateNullAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertEqualShouldNotBeUsedForNullCheckFixer.Key_UseAlternateAssert);
	}
}
