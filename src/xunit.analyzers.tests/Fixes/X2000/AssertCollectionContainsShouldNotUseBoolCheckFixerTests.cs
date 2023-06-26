using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertCollectionContainsShouldNotUseBoolCheck>;

public class AssertCollectionContainsShouldNotUseBoolCheckFixerTests
{
	const string template = @"
using System;
using System.Linq;
using Xunit;

public class TestClass {{
    [Fact]
    public void TestMethod() {{
        var items = new[] {{ ""a"", ""b"", ""c"" }};

        {0};
    }}
}}";

	[Theory]
	[InlineData(
		@"[|Assert.True(items.Contains(""b""))|]",
		@"Assert.Contains(""b"", items)")]
	[InlineData(
		@"[|Assert.True(items.Contains(""b"", StringComparer.Ordinal))|]",
		@"Assert.Contains(""b"", items, StringComparer.Ordinal)")]
	[InlineData(
		@"[|Assert.False(items.Contains(""b""))|]",
		@"Assert.DoesNotContain(""b"", items)")]
	[InlineData(
		@"[|Assert.False(items.Contains(""b"", StringComparer.Ordinal))|]",
		@"Assert.DoesNotContain(""b"", items, StringComparer.Ordinal)")]
	public async void ReplacesBooleanAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertCollectionContainsShouldNotUseBoolCheckFixer.Key_UseAlternateAssert);
	}
}
