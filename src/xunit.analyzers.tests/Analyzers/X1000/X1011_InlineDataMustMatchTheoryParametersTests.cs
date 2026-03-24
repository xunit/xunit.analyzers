using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;

public class X1011_InlineDataMustMatchTheoryParametersTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				[InlineData(1, 2, "abc")]
				public void Fact_DoesNotTrigger(int a) { }

				[Theory]
				[InlineData(1, {|#0:2|}, {|#1:"abc"|})]
				public void Theory_ExtraArguments_Triggers(int a) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1011").WithLocation(0).WithArguments("2"),
			Verify.Diagnostic("xUnit1011").WithLocation(1).WithArguments("\"abc\""),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1, {|#0:2|}, {|#1:"abc"|})]
				public void CulturedTheory_ExtraArguments_Triggers(int a) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1011").WithLocation(0).WithArguments("2"),
			Verify.Diagnostic("xUnit1011").WithLocation(1).WithArguments("\"abc\""),
		};

		await Verify.VerifyAnalyzerV3(source, expected);
	}
}
