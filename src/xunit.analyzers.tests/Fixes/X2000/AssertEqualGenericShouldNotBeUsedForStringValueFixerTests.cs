using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualGenericShouldNotBeUsedForStringValue>;

public class AssertEqualGenericShouldNotBeUsedForStringValueFixerTests
{
	const string template = /* lang=c#-test */ """
		using Xunit;

		public class TestClass {{
		    [Fact]
		    public void TestMethod() {{
		        string result = "foo";

		        {0};
		    }}
		}}
		""";

	[Theory]
	[InlineData(Constants.Asserts.Equal)]
	[InlineData(Constants.Asserts.StrictEqual)]
	public async Task RemovesGeneric(string assert)
	{
		var before = string.Format(template, $@"[|Assert.{assert}<string>(""foo"", result)|]");
		var after = string.Format(template, @"Assert.Equal(""foo"", result)");

		await Verify.VerifyCodeFix(before, after, AssertEqualGenericShouldNotBeUsedForStringValueFixer.Key_UseStringAssertEqual);
	}
}
