using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualsShouldNotBeUsed>;

public class X2001_AssertEqualsShouldNotBeUsedTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				void ProhibitedMethod_Triggers() {
					{|CS0619:{|#0:Assert.Equals("Hello world", "HELLO")|}|};
					{|CS0619:{|#1:Assert.ReferenceEquals("Hello world", "HELLO")|}|};
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.Equals()", "Equal"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.ReferenceEquals()", "Same"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
