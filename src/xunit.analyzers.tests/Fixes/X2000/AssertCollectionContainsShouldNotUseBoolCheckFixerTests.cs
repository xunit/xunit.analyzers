using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertCollectionContainsShouldNotUseBoolCheck>;

public class AssertCollectionContainsShouldNotUseBoolCheckFixerTests
{
	const string template = /* lang=c#-test */ """
		using System;
		using System.Linq;
		using Xunit;

		public class TestClass {{
			[Fact]
			public void TestMethod() {{
				var items = new[] {{ "a", "b", "c" }};

				{0};
			}}
		}}
		""";

	[Theory]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(items.Contains(""b""))|]",
		/* lang=c#-test */ @"Assert.Contains(""b"", items)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(items.Contains(""b"", StringComparer.Ordinal))|]",
		/* lang=c#-test */ @"Assert.Contains(""b"", items, StringComparer.Ordinal)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.True(items.Contains(""b"", null))|]",
		/* lang=c#-test */ @"Assert.Contains(""b"", items)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.False(items.Contains(""b""))|]",
		/* lang=c#-test */ @"Assert.DoesNotContain(""b"", items)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.False(items.Contains(""b"", StringComparer.Ordinal))|]",
		/* lang=c#-test */ @"Assert.DoesNotContain(""b"", items, StringComparer.Ordinal)")]
	[InlineData(
		/* lang=c#-test */ @"[|Assert.False(items.Contains(""b"", null))|]",
		/* lang=c#-test */ @"Assert.DoesNotContain(""b"", items)")]
	public async Task ReplacesBooleanAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertCollectionContainsShouldNotUseBoolCheckFixer.Key_UseAlternateAssert);
	}

	[Fact]
	public async Task FixAll_ReplacesAllBooleanAsserts()
	{
		var before = /* lang=c#-test */ """
			using System;
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var items = new[] { "a", "b", "c" };

					[|Assert.True(items.Contains("b"))|];
					[|Assert.False(items.Contains("b"))|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var items = new[] { "a", "b", "c" };

					Assert.Contains("b", items);
					Assert.DoesNotContain("b", items);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertCollectionContainsShouldNotUseBoolCheckFixer.Key_UseAlternateAssert);
	}
}
