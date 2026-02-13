using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSubstringCheckShouldNotUseBoolCheck>;

public class AssertSubstringCheckShouldNotUseBoolCheckFixerTests
{
	const string template = /* lang=c#-test */ """
		using System;
		using Xunit;

		public class TestClass {{
			[Fact]
			public void TestMethod() {{
				var data = "foo bar baz";

				{0};
			}}
		}}
		""";

	[Theory]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(data.Contains(""foo""))|]",
		/* lang=c#-test */ @"Assert.Contains(""foo"", data)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(data.StartsWith(""foo""))|]",
		/* lang=c#-test */ @"Assert.StartsWith(""foo"", data)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(data.StartsWith(""foo"", StringComparison.Ordinal))|]",
		/* lang=c#-test */ @"Assert.StartsWith(""foo"", data, StringComparison.Ordinal)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(data.EndsWith(""foo""))|]",
		/* lang=c#-test */ @"Assert.EndsWith(""foo"", data)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(data.EndsWith(""foo"", StringComparison.OrdinalIgnoreCase))|]",
		/* lang=c#-test */ @"Assert.EndsWith(""foo"", data, StringComparison.OrdinalIgnoreCase)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.False(data.Contains(""foo""))|]",
		/* lang=c#-test */ @"Assert.DoesNotContain(""foo"", data)")]
	public async Task ConvertsBooleanAssertToStringSpecificAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertSubstringCheckShouldNotUseBoolCheckFixer.Key_UseAlternateAssert);
	}

	[Fact]
	public async Task FixAll_ReplacesAllBooleanSubstringChecks()
	{
		var before = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = "foo bar baz";

					[|Assert.True(data.Contains("foo"))|];
					[|Assert.False(data.Contains("foo"))|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = "foo bar baz";

					Assert.Contains("foo", data);
					Assert.DoesNotContain("foo", data);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertSubstringCheckShouldNotUseBoolCheckFixer.Key_UseAlternateAssert);
	}
}
