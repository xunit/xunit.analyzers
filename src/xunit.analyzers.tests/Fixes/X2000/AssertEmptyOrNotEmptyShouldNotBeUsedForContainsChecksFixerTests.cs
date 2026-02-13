using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecks>;

public class AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixerTests
{
	const string template = /* lang=c#-test */ """
		using System.Linq;
		using Xunit;

		public class TestClass {{
			[Fact]
			public void TestMethod() {{
				var list = new[] {{ -1, 0, 1, 2 }};

				{0};
			}}

			public bool IsEven(int num) => num % 2 == 0;
		}}
		""";

	[Theory]
	[InlineData(
		/* lang=c#-test */ "{|xUnit2029:Assert.Empty(list.Where(f => f > 0))|}",
		/* lang=c#-test */ "Assert.DoesNotContain(list, f => f > 0)")]
	[InlineData(
		/* lang=c#-test */ "{|xUnit2029:Assert.Empty(list.Where(n => n == 1))|}",
		/* lang=c#-test */ "Assert.DoesNotContain(list, n => n == 1)")]
	[InlineData(
		/* lang=c#-test */ "{|xUnit2029:Assert.Empty(list.Where(IsEven))|}",
		/* lang=c#-test */ "Assert.DoesNotContain(list, IsEven)")]
	public async Task FixerReplacesAssertEmptyWithAssertDoesNotContain(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixer.Key_UseDoesNotContain);
	}

	[Theory]
	[InlineData(
		/* lang=c#-test */ "{|xUnit2030:Assert.NotEmpty(list.Where(f => f > 0))|}",
		/* lang=c#-test */ "Assert.Contains(list, f => f > 0)")]
	[InlineData(
		/* lang=c#-test */ "{|xUnit2030:Assert.NotEmpty(list.Where(n => n == 1))|}",
		/* lang=c#-test */ "Assert.Contains(list, n => n == 1)")]
	[InlineData(
		/* lang=c#-test */ "{|xUnit2030:Assert.NotEmpty(list.Where(IsEven))|}",
		/* lang=c#-test */ "Assert.Contains(list, IsEven)")]
	public async Task FixerReplacesAssertNotEmptyWithAssertContains(
		string beforeAssert,
		string afterAssert)
	{
		var before = string.Format(template, beforeAssert);
		var after = string.Format(template, afterAssert);

		await Verify.VerifyCodeFix(before, after, AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixer.Key_UseContains);
	}

	[Fact]
	public async Task FixAll_ReplacesAssertEmptyWithDoesNotContain()
	{
		var before = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var list = new[] { -1, 0, 1, 2 };

					{|xUnit2029:Assert.Empty(list.Where(f => f > 0))|};
					{|xUnit2029:Assert.Empty(list.Where(n => n == 1))|};
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var list = new[] { -1, 0, 1, 2 };

					Assert.DoesNotContain(list, f => f > 0);
					Assert.DoesNotContain(list, n => n == 1);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixer.Key_UseDoesNotContain);
	}

	[Fact]
	public async Task FixAll_ReplacesAssertNotEmptyWithContains()
	{
		var before = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var list = new[] { -1, 0, 1, 2 };

					{|xUnit2030:Assert.NotEmpty(list.Where(f => f > 0))|};
					{|xUnit2030:Assert.NotEmpty(list.Where(n => n == 1))|};
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var list = new[] { -1, 0, 1, 2 };

					Assert.Contains(list, f => f > 0);
					Assert.Contains(list, n => n == 1);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixer.Key_UseContains);
	}
}
