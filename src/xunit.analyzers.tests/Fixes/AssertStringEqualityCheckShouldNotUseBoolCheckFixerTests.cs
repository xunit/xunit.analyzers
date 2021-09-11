using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertStringEqualityCheckShouldNotUseBoolCheck>;

public class AssertStringEqualityCheckShouldNotUseBoolCheckFixerTests
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
	// Instance Equals (true)
	[InlineData(@"[|Assert.True(""foo bar baz"".Equals(data))|]", @"Assert.Equal(""foo bar baz"", data)")]
	[InlineData(@"[|Assert.True(""foo bar baz"".Equals(data, StringComparison.Ordinal))|]", @"Assert.Equal(""foo bar baz"", data)")]
	[InlineData(@"[|Assert.True(""foo bar baz"".Equals(data, StringComparison.OrdinalIgnoreCase))|]", @"Assert.Equal(""foo bar baz"", data, ignoreCase: true)")]
	// Static Equals (true)
	[InlineData(@"[|Assert.True(string.Equals(""foo bar baz"", data))|]", @"Assert.Equal(""foo bar baz"", data)")]
	[InlineData(@"[|Assert.True(string.Equals(""foo bar baz"", data, StringComparison.Ordinal))|]", @"Assert.Equal(""foo bar baz"", data)")]
	[InlineData(@"[|Assert.True(string.Equals(""foo bar baz"", data, StringComparison.OrdinalIgnoreCase))|]", @"Assert.Equal(""foo bar baz"", data, ignoreCase: true)")]
	// Instance Equals (false)
	[InlineData(@"[|Assert.False(""foo bar baz"".Equals(data))|]", @"Assert.NotEqual(""foo bar baz"", data)")]
	// Static Equals (false)
	[InlineData(@"[|Assert.False(string.Equals(""foo bar baz"", data))|]", @"Assert.NotEqual(""foo bar baz"", data)")]
	public async void ConvertsBooleanAssertToEqualityAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFixAsync(before, after);
	}
}
