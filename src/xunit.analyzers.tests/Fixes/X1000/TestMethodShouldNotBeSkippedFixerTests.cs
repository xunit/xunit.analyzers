using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodShouldNotBeSkipped>;

public class TestMethodShouldNotBeSkippedFixerTests
{
	[Fact]
	public async Task FixAll_RemovesSkipFromAllMethods()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact([|Skip = "Don't run this"|])]
				public void TestMethod1() { }

				[Fact([|Skip = "Also skipped"|])]
				public void TestMethod2() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod1() { }

				[Fact]
				public void TestMethod2() { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, TestMethodShouldNotBeSkippedFixer.Key_RemoveSkipArgument);
	}
}
