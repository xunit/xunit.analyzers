using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataShouldBeUniqueWithinTheory>;

public class InlineDataShouldBeUniqueWithinTheoryFixerTests
{
	[Fact]
	public async Task RemovesDuplicateData()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    [[|InlineData(1)|]]
    public void TestMethod(int x) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(1)]
    public void TestMethod(int x) { }
}";

		await Verify.VerifyCodeFix(before, after, InlineDataShouldBeUniqueWithinTheoryFixer.Key_RemoveDuplicateInlineData);
	}
}
