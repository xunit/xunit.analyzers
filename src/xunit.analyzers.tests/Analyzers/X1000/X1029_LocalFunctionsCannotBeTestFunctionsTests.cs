using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.LocalFunctionsCannotBeTestFunctions>;

public class X1029_LocalFunctionsCannotBeTestFunctionsTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static IEnumerable<object[]> MyData;

				public void TestMethod() {
					void NoTestAttribute_DoesNotTrigger() { }

					[{|#0:Fact|}]
					void FactAttribute_Triggers() { }

					[{|#1:Theory|}]
					void TheoryAttribute_Triggers() { }

					[{|#2:InlineData(42)|}]
					void InlineDataAttribute_Triggers() { }

					[{|#3:MemberData(nameof(MyData))|}]
					void MemberDataAttribute_Triggers() { }

					[{|#4:ClassData(typeof(string))|}]
					void ClassDataAttribute_Triggers() { }
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("[Fact]"),
			Verify.Diagnostic().WithLocation(1).WithArguments("[Theory]"),
			Verify.Diagnostic().WithLocation(2).WithArguments("[InlineData(42)]"),
			Verify.Diagnostic().WithLocation(3).WithArguments("[MemberData(nameof(MyData))]"),
			Verify.Diagnostic().WithLocation(4).WithArguments("[ClassData(typeof(string))]"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source, expected);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static IEnumerable<object[]> MyData;

				public void TestMethod() {
					[{|#0:CulturedFact(new[] { "en-us" })|}]
					void CulturedFactAttribute_Triggers() { }

					[{|#1:CulturedTheory(new[] { "en-us" })|}]
					void CulturedTheoryAttribute_Triggers() { }
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments(@"[CulturedFact(new[] { ""en-us"" })]"),
			Verify.Diagnostic().WithLocation(1).WithArguments(@"[CulturedTheory(new[] { ""en-us"" })]"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source, expected);
	}
}
