using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataShouldBeUniqueWithinTheory>;

public class X1025_InlineDataShouldBeUniqueWithinTheoryTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {
				// Facts

				[Fact]
				public void Fact_NoDataAttributes_DoesNotTrigger() { }

				[Fact]
				[InlineData(10)]
				public void Fact_UniqueData_DoesNotTrigger(int n) { }

				[Fact]
				[InlineData(10)]
				[InlineData(10)]
				public void Fact_DuplicateData_DoesNotTrigger(int n) { }

				// Non-InlineData

				[Theory]
				[MemberData("")]
				public void MemberData_DoesNotTrigger() {{ }}

				[Theory]
				[ClassData(typeof(string))]
				public void ClassData_DoesNotTrigger() {{ }}

				// Unique data

				[Theory]
				[InlineData(10)]
				[InlineData(20)]
				public void UniqueData_DoesNotTrigger(int n) { }

				[Theory]
				[InlineData(1)]
				[InlineData(new object[] { 1, 2 })]
				[InlineData(data: new object[] { 1, 2, 3 })]
				public void UniqueParamsData_DoesNotTrigger(params int[] a) {{ }}

				[Theory]
				[InlineData(1)]
				[InlineData(1, "non-default")]
				public void UniqueValues_WithOverridingDefaultValues_DoesNotTrigger(int n, string s = "default") { }

				[Theory]
				[InlineData]
				[InlineData(null)]
				public void NullAndEmpty_DoesNotTrigger(string s) { }

				[Theory]
				[InlineData(null)]
				[InlineData(new[] { 0 })]
				public void NullAndArray_DoesNotTrigger(int[] a) { }

				// Specially crafted InlineData values that will cause the InlineDataUniquenessComparer
				// to return same hashcodes, because GetFlattenedArgumentPrimitives ignores empty arrays.
				// This will trigger the actual bug, where the first parameter object array being equal
				// would cause the other parameters to not be evaluated for equality at all.
				// Specially crafted InlineData values that will cause the InlineDataUniquenessComparer
				// to return same hashcodes, because GetFlattenedArgumentPrimitives ignores empty arrays.
				// This will trigger the actual bug, where the first parameter object array being equal
				// would cause the other parameters to not be evaluated for equality at all.
				[Theory]
				[InlineData(new int[] { 1 }, new int[0], new int[] { 1 })]
				[InlineData(new int[] { 1 }, new int[] { 1 }, new int[0])]
				public void ArrayOrderVariance_DoesNotTrigger(int[] x, int[] y, int[] z) { }

				// Duplicated data

				[Theory]
				[InlineData]
				[{|#0:InlineData|}]
				public void DoubleEmptyInlineData_Triggers(int n) { }

				[Theory]
				[InlineData(null)]
				[{|#1:InlineData(null)|}]
				public void DoubleNullInlineData_Triggers(string s) { }

				[Theory]
				[InlineData(10)]
				[{|#2:InlineData(10)|}]
				public void DoubleValues_Triggers(int n) { }

				private const int ConstantInt = 10;

				[Theory]
				[InlineData(10)]
				[{|#3:InlineData(ConstantInt)|}]
				public void ValueFromConstant_Triggers(int n) { }

				[Theory]
				[InlineData(10, 20)]
				[{|#4:InlineData(new object[] { 10, 20 })|}]
				[{|#5:InlineData(data: new object[] { 10, 20 })|}]
				public void TwoParams_RawValuesVsArgumentArray_Triggers(int x, int y) { }

				[Theory]
				[InlineData(10, 20)]
				[{|#6:InlineData(new object[] { 10, 20 })|}]
				[{|#7:InlineData(data: new object[] { 10, 20 })|}]
				public void ParamsArray_RawValuesVsArgumentArray_Triggers(params int[] a) { }

				[Theory]
				[InlineData(new object[] { 10, 20 })]
				[{|#8:InlineData(data: new object[] { 10, 20 })|}]
				public void DoubledArgumentArrays_Triggers(int x, int y) { }

				[Theory]
				[InlineData(data: new object[] { 10, new object[] { new object[] { 20 }, 30 } })]
				[{|#9:InlineData(new object[] { 10, new object[] { new object[] { 20 }, 30 }})|}]
				public void DoubledComplexValuesForObject_Triggers(object x, object y) { }

				[Theory]
				[InlineData(10, new object[] { new object[] { 20 }, 30 }, 40)]
				[{|#10:InlineData(new object[] { 10, new object[] { new object[] { 20 }, 30 } })|}]
				public void DoubledComplexValues_RawValuesVsArgumentArray_Triggers(object x, object y, int z = 40) { }

				[Theory]
				[InlineData]
				[{|#11:InlineData(42)|}]
				public void DefaultValueVsExplicitValue_Triggers(int n = 42) { }

				[Theory]
				[InlineData(42)]
				[{|#12:InlineData|}]
				public void ExplicitValueVsDefaultValue_Triggers(int n = 42) { }

				[Theory]
				[InlineData]
				[{|#13:InlineData|}]
				public void DefaultValueVsDefaultValue_Triggers(int n = 42) { }

				[Theory]
				[InlineData]
				[{|#14:Xunit.InlineData(null)|}]
				public void DefaultValueVsNull_Triggers(string s = null) { }

				[Theory]
				[InlineData(1, null)]
				[{|#15:InlineData(new object[] { 1, null })|}]
				public void Null_RawValuesVsExplicitArray_Triggers(object x, object y) { }

				[Theory]
				[InlineData(1, default)]
				[{|#16:InlineData(1)|}]
				public void DefaultOfValueType_Triggers(int n, DateTime d = default) { }

				[Theory]
				[InlineData(1, default)]
				[{|#17:InlineData(1)|}]
				public void DefaultOfReferenceType_Triggers(int n, string s = default) { }

				[Theory]
				[InlineData(10)]
				[{|#18:InlineData(10)|}]
				[{|#19:InlineData(10)|}]
				public void Tripled_TriggersTwice(int n) { }

				[Theory]
				[InlineData(10)]
				[InlineData(20)]
				[{|#20:InlineData(10)|}]
				[{|#21:InlineData(20)|}]
				public void DoubledTwice_TriggersTwice(int n) { }
			}

			#nullable enable

			public class NullableTestClass {
				[Theory]
				[InlineData(null)]
				[{|#101:InlineData(null)|}]
				public void DoubleNullInlineData_Triggers(string? s) { }

				[Theory]
				[InlineData]
				[{|#114:Xunit.InlineData(null)|}]
				public void DefaultValueVsNull_Triggers(string? s = null) { }

				[Theory]
				[InlineData(1, null)]
				[{|#115:InlineData(new object[] { 1, null })|}]
				public void Null_RawValuesVsExplicitArray_Triggers(object x, object? y) { }

				[Theory]
				[InlineData(1, default)]
				[{|#117:InlineData(1)|}]
				public void DefaultOfReferenceType_Triggers(int n, string? s = default) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("DoubleEmptyInlineData_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(1).WithArguments("DoubleNullInlineData_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(2).WithArguments("DoubleValues_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(3).WithArguments("ValueFromConstant_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(4).WithArguments("TwoParams_RawValuesVsArgumentArray_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(5).WithArguments("TwoParams_RawValuesVsArgumentArray_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(6).WithArguments("ParamsArray_RawValuesVsArgumentArray_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(7).WithArguments("ParamsArray_RawValuesVsArgumentArray_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(8).WithArguments("DoubledArgumentArrays_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(9).WithArguments("DoubledComplexValuesForObject_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(10).WithArguments("DoubledComplexValues_RawValuesVsArgumentArray_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(11).WithArguments("DefaultValueVsExplicitValue_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(12).WithArguments("ExplicitValueVsDefaultValue_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(13).WithArguments("DefaultValueVsDefaultValue_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(14).WithArguments("DefaultValueVsNull_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(15).WithArguments("Null_RawValuesVsExplicitArray_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(16).WithArguments("DefaultOfValueType_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(17).WithArguments("DefaultOfReferenceType_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(18).WithArguments("Tripled_TriggersTwice", "TestClass"),
			Verify.Diagnostic().WithLocation(19).WithArguments("Tripled_TriggersTwice", "TestClass"),
			Verify.Diagnostic().WithLocation(20).WithArguments("DoubledTwice_TriggersTwice", "TestClass"),
			Verify.Diagnostic().WithLocation(21).WithArguments("DoubledTwice_TriggersTwice", "TestClass"),

			Verify.Diagnostic().WithLocation(101).WithArguments("DoubleNullInlineData_Triggers", "NullableTestClass"),
			Verify.Diagnostic().WithLocation(114).WithArguments("DefaultValueVsNull_Triggers", "NullableTestClass"),
			Verify.Diagnostic().WithLocation(115).WithArguments("Null_RawValuesVsExplicitArray_Triggers", "NullableTestClass"),
			Verify.Diagnostic().WithLocation(117).WithArguments("DefaultOfReferenceType_Triggers", "NullableTestClass"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {
				// Non-InlineData

				[CulturedTheory(new[] { "en-US" })]
				[MemberData("")]
				public void MemberData_DoesNotTrigger() {{ }}

				[CulturedTheory(new[] { "en-US" })]
				[ClassData(typeof(string))]
				public void ClassData_DoesNotTrigger() {{ }}

				// Unique data

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(10)]
				[InlineData(20)]
				public void UniqueData_DoesNotTrigger(int n) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1)]
				[InlineData(new object[] { 1, 2 })]
				[InlineData(data: new object[] { 1, 2, 3 })]
				public void UniqueParamsData_DoesNotTrigger(params int[] a) {{ }}

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1)]
				[InlineData(1, "non-default")]
				public void UniqueValues_WithOverridingDefaultValues_DoesNotTrigger(int n, string s = "default") { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData]
				[InlineData(null)]
				public void NullAndEmpty_DoesNotTrigger(string s) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(null)]
				[InlineData(new[] { 0 })]
				public void NullAndArray_DoesNotTrigger(int[] a) { }

				// Specially crafted InlineData values that will cause the InlineDataUniquenessComparer
				// to return same hashcodes, because GetFlattenedArgumentPrimitives ignores empty arrays.
				// This will trigger the actual bug, where the first parameter object array being equal
				// would cause the other parameters to not be evaluated for equality at all.
				// Specially crafted InlineData values that will cause the InlineDataUniquenessComparer
				// to return same hashcodes, because GetFlattenedArgumentPrimitives ignores empty arrays.
				// This will trigger the actual bug, where the first parameter object array being equal
				// would cause the other parameters to not be evaluated for equality at all.
				[CulturedTheory(new[] { "en-US" })]
				[InlineData(new int[] { 1 }, new int[0], new int[] { 1 })]
				[InlineData(new int[] { 1 }, new int[] { 1 }, new int[0])]
				public void ArrayOrderVariance_DoesNotTrigger(int[] x, int[] y, int[] z) { }

				// Duplicated data

				[CulturedTheory(new[] { "en-US" })]
				[InlineData]
				[{|#0:InlineData|}]
				public void DoubleEmptyInlineData_Triggers(int n) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(null)]
				[{|#1:InlineData(null)|}]
				public void DoubleNullInlineData_Triggers(string s) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(10)]
				[{|#2:InlineData(10)|}]
				public void DoubleValues_Triggers(int n) { }

				private const int ConstantInt = 10;

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(10)]
				[{|#3:InlineData(ConstantInt)|}]
				public void ValueFromConstant_Triggers(int n) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(10, 20)]
				[{|#4:InlineData(new object[] { 10, 20 })|}]
				[{|#5:InlineData(data: new object[] { 10, 20 })|}]
				public void TwoParams_RawValuesVsArgumentArray_Triggers(int x, int y) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(10, 20)]
				[{|#6:InlineData(new object[] { 10, 20 })|}]
				[{|#7:InlineData(data: new object[] { 10, 20 })|}]
				public void ParamsArray_RawValuesVsArgumentArray_Triggers(params int[] a) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(new object[] { 10, 20 })]
				[{|#8:InlineData(data: new object[] { 10, 20 })|}]
				public void DoubledArgumentArrays_Triggers(int x, int y) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(data: new object[] { 10, new object[] { new object[] { 20 }, 30 } })]
				[{|#9:InlineData(new object[] { 10, new object[] { new object[] { 20 }, 30 }})|}]
				public void DoubledComplexValuesForObject_Triggers(object x, object y) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(10, new object[] { new object[] { 20 }, 30 }, 40)]
				[{|#10:InlineData(new object[] { 10, new object[] { new object[] { 20 }, 30 } })|}]
				public void DoubledComplexValues_RawValuesVsArgumentArray_Triggers(object x, object y, int z = 40) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData]
				[{|#11:InlineData(42)|}]
				public void DefaultValueVsExplicitValue_Triggers(int n = 42) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(42)]
				[{|#12:InlineData|}]
				public void ExplicitValueVsDefaultValue_Triggers(int n = 42) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData]
				[{|#13:InlineData|}]
				public void DefaultValueVsDefaultValue_Triggers(int n = 42) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData]
				[{|#14:Xunit.InlineData(null)|}]
				public void DefaultValueVsNull_Triggers(string s = null) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1, null)]
				[{|#15:InlineData(new object[] { 1, null })|}]
				public void Null_RawValuesVsExplicitArray_Triggers(object x, object y) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1, default)]
				[{|#16:InlineData(1)|}]
				public void DefaultOfValueType_Triggers(int n, DateTime d = default) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1, default)]
				[{|#17:InlineData(1)|}]
				public void DefaultOfReferenceType_Triggers(int n, string s = default) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(10)]
				[{|#18:InlineData(10)|}]
				[{|#19:InlineData(10)|}]
				public void Tripled_TriggersTwice(int n) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(10)]
				[InlineData(20)]
				[{|#20:InlineData(10)|}]
				[{|#21:InlineData(20)|}]
				public void DoubledTwice_TriggersTwice(int n) { }
			}

			#nullable enable

			public class NullableTestClass {
				[CulturedTheory(new[] { "en-US" })]
				[InlineData(null)]
				[{|#101:InlineData(null)|}]
				public void DoubleNullInlineData_Triggers(string? s) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData]
				[{|#114:Xunit.InlineData(null)|}]
				public void DefaultValueVsNull_Triggers(string? s = null) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1, null)]
				[{|#115:InlineData(new object[] { 1, null })|}]
				public void Null_RawValuesVsExplicitArray_Triggers(object x, object? y) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(1, default)]
				[{|#117:InlineData(1)|}]
				public void DefaultOfReferenceType_Triggers(int n, string? s = default) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("DoubleEmptyInlineData_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(1).WithArguments("DoubleNullInlineData_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(2).WithArguments("DoubleValues_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(3).WithArguments("ValueFromConstant_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(4).WithArguments("TwoParams_RawValuesVsArgumentArray_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(5).WithArguments("TwoParams_RawValuesVsArgumentArray_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(6).WithArguments("ParamsArray_RawValuesVsArgumentArray_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(7).WithArguments("ParamsArray_RawValuesVsArgumentArray_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(8).WithArguments("DoubledArgumentArrays_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(9).WithArguments("DoubledComplexValuesForObject_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(10).WithArguments("DoubledComplexValues_RawValuesVsArgumentArray_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(11).WithArguments("DefaultValueVsExplicitValue_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(12).WithArguments("ExplicitValueVsDefaultValue_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(13).WithArguments("DefaultValueVsDefaultValue_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(14).WithArguments("DefaultValueVsNull_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(15).WithArguments("Null_RawValuesVsExplicitArray_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(16).WithArguments("DefaultOfValueType_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(17).WithArguments("DefaultOfReferenceType_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(18).WithArguments("Tripled_TriggersTwice", "TestClass"),
			Verify.Diagnostic().WithLocation(19).WithArguments("Tripled_TriggersTwice", "TestClass"),
			Verify.Diagnostic().WithLocation(20).WithArguments("DoubledTwice_TriggersTwice", "TestClass"),
			Verify.Diagnostic().WithLocation(21).WithArguments("DoubledTwice_TriggersTwice", "TestClass"),

			Verify.Diagnostic().WithLocation(101).WithArguments("DoubleNullInlineData_Triggers", "NullableTestClass"),
			Verify.Diagnostic().WithLocation(114).WithArguments("DefaultValueVsNull_Triggers", "NullableTestClass"),
			Verify.Diagnostic().WithLocation(115).WithArguments("Null_RawValuesVsExplicitArray_Triggers", "NullableTestClass"),
			Verify.Diagnostic().WithLocation(117).WithArguments("DefaultOfReferenceType_Triggers", "NullableTestClass"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
	}
}
