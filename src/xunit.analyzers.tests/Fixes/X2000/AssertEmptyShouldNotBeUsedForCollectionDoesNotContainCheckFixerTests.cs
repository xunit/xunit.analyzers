using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck>;

public class AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheckFixerTests
{
	const string template = @"
using System.Linq;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var list = new[] {{ -1, 0, 1, 2 }};

        {0};
    }}

	public bool IsEven(int num) => num % 2 == 0;
}}";

	[Theory]
	[InlineData("[|Assert.Empty(list.Where(f => f > 0))|]", "Assert.DoesNotContain(list, f => f > 0)")]
	[InlineData("[|Assert.Empty(list.Where(n => n == 1))|]", "Assert.DoesNotContain(list, n => n == 1)")]
	[InlineData("[|Assert.Empty(list.Where(IsEven))|]", "Assert.DoesNotContain(list, IsEven)")]
	public async Task FixerReplacesAssertEmptyWithAssertDoesNotContain(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheckFixer.Key_UseAlternateAssert);
	}
}
