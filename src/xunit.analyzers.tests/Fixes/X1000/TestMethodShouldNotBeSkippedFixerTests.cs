using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodShouldNotBeSkipped>;

public class TestMethodShouldNotBeSkippedFixerTests
{
	[Fact]
	public async Task RemovesSkipProperty()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Fact([|Skip = "Don't run this"|])]
			    public void TestMethod() { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Fact]
			    public void TestMethod() { }
			}
			""";

		await Verify.VerifyCodeFix(before, after, TestMethodShouldNotBeSkippedFixer.Key_RemoveSkipArgument);
	}
}
