using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForBoolLiteralCheck>;

public class AssertEqualShouldNotBeUsedForBoolLiteralCheckFixerTests
{
	const string template = /* lang=c#-test */ """
		using Xunit;

		public class TestClass {{
			[Fact]
			public void TestMethod() {{
				var actual = true;

				{0};
			}}
		}}
		""";

	[Theory]
	[InlineData(
		/* lang=c#-test */ "[|Assert.Equal(false, actual)|]",
		/* lang=c#-test */ "Assert.False(actual)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.Equal(true, actual)|]",
		/* lang=c#-test */ "Assert.True(actual)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.StrictEqual(false, actual)|]",
		/* lang=c#-test */ "Assert.False(actual)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.StrictEqual(true, actual)|]",
		/* lang=c#-test */ "Assert.True(actual)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.NotEqual(false, actual)|]",
		/* lang=c#-test */ "Assert.True(actual)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.NotEqual(true, actual)|]",
		/* lang=c#-test */ "Assert.False(actual)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.NotStrictEqual(false, actual)|]",
		/* lang=c#-test */ "Assert.True(actual)")]
	[InlineData(
		/* lang=c#-test */ "[|Assert.NotStrictEqual(true, actual)|]",
		/* lang=c#-test */ "Assert.False(actual)")]
	public async Task ConvertsToBooleanAssert(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertEqualShouldNotBeUsedForBoolLiteralCheckFixer.Key_UseAlternateAssert);
	}

	[Fact]
	public async Task FixAll_ConvertsAllToBooleanAsserts()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var actual = true;

					[|Assert.Equal(false, actual)|];
					[|Assert.Equal(true, actual)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var actual = true;

					Assert.False(actual);
					Assert.True(actual);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEqualShouldNotBeUsedForBoolLiteralCheckFixer.Key_UseAlternateAssert);
	}
}
