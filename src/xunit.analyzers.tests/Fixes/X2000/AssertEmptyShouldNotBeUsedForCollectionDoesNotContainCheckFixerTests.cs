using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck>;

public class AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheckFixerTests
{
	const string template = /* lang=c#-test */ """
		using System.Linq;
		using Xunit;

		public class TestClass {{
		    [Fact]
		    public void TestMethod() {{
		        var list = new[] {{ -1, 0, 1, 2 }};

		        {0};
		    }}

			public bool IsEven(int num) => num % 2 == 0;
		}}
		""";

	[Theory]
	[InlineData(
		/* lang=c#-test */ "[|Assert.Empty(list.Where(f => f > 0))|]",
		/* lang=c#-test */ "Assert.DoesNotContain(list, f => f > 0)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.Empty(list.Where(n => n == 1))|]",
		/* lang=c#-test */ "Assert.DoesNotContain(list, n => n == 1)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.Empty(list.Where(IsEven))|]",
		/* lang=c#-test */ "Assert.DoesNotContain(list, IsEven)")]
	public async Task FixerReplacesAssertEmptyWithAssertDoesNotContain(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheckFixer.Key_UseAlternateAssert);
	}
}
