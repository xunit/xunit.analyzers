using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.FactMethodMustNotHaveParameters>;

public class FactMethodMustNotHaveParametersFixerTests
{
	[Fact]
	public async Task RemovesParameter()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Fact]
    public void [|TestMethod|](int x) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyCodeFix(before, after, FactMethodMustNotHaveParametersFixer.Key_RemoveParameters);
	}
}
