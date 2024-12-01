using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForCollectionSizeCheck>;

public class AssertEqualShouldNotBeUsedForCollectionSizeCheckFixerTests
{
	const string template = /* lang=c#-test */ """
		using System.Linq;
		using Xunit;

		public class TestClass {{
			[Fact]
			public void TestMethod() {{
				var data = new[] {{ 1, 2, 3 }};

				{0};
			}}
		}}
		""";

	[Theory]
	[InlineData(
		/* lang=c#-test */ "[|Assert.Equal(1, data.Count())|]",
		/* lang=c#-test */ "Assert.Single(data)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.Equal(0, data.Count())|]",
		/* lang=c#-test */ "Assert.Empty(data)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.NotEqual(0, data.Count())|]",
		/* lang=c#-test */ "Assert.NotEmpty(data)")]
	public async Task ReplacesCollectionCountWithAppropriateAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertEqualShouldNotBeUsedForCollectionSizeCheckFixer.Key_UseAlternateAssert);
	}
}
