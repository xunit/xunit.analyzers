using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldHaveParameters>;

public class X1006_TheoryMethodShouldHaveParametersTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void FactMethod_DoesNotTrigger() { }

				[Theory]
				public void TheoryMethodWithParameters_DoesNotTrigger(string s) { }

				[Theory]
				public void [|TheoryMethodWithoutParameters_Triggers|]() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[CulturedFact(new[] { "en-US" })]
				public void FactMethod_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				public void TheoryMethodWithParameters_DoesNotTrigger(string s) { }

				[CulturedTheory(new[] { "en-US" })]
				public void [|TheoryMethodWithoutParameters_Triggers|]() { }
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}
}
