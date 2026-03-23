using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeNegated>;

public class X2022_BooleanAssertsShouldNotBeNegatedTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var code = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void NegatedBooleanAssertion_Triggers() {
					bool condition = true;

					{|#0:Assert.True(!condition)|};
					{|#1:Assert.False(!condition)|};
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("True", "False"),
			Verify.Diagnostic().WithLocation(1).WithArguments("False", "True"),
		};

		await Verify.VerifyAnalyzer(code, expected);
	}
}
