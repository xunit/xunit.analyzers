using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.UseAssertFailInsteadOfBooleanAssert>;

public class UseAssertFailInsteadOfBooleanAssertFixerTests
{
	const string template = @"
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        {0};
    }}
}}";

	[Theory]
	[InlineData(@"[|Assert.True(false, ""message"")|]")]
	[InlineData(@"[|Assert.False(true, ""message"")|]")]
	public async void ReplacesBooleanAssert(string badAssert)
	{
		var before = string.Format(template, badAssert);
		var after = string.Format(template, @"Assert.Fail(""message"")");

		await Verify.VerifyCodeFix(before, after, UseAssertFailInsteadOfBooleanAssertFixer.Key_UseAssertFail);
	}
}
