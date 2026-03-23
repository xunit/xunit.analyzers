using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.FactMethodMustNotHaveParameters>;

public class X1001_FactMethodMustNotHaveParametersTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void NoParameters_DoesNotTrigger() { }

				[Theory]
				public void TheoryWithParameters_DoesNotTrigger(string p) { }

				[Fact]
				public void [|FactWithParameters_Triggers|](string p) { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
