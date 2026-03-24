using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;

public class X1012_InlineDataMustMatchTheoryParametersTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				[InlineData(null)]
				public void Fact_DoesNotTrigger(int a) { }

				[Theory]
				[InlineData({|#0:null|})]
				public void NullValue_Triggers(int a) { }

				[Theory]
				[InlineData({|#1:null|})]
				public void NullForParamsValue_Triggers(params int[] a) { }

				[Theory]
				[InlineData(1, null)]
				public void NullableValueType_DoesNotTrigger(int a, int? b) { }

				[Theory]
				[InlineData(1, null)]
				public void NullableReferenceType_DoesNotTrigger(int a, object b) { }
			}

			#nullable enable

			public class NullableTestClass {
				[Theory]
				[InlineData(1, null)]
				public void NullableReferenceType_DoesNotTrigger(int a, object? b) { }

				[Theory]
				[InlineData(1, {|#10:null|})]
				public void NonNullableReferenceType_Triggers(int a, object b) { }

				[Theory]
				[InlineData(1, "Hello", null, null)]
				public void NullableReferenceTypeParams_DoesNotTrigger(int a, params string?[] b) { }

				[Theory]
				[InlineData(1, "Hello", {|#11:null|}, {|#12:null|})]
				public void NonNullableReferenceTypeParams_DoesNotTrigger(int a, params string[] b) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1012").WithLocation(0).WithArguments("a", "int"),
			Verify.Diagnostic("xUnit1012").WithLocation(1).WithArguments("a", "int"),
			Verify.Diagnostic("xUnit1012").WithLocation(10).WithArguments("b", "object"),
			Verify.Diagnostic("xUnit1012").WithLocation(11).WithArguments("b", "string"),
			Verify.Diagnostic("xUnit1012").WithLocation(12).WithArguments("b", "string"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[CulturedTheory(new[] { "en-US" })]
				[InlineData({|#0:null|})]
				public void NullValue_Triggers(int a) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData({|#1:null|})]
				public void NullForParamsValue_Triggers(params int[] a) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1, null)]
				public void NullableValueType_DoesNotTrigger(int a, int? b) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1, null)]
				public void NullableReferenceType_DoesNotTrigger(int a, object b) { }
			}

			#nullable enable

			public class NullableTestClass {
				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1, null)]
				public void NullableReferenceType_DoesNotTrigger(int a, object? b) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1, {|#10:null|})]
				public void NonNullableReferenceType_Triggers(int a, object b) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1, "Hello", null, null)]
				public void NullableReferenceTypeParams_DoesNotTrigger(int a, params string?[] b) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1, "Hello", {|#11:null|}, {|#12:null|})]
				public void NonNullableReferenceTypeParams_DoesNotTrigger(int a, params string[] b) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1012").WithLocation(0).WithArguments("a", "int"),
			Verify.Diagnostic("xUnit1012").WithLocation(1).WithArguments("a", "int"),
			Verify.Diagnostic("xUnit1012").WithLocation(10).WithArguments("b", "object"),
			Verify.Diagnostic("xUnit1012").WithLocation(11).WithArguments("b", "string"),
			Verify.Diagnostic("xUnit1012").WithLocation(12).WithArguments("b", "string"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
	}
}
