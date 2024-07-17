using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheck>;

public class AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheckFixerTests
{
	const string template = /* lang=c#-test */ """
		using System.Linq;
		using Xunit;

		public class TestClass {{
		    [Fact]
		    public void TestMethod() {{
		        var collection = new[] {{ 1, 2, 3 }};

		        {0};
		    }}
		}}
		""";

	[Theory]
	[InlineData(
		/* lang=c#-test */ "[|Assert.True(collection.Any(x => x == 2))|]",
		/* lang=c#-test */ "Assert.Contains(collection, x => x == 2)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.False(collection.Any(x => x == 2))|]",
		/* lang=c#-test */ "Assert.DoesNotContain(collection, x => x == 2)")]
	public async Task ReplacesAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertEnumerableAnyCheckShouldNotBeUsedForCollectionContainsCheckFixer.Key_UseAlternateAssert);
	}
}
