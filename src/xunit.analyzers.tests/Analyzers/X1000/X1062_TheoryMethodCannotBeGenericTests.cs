using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodCannotBeGeneric>;

public class X1062_TheoryMethodCannotBeGenericTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				public void NonGenericTestMethod() { }

				[Theory]
				public void {|#0:GenericTestMethod|}<T>() { }
			}
			""";

		await Verify.VerifyAnalyzerNonAot(source);
#if NETCOREAPP && ROSLYN_LATEST
		await Verify.VerifyAnalyzerV3Aot(source, Verify.Diagnostic().WithLocation(0));
#endif
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[CulturedTheory(new[] { "en-US" })]
				public void NonGenericTestMethod() { }

				[CulturedTheory(new[] { "en-US" })]
				public void {|#0:GenericTestMethod|}<T>() { }
			}
			""";

		await Verify.VerifyAnalyzerV3NonAot(source);
#if NETCOREAPP && ROSLYN_LATEST
		await Verify.VerifyAnalyzerV3Aot(source, Verify.Diagnostic().WithLocation(0));
#endif
	}
}
