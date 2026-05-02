using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryParameterCannotUseParamsModifier>;

public class X1064_TheoryParameterCannotUseParamsModifierTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public void NonTestMethod(string greeting, int[] values) { }

				[Theory]
				[InlineData("Hello world", new[] { 42 })]
				[InlineData("Hello world", new[] { 2112, 2600 })]
				public void WithoutParams(string greeting, int[] values) { }

				[Theory]
				[InlineData("Hello world", 42)]
				[InlineData("Hello world", 2112, 2600)]
				public void WithParams(string greeting, params int[] {|#0:values|}) { }
			}
			""";

		await Verify.VerifyAnalyzerNonAot(source);
#if NETCOREAPP && ROSLYN_LATEST
		await Verify.VerifyAnalyzerV3Aot(source, Verify.Diagnostic().WithLocation(0));
#endif
	}
}
