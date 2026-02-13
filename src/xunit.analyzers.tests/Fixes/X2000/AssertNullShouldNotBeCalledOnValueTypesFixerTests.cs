using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertNullShouldNotBeCalledOnValueTypes>;

public class AssertNullShouldNotBeCalledOnValueTypesFixerTests
{
	[Fact]
	public async Task ForValueTypeNullAssert_RemovesAssertion()
	{
		const string before = /* lang=c#-test */ """
			using Xunit;

			public class Tests {
				[Fact]
				public void TestMethod() {
					int i = 1;

					[|Assert.NotNull(i)|];
				}
			}
			""";
		const string after = /* lang=c#-test */ """
			using Xunit;

			public class Tests {
				[Fact]
				public void TestMethod() {
					int i = 1;
				}
			}
			""";

		await Verify.VerifyCodeFix(before, after, AssertNullShouldNotBeCalledOnValueTypesFixer.Key_RemoveAssert);
	}

	// https://github.com/xunit/xunit/issues/1753
	[Fact]
	public async Task ForAssertionWithTrivia_RemovesAssertionAndLeavesLeadingTriviaInPlace()
	{
		const string before = /* lang=c#-test */ """
			using System;
			using Xunit;

			namespace XUnitTestProject1 {
				public class UnitTest1 {
					[Fact]
					public void Test1() {
						int i = 1;

						// I am a comment which gets deleted by the quick fix
						// Assert
						[|Assert.NotNull(i)|];
						Assert.Null(null);
					}
				}
			}
			""";
		const string after = /* lang=c#-test */ """
			using System;
			using Xunit;

			namespace XUnitTestProject1 {
				public class UnitTest1 {
					[Fact]
					public void Test1() {
						int i = 1;

						// I am a comment which gets deleted by the quick fix
						// Assert
						Assert.Null(null);
					}
				}
			}
			""";

		await Verify.VerifyCodeFix(before, after, AssertNullShouldNotBeCalledOnValueTypesFixer.Key_RemoveAssert);
	}

	[Fact]
	public async Task FixAll_RemovesAllValueTypeNullAssertions()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					int i = 1;
					bool b = true;

					[|Assert.NotNull(i)|];
					[|Assert.NotNull(b)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					int i = 1;
					bool b = true;
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertNullShouldNotBeCalledOnValueTypesFixer.Key_RemoveAssert);
	}
}
