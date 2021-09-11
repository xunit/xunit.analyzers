using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSubstringCheckShouldNotUseBoolCheck>;

public class AssertSubstringCheckShouldNotUseBoolCheckFixerTests
{
	const string template = @"
using System;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var data = ""foo bar baz"";

        {0};
    }}
}}";

	[Theory]
	[InlineData(@"[|Assert.True(data.Contains(""foo""))|]", @"Assert.Contains(""foo"", data)")]
	[InlineData(@"[|Assert.True(data.StartsWith(""foo""))|]", @"Assert.StartsWith(""foo"", data)")]
	[InlineData(@"[|Assert.True(data.StartsWith(""foo"", StringComparison.Ordinal))|]", @"Assert.StartsWith(""foo"", data, StringComparison.Ordinal)")]
	[InlineData(@"[|Assert.True(data.EndsWith(""foo""))|]", @"Assert.EndsWith(""foo"", data)")]
	[InlineData(@"[|Assert.True(data.EndsWith(""foo"", StringComparison.OrdinalIgnoreCase))|]", @"Assert.EndsWith(""foo"", data, StringComparison.OrdinalIgnoreCase)")]
	[InlineData(@"[|Assert.False(data.Contains(""foo""))|]", @"Assert.DoesNotContain(""foo"", data)")]
	public async void ConvertsBooleanAssertToStringSpecificAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFixAsync(before, after);
	}
}
