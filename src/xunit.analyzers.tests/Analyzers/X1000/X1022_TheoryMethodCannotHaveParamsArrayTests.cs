using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodCannotHaveParamsArray>;
using Verify_v2_Pre220 = CSharpVerifier<X1022_TheoryMethodCannotHaveParamsArrayTests.Analyzer_v2_Pre220>;

public class X1022_TheoryMethodCannotHaveParamsArrayTests
{
	[Fact]
	public async ValueTask V2_Pre220()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				[Theory]
				public void NonParams_DoesNotTrigger(int a, string b, string[] c) { }

				[Theory]
				public void ParamsModifier_Triggers(int a, string b, {|#0:params string[] c|}) { }
			}
			""";
		var expected = Verify_v2_Pre220.Diagnostic().WithLocation(0).WithArguments("ParamsModifier_Triggers", "TestClass", "c");

		await Verify_v2_Pre220.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				[Theory]
				public void NonParamsDoesNotTrigger(int a, string b, string[] c) { }

				[Theory]
				public void ParamsModifier_DoesNotTrigger(int a, string b, params string[] c) { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	internal class Analyzer_v2_Pre220 : TheoryMethodCannotHaveParamsArray
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 1, 999));
	}
}
