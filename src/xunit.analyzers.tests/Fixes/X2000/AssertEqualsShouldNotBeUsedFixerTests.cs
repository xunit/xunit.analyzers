using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualsShouldNotBeUsed>;

public class AssertEqualsShouldNotBeUsedFixerTests
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
		/* lang=c#-test */ "{|CS0619:[|Assert.Equals(1, data)|]|}",
		/* lang=c#-test */ "Assert.Equal(1, data)")]
	[InlineData(
		/* lang=c#-test */ "{|CS0619:[|Assert.ReferenceEquals(1, data)|]|}",
		/* lang=c#-test */ "Assert.Same(1, data)")]
	public async Task ConvertsObjectCallToAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertEqualsShouldNotBeUsedFixer.Key_UseAlternateAssert);
	}
}
