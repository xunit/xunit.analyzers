using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;

public class InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameterFixerTests
{
	[Fact]
	public async void MakesParameterNullable()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(42, {|xUnit1012:null|})]
    public void TestMethod(int a, int b) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(42, null)]
    public void TestMethod(int a, int? b) { }
}";

		await Verify.VerifyCodeFix(before, after, InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameterFixer.Key_MakeParameterNullable);
	}
}
