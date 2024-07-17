using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeNegated>;

public class BooleanAssertsShouldNotBeNegatedTests
{
	[Theory]
	[InlineData("False", "True")]
	[InlineData("True", "False")]
	public async Task NegatedBooleanAssertion_Triggers(
		string assertion,
		string replacement)
	{
		var code = string.Format(/* lang=c#-test */ """
			using Xunit;

			public class TestClass {{
			    [Fact]
			    public void TestMethod() {{
			        bool condition = true;

			        {{|#0:Assert.{0}(!condition)|}};
			    }}
			}}
			""", assertion);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments(assertion, replacement);

		await Verify.VerifyAnalyzer(code, expected);
	}
}
