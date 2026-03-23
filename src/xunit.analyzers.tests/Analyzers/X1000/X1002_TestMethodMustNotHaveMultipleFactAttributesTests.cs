using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestMethodMustNotHaveMultipleFactAttributes>;

public class X1002_TestMethodMustNotHaveMultipleFactAttributesTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void Fact_MethodWithSingleAttribute_DoesNotTrigger() { }

				[Theory]
				public void Theory_MethodWithSingleAttribute_DoesNotTrigger() { }

				[Fact]
				[Theory]
				public void [|MethodWithFactAndTheory_Triggers|]() { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async ValueTask V2_and_V3_NonAOT()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class CustomFactAttribute : FactAttribute { }

			public class TestClass {
				[Fact]
				[CustomFact]
				public void [|TestMethod|]() { }
			}
			""";

		await Verify.VerifyAnalyzerNonAot(source);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[CulturedFact(new[] { "en-US" })]
				public void Fact_MethodWithSingleAttribute_DoesNotTrigger() { }

				[CulturedTheory(new[] { "en-US" })]
				public void Theory_MethodWithSingleAttribute_DoesNotTrigger() { }

				[CulturedFact(new[] { "en-US" })]
				[CulturedTheory(new[] { "en-US" })]
				public void [|MethodWithFactAndTheory_Triggers|]() { }
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}
}
