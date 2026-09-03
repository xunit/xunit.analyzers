using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ConditionalSkipPropertyValidation>;

public class X1055_ConditionalSkipPropertyValidationTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact({|#0:SkipUnless = nameof(AlwaysTrue)|}, {|#1:SkipWhen = nameof(AlwaysTrue)|})] public void OnFact() { }
				[CulturedFact(new[] { "en-US" }, {|#10:SkipUnless = nameof(AlwaysTrue)|}, {|#11:SkipWhen = nameof(AlwaysTrue)|})] public void OnCulturedFact() { }
				[Theory({|#20:SkipUnless = nameof(AlwaysTrue)|}, {|#21:SkipWhen = nameof(AlwaysTrue)|})] public void OnTheory() { }
				[CulturedTheory(new[] { "en-us" }, {|#30:SkipUnless = nameof(AlwaysTrue)|}, {|#31:SkipWhen = nameof(AlwaysTrue)|})] public void OnCulturedTheory() { }
				[ClassData(typeof(TestClass), {|#40:SkipUnless = nameof(AlwaysTrue)|}, {|#41:SkipWhen = nameof(AlwaysTrue)|})] public void OnClassData() { }
				[InlineData({|#50:SkipUnless = nameof(AlwaysTrue)|}, {|#51:SkipWhen = nameof(AlwaysTrue)|})] public void OnInlineData() { }
				[MemberData("bar", {|#60:SkipUnless = nameof(AlwaysTrue)|}, {|#61:SkipWhen = nameof(AlwaysTrue)|})] public void OnMemberData() { }
				[ClassData<TestClass>({|#70:SkipUnless = nameof(AlwaysTrue)|}, {|#71:SkipWhen = nameof(AlwaysTrue)|})] public void OnClassDataGeneric() { }
			
				public static bool AlwaysTrue => true;
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1055").WithLocation(0),
			Verify.Diagnostic("xUnit1055").WithLocation(1),

			Verify.Diagnostic("xUnit1055").WithLocation(10),
			Verify.Diagnostic("xUnit1055").WithLocation(11),

			Verify.Diagnostic("xUnit1055").WithLocation(20),
			Verify.Diagnostic("xUnit1055").WithLocation(21),

			Verify.Diagnostic("xUnit1055").WithLocation(30),
			Verify.Diagnostic("xUnit1055").WithLocation(31),

			Verify.Diagnostic("xUnit1055").WithLocation(40),
			Verify.Diagnostic("xUnit1055").WithLocation(41),

			Verify.Diagnostic("xUnit1055").WithLocation(50),
			Verify.Diagnostic("xUnit1055").WithLocation(51),

			Verify.Diagnostic("xUnit1055").WithLocation(60),
			Verify.Diagnostic("xUnit1055").WithLocation(61),

			Verify.Diagnostic("xUnit1055").WithLocation(70),
			Verify.Diagnostic("xUnit1055").WithLocation(71),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp11, source, expected);
	}
}
