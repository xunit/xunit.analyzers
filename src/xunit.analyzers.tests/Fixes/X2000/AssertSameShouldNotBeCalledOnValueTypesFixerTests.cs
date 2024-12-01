using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSameShouldNotBeCalledOnValueTypes>;

public class AssertSameShouldNotBeCalledOnValueTypesFixerTests
{
	const string template = /* lang=c#-test */ """
		using Xunit;

		public class TestClass {{
			[Fact]
			public void TestMethod() {{
				var data = 1;

				{0};
			}}
		}}
		""";

	[Theory]
	[InlineData(
		/* lang=c#-test */ "[|Assert.Same(1, data)|]",
		/* lang=c#-test */ "Assert.Equal(1, data)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.NotSame(1, data)|]",
		/* lang=c#-test */ "Assert.NotEqual(1, data)")]
	public async Task ConvertsSameToEqual(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertSameShouldNotBeCalledOnValueTypesFixer.Key_UseAlternateAssert);
	}
}
