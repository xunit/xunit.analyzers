using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.UseAssertFailInsteadOfBooleanAssert>;
using Verify_V2_Pre25 = CSharpVerifier<X2020_UseAssertFailInsteadOfBooleanAssertTests.Analyzer_Pre25>;

public class X2020_UseAssertFailInsteadOfBooleanAssertTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				void OppositeTestWithMessage_Triggers() {
					{|#0:Assert.True(false, "message")|};
					{|#1:Assert.False(true, "message")|};
				}

				void SameTestWithMessage_DoesNotTrigger() {
					Assert.True(true, "message");
					Assert.False(false, "message");
				}

				void NonConstantInvocation_DoesNotTrigger() {
					var value = true;

					Assert.False(value, "message");
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("True", "false"),
			Verify.Diagnostic().WithLocation(1).WithArguments("False", "true"),
		};

		await Verify.VerifyAnalyzer(source, expected);
		await Verify_V2_Pre25.VerifyAnalyzerV2(source);
	}

	internal class Analyzer_Pre25 : UseAssertFailInsteadOfBooleanAssert
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Assert(compilation, new Version(2, 4, 999));
	}
}
