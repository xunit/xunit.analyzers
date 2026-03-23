using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;
using Verify_v2_Pre240 = CSharpVerifier<InlineDataMustMatchTheoryParametersTests.Analyzer_v2_Pre240>;

public class InlineDataMustMatchTheoryParametersTests
{
	public class NonErrors
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				using System.Runtime.InteropServices;
				using Xunit;

				public class TestClass {
					[Theory]
					[InlineData("abc", 1, null)]
					public void MethodWithImplicitParameters(string a, int b, object c) { }

					[Theory]
					[InlineData(new object[] { "abc", 1, null })]
					public void MethodWithExplicitParameters(string a, int b, object c) { }

					[Theory]
					[InlineData(data: new object[] { "abc", 1, null })]
					public void MethodWithExplicitParameters_Named(string a, int b, object c) { }

					[Theory]
					[InlineData(new byte[0])]
					public void MethodWithEmptyArray(byte[] input) { }

					// https://github.com/xunit/xunit/issues/3000
					[Theory]
					[InlineData({|CS0182:0.1m|})]
					public void MethodWithDecimalValue(decimal m)
					{ }

					// Optional parameters

					[Theory]
					[InlineData("abc")]
					[InlineData("abc", "def")]
					[InlineData("abc", "def", "ghi")]
					public void MethodWithDefaultParameterValues(string a, string b = "default", string c = null) { }

					[Theory]
					[InlineData]
					[InlineData("abc")]
					[InlineData("abc", "def")]
					public void MethodWithOptionalAttribute([Optional] string a, [Optional] string b) { }

					// Params parameter

					[Theory]
					[InlineData("abc", "xyz")]
					public void MethodWithImplicitParams(params string[] args) { }

					[Theory]
					[InlineData(new object[] { new string[] { "abc", "xyz" } })]
					public void MethodWithExplicitParams(params string[] args) { }

					[Theory]
					[InlineData]
					public void MethodWithImplicitEmptyParams(params string[] args) { }

					[Theory]
					[InlineData(new object[] { new string[0] })]
					public void MethodWithExplicitEmptyParams(params string[] args) { }

					[Theory]
					[InlineData(null)]
					public void MethodWithNullParams(params string[] args) { }

					[Theory]
					[InlineData("abc", "xyz")]
					public void MethodWithMixedNormalAndImplicitParams(string first, params string[] args) { }

					[Theory]
					[InlineData("abc", new[] { "xyz" })]
					public void MethodWithMixedNormalAndExplicitParams(string first, params string[] args) { }

					[Theory]
					[InlineData("abc")]
					public void MethodWithMixedNormalAndImplicitEmptyParams(string first, params string[] args) { }

					[Theory]
					[InlineData("abc", new string[] { })]
					public void MethodWithMixedNormalAndExplicitEmptyParams(string first, params string[] args) { }

					[Theory]
					[InlineData("abc", null)]
					public void MethodWithMixedNormalAndNullParams(string first, params string[] args) { }

					// Mixed optional and params parameters

					[Theory]
					[InlineData("abc")]
					public void MethodWithOptionalParametersWithDefaultValuesAndParamsParameter(string a, string b = "default", string c = null, params string[] d) { }
				}

				#nullable enable

				public class NullableTestClass {
					[Theory]
					[InlineData("abc", 1, null)]
					public void MethodWithImplicitParameters(string a, int b, object? c) { }

					[Theory]
					[InlineData(new object[] { "abc", 1, null })]
					public void MethodWithExplicitParameters(string a, int b, object? c) { }

					[Theory]
					[InlineData(data: new object[] { "abc", 1, null })]
					public void MethodWithExplicitParameters_Named(string a, int b, object? c) { }

					// Optional parameters

					[Theory]
					[InlineData("abc")]
					[InlineData("abc", "def")]
					[InlineData("abc", "def", "ghi")]
					public void MethodWithDefaultParameterValues(string a, string b = "default", string? c = null) { }

					[Theory]
					[InlineData]
					[InlineData("abc")]
					[InlineData("abc", "def")]
					public void MethodWithOptionalAttribute([Optional] string? a, [Optional] string? b) { }

					// Params parameter

					[Theory]
					[InlineData(null)]
					public void MethodWithNullParams(params string[]? args) { }

					[Theory]
					[InlineData("abc", null)]
					public void MethodWithMixedNormalAndNullParams(string first, params string[]? args) { }

					// Mixed optional and params parameters

					[Theory]
					[InlineData("abc")]
					public void MethodWithOptionalParametersWithDefaultValuesAndParamsParameter(string a, string b = "default", string? c = null, params string[] d) { }
				}
				""";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Fact]
		public async ValueTask V3_only()
		{
			var source = /* lang=c#-test */ """
				using System.Runtime.InteropServices;
				using Xunit;

				public class TestClass {
					[CulturedTheory(new[] { "en-US" })]
					[InlineData("abc", 1, null)]
					public void MethodWithImplicitParameters(string a, int b, object c) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData(new object[] { "abc", 1, null })]
					public void MethodWithExplicitParameters(string a, int b, object c) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData(data: new object[] { "abc", 1, null })]
					public void MethodWithExplicitParameters_Named(string a, int b, object c) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData(new byte[0])]
					public void MethodWithEmptyArray(byte[] input) { }

					// https://github.com/xunit/xunit/issues/3000
					[CulturedTheory(new[] { "en-US" })]
					[InlineData({|CS0182:0.1m|})]
					public void MethodWithDecimalValue(decimal m)
					{ }

					// Optional parameters

					[CulturedTheory(new[] { "en-US" })]
					[InlineData("abc")]
					[InlineData("abc", "def")]
					[InlineData("abc", "def", "ghi")]
					public void MethodWithDefaultParameterValues(string a, string b = "default", string c = null) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData]
					[InlineData("abc")]
					[InlineData("abc", "def")]
					public void MethodWithOptionalAttribute([Optional] string a, [Optional] string b) { }

					// Params parameter

					[CulturedTheory(new[] { "en-US" })]
					[InlineData("abc", "xyz")]
					public void MethodWithImplicitParams(params string[] args) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData(new object[] { new string[] { "abc", "xyz" } })]
					public void MethodWithExplicitParams(params string[] args) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData]
					public void MethodWithImplicitEmptyParams(params string[] args) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData(new object[] { new string[0] })]
					public void MethodWithExplicitEmptyParams(params string[] args) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData(null)]
					public void MethodWithNullParams(params string[] args) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData("abc", "xyz")]
					public void MethodWithMixedNormalAndImplicitParams(string first, params string[] args) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData("abc", new[] { "xyz" })]
					public void MethodWithMixedNormalAndExplicitParams(string first, params string[] args) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData("abc")]
					public void MethodWithMixedNormalAndImplicitEmptyParams(string first, params string[] args) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData("abc", new string[] { })]
					public void MethodWithMixedNormalAndExplicitEmptyParams(string first, params string[] args) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData("abc", null)]
					public void MethodWithMixedNormalAndNullParams(string first, params string[] args) { }

					// Mixed optional and params parameters

					[CulturedTheory(new[] { "en-US" })]
					[InlineData("abc")]
					public void MethodWithOptionalParametersWithDefaultValuesAndParamsParameter(string a, string b = "default", string c = null, params string[] d) { }
				}

				#nullable enable

				public class NullableTestClass {
					[CulturedTheory(new[] { "en-US" })]
					[InlineData("abc", 1, null)]
					public void MethodWithImplicitParameters(string a, int b, object? c) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData(new object[] { "abc", 1, null })]
					public void MethodWithExplicitParameters(string a, int b, object? c) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData(data: new object[] { "abc", 1, null })]
					public void MethodWithExplicitParameters_Named(string a, int b, object? c) { }

					// Optional parameters

					[CulturedTheory(new[] { "en-US" })]
					[InlineData("abc")]
					[InlineData("abc", "def")]
					[InlineData("abc", "def", "ghi")]
					public void MethodWithDefaultParameterValues(string a, string b = "default", string? c = null) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData]
					[InlineData("abc")]
					[InlineData("abc", "def")]
					public void MethodWithOptionalAttribute([Optional] string? a, [Optional] string? b) { }

					// Params parameter

					[CulturedTheory(new[] { "en-US" })]
					[InlineData(null)]
					public void MethodWithNullParams(params string[]? args) { }

					[CulturedTheory(new[] { "en-US" })]
					[InlineData("abc", null)]
					public void MethodWithMixedNormalAndNullParams(string first, params string[]? args) { }

					// Mixed optional and params parameters

					[CulturedTheory(new[] { "en-US" })]
					[InlineData("abc")]
					public void MethodWithOptionalParametersWithDefaultValuesAndParamsParameter(string a, string b = "default", string? c = null, params string[] d) { }
				}
				""";

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
		}
	}

	public class X1009_TooFewValues
	{
		[Fact]
		public async ValueTask V2_and_V3()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					[Fact]
					[InlineData]
					public void Fact_DoesNotTrigger(string a) { }

					[Theory]
					[{|xUnit1009:InlineData|}]
					public void Theory_NoArguments_Triggers(string a) { }

					[Theory]
					[{|xUnit1009:InlineData(1)|}]
					public void Theory_TooFewArguments_Triggers(int a, int b, string c) { }

					[Theory]
					[{|xUnit1009:InlineData(1)|}]
					public void Theory_TooFewArgumentsWithParams_Triggers(int a, int b, params string[] value) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async ValueTask V3_only()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					[CulturedTheory(new[] { "en-US" })]
					[{|xUnit1009:InlineData|}]
					public void Theory_NoArguments_Triggers(string a) { }

					[CulturedTheory(new[] { "en-US" })]
					[{|xUnit1009:InlineData(1)|}]
					public void Theory_TooFewArguments_Triggers(int a, int b, string c) { }

					[CulturedTheory(new[] { "en-US" })]
					[{|xUnit1009:InlineData(1)|}]
					public void Theory_TooFewArgumentsWithParams_Triggers(int a, int b, params string[] value) { }
				}
				""";

			await Verify.VerifyAnalyzerV3(source);
		}
	}

	public class X1010_IncompatibleValueType
	{
		public class IncorrectParamsArrayType
		{
			[Fact]
			public async ValueTask V2_and_V3()
			{
				var source = /* lang=c#-test */ """
					using Xunit;

					public class TestClass {
						[Theory]
						[InlineData(21.12, {|xUnit1010:new object[] { }|})]
						public void TestMethod(double d, params int[] sq) { }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async ValueTask V3_only()
			{
				var source = /* lang=c#-test */ """
					using Xunit;

					public class TestClass {
						[CulturedTheory(new[] { "en-US" })]
						[InlineData(21.12, {|xUnit1010:new object[] { }|})]
						public void TestMethod(double d, params int[] sq) { }
					}
					""";

				await Verify.VerifyAnalyzerV3(source);
			}
		}

		public class NumericParameter
		{
			public static readonly IEnumerable<TheoryDataRow<string>> NumericTypes =
			[
				"int",
				"uint",
				"long",
				"ulong",
				"short",
				"ushort",
				"byte",
				"sbyte",
				"float",
				"double",
				"decimal",
			];

			public static MatrixTheoryData<string, string> NumericValuesAndNumericTypes =
				new(NumericValues.Select(d => d.Data), NumericTypes.Select(d => d.Data));

			public static MatrixTheoryData<string, string> BoolValuesAndNumericTypes =
				new(BoolValues.Select(d => d.Data), NumericTypes.Select(d => d.Data));

			[Theory]
			[MemberData(nameof(NumericValuesAndNumericTypes))]
			public async ValueTask CompatibleNumericValue_NonNullableType(
				string value,
				string type)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod({1} a) {{ }}
					}}
					""", value, type);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(NumericValuesAndNumericTypes))]
			public async ValueTask CompatibleNumericValue_NullableType(
				string value,
				string type)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod({1}? a) {{ }}
					}}
					""", value, type);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(BoolValuesAndNumericTypes))]
			public async ValueTask BooleanValue_NumericType(
				string value,
				string type)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:{0}|}})]
						public void TestMethod({1} a) {{ }}
					}}
					""", value, type);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("a", type);

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Theory]
			[MemberData(nameof(NumericTypes))]
			public async ValueTask CharValue_NumericType(string type)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData('a')]
						public void TestMethod({0} a) {{ }}
					}}
					""", type);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(NumericTypes))]
			public async ValueTask EnumValue_NumericType(string type)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:System.StringComparison.InvariantCulture|}})]
						public void TestMethod({0} a) {{ }}
					}}
					""", type);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("a", type);

				await Verify.VerifyAnalyzer(source, expected);
			}
		}

		public class BooleanParameter
		{
			[Theory]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			public async ValueTask FromBooleanValue_ToNonNullable(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod(bool a) {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			public async ValueTask FromBooleanValue_ToNullable(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod(bool? a) {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(NumericValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async ValueTask FromIncompatibleValue(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:{0}|}})]
						public void TestMethod(bool a) {{ }}
					}}
					""", value);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("a", "bool");

				await Verify.VerifyAnalyzer(source, expected);
			}
		}

		public class CharParameter
		{
			[Theory]
			[InlineData("'a'")]
			[MemberData(nameof(IntegerValues), MemberType = typeof(X1010_IncompatibleValueType))]
			public async ValueTask FromCharOrIntegerValue_ToNonNullable(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod(char a) {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("'a'")]
			[MemberData(nameof(IntegerValues), MemberType = typeof(X1010_IncompatibleValueType))]
			public async ValueTask FromCharOrIntegerValue_ToNullable(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod(char? a) {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(FloatingPointValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[InlineData("\"abc\"")]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("typeof(string)")]
			public async ValueTask FromIncompatibleValue(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:{0}|}})]
						public void TestMethod(char a) {{ }}
					}}
					""", value);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("a", "char");

				await Verify.VerifyAnalyzer(source, expected);
			}
		}

		public class EnumParameter
		{
			[Fact]
			public async ValueTask FromEnumValue_ToNonNullable()
			{
				var source = /* lang=c#-test */ """
					public class TestClass {
						[Xunit.Theory]
						[Xunit.InlineData(System.StringComparison.Ordinal)]
						public void TestMethod(System.StringComparison a) { }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async ValueTask FromEnumValue_ToNullable()
			{
				var source = /* lang=c#-test */ """
					public class TestClass {
						[Xunit.Theory]
						[Xunit.InlineData(System.StringComparison.Ordinal)]
						public void TestMethod(System.StringComparison? a) { }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(NumericValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async ValueTask FromIncompatibleValue(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:{0}|}})]
						public void TestMethod(System.StringComparison a) {{ }}
					}}
					""", value);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("a", "System.StringComparison");

				await Verify.VerifyAnalyzer(source, expected);
			}
		}

		public class TypeParameter
		{
			[Theory]
			[InlineData("typeof(string)")]
			[InlineData("null")]
			public async ValueTask FromTypeValue(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod(System.Type a) {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("typeof(string)")]
			[InlineData("null")]
			public async ValueTask FromTypeValue_ToParams(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod(params System.Type[] a) {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(NumericValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("System.StringComparison.Ordinal")]
			public async ValueTask FromIncompatibleValue(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:{0}|}})]
						public void TestMethod(System.Type a) {{ }}
					}}
					""", value);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("a", "System.Type");

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Theory]
			[MemberData(nameof(NumericValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("System.StringComparison.Ordinal")]
			public async ValueTask FromIncompatibleValue_ToParams(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:{0}|}})]
						public void TestMethod(params System.Type[] a) {{ }}
					}}
					""", value);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("a", "System.Type");

				await Verify.VerifyAnalyzer(source, expected);
			}
		}

		public class StringParameter
		{
			[Theory]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			public async ValueTask FromStringValue(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod(string a) {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(NumericValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("typeof(string)")]
			public async ValueTask FromIncompatibleValue(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:{0}|}})]
						public void TestMethod(string a) {{ }}
					}}
					""", value);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("a", "string");

				await Verify.VerifyAnalyzer(source, expected);
			}
		}

		public class InterfaceParameter
		{
			[Theory]
			[MemberData(nameof(NumericValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("null")]
			public async ValueTask FromTypeImplementingInterface(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod(System.IFormattable a) {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
#if NETFRAMEWORK
			[InlineData("'a'")]
#endif
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			public async ValueTask FromIncompatibleValue(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:{0}|}})]
						public void TestMethod(System.IFormattable a) {{ }}
					}}
					""", value);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("a", "System.IFormattable");

				await Verify.VerifyAnalyzer(source, expected);
			}
		}

		public class ObjectParameter
		{
			[Theory]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[MemberData(nameof(NumericValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			[InlineData("typeof(string)")]
			public async ValueTask FromAnyValue(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod(object a) {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[MemberData(nameof(NumericValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			[InlineData("typeof(string)")]
			public async ValueTask FromAnyValue_ToParams(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod(params object[] a) {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}
		}

		public class GenericParameter
		{
			[Theory]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[MemberData(nameof(NumericValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			[InlineData("typeof(string)")]
			public async ValueTask FromAnyValue_NoConstraint(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod<T>(T a) {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[MemberData(nameof(NumericValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			public async ValueTask FromValueTypeValue_WithStructConstraint(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod<T>(T a) where T: struct {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async ValueTask FromReferenceTypeValue_WithStructConstraint(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:{0}|}})]
						public void TestMethod<T>(T a) where T: struct {{ }}
					}}
					""", value);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("a", "T");

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Theory]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			[InlineData("null")]
			public async ValueTask FromReferenceTypeValue_WithClassConstraint(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod<T>(T a) where T: class {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[MemberData(nameof(NumericValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			public async ValueTask FromValueTypeValue_WithClassConstraint(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:{0}|}})]
						public void TestMethod<T>(T a) where T: class {{ }}
					}}
					""", value);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("a", "T");

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Theory]
			[InlineData("null")]
			[InlineData("System.StringComparison.Ordinal")]
			[MemberData(nameof(NumericValues), MemberType = typeof(X1010_IncompatibleValueType))]
			public async ValueTask FromCompatibleValue_WithTypeConstraint(string value)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({0})]
						public void TestMethod<T>(T a) where T: System.IConvertible, System.IFormattable {{ }}
					}}
					""", value);

				await Verify.VerifyAnalyzer(source);
			}

#if ROSLYN_LATEST && NET8_0_OR_GREATER

			[Fact]
			public async ValueTask TypeConstraint_WithCRTP() // CRTP: Curiously Recurring Template Pattern or T: Interface<T>
			{
				var source = /* lang=c#-test */ """
				using Xunit;
				using System.Numerics;

				public class TestClass {
					[Theory]
					[InlineData(0U)]
					[InlineData(2U)]
					[InlineData(5294967295U)] // ulong value
					[InlineData({|xUnit1010:-1U|})]
					[InlineData({|xUnit1010:2|})]
					[InlineData({|xUnit1010:0|})]
					[InlineData({|xUnit1010:"A"|})]
					public void UnsignedNumberIsAtLeastZero<T>(T number)
						where T : IUnsignedNumber<T> => Assert.False(T.IsNegative(number));
				}
				""";

				await Verify.VerifyAnalyzer(LanguageVersion.CSharp11, source);
			}

#endif  // ROSLYN_LATEST && NET8_0_OR_GREATER

			[Theory]
#if NETFRAMEWORK
			[InlineData("'a'")]
#endif
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			[MemberData(nameof(BoolValues), MemberType = typeof(X1010_IncompatibleValueType))]
			public async ValueTask FromIncompatibleValue_WithTypeConstraint(string value)
			{
				var source = string.Format(/* lang=c#-test */ """

					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:{0}|}})]
						public void TestMethod<T>(T a) where T: System.IConvertible, System.IFormattable {{ }}
					}}
					""", value);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("a", "T");

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Fact]
			public async ValueTask FromIncompatibleArray()
			{
				var source =/* lang=c#-test */ """
					public class TestClass {
						[Xunit.Theory]
						[Xunit.InlineData(new int[] { {|#0:1|}, 2, 3 })]
						public void TestMethod<T>(T a) where T: System.IConvertible, System.IFormattable { }
					}
					""";
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("a", "T");

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Fact]
			public async ValueTask FromCompatibleArray()
			{
				var source = /* lang=c#-test */ """
					public class TestClass {
						[Xunit.Theory]
						[Xunit.InlineData(new int[] { 1, 2, 3 })]
						public void TestMethod<T>(T[] a) { }
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(SignedIntAndUnsignedInt), MemberType = typeof(X1010_IncompatibleValueType))]
			public async ValueTask FromNegativeInteger_ToUnsignedInteger(
				string signedType,
				string unsignedType)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:({0})-1|}})]
						public void TestMethod({1} value) {{ }}
					}}
					""", signedType, unsignedType);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("value", unsignedType);

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Theory]
			[MemberData(nameof(UnsignedIntegralTypes), MemberType = typeof(X1010_IncompatibleValueType))]
			public async ValueTask FromLongMinValue_ToUnsignedInteger(string unsignedType)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						[Xunit.Theory]
						[Xunit.InlineData({{|#0:long.MinValue|}})]
						public void TestMethod({0} value) {{ }}
					}}
					""", unsignedType);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("value", unsignedType);

				await Verify.VerifyAnalyzer(source, expected);
			}
		}

		public class DateTimeLikeParameter
		{
			public static readonly IEnumerable<TheoryDataRow<string>> ValidDateTimeStrings =
			[
				"\"\"",
				"\"2018-01-02\"",
				"\"2018-01-02 12:34\"",
				"\"obviously-rubbish-datetime-value\"",
				"MyConstString"
			];

			public static MatrixTheoryData<string, string> ValueTypedArgumentsCombinedWithDateTimeLikeTypes =
				new(ValueTypedValues.Select(d => d.Data), ["System.DateTime", "System.DateTimeOffset"]);

			[Theory]
			[MemberData(nameof(ValueTypedArgumentsCombinedWithDateTimeLikeTypes))]
			[InlineData("MyConstInt", "System.DateTime")]
			[InlineData("MyConstInt", "System.DateTimeOffset")]
			public async ValueTask NonStringValue(
				string data,
				string parameterType)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						const int MyConstInt = 1;

						[Xunit.Theory]
						[Xunit.InlineData({{|#0:{0}|}})]
						public void TestMethod({1} parameter) {{ }}
					}}
					""", data, parameterType);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("parameter", parameterType);

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Theory]
			[MemberData(nameof(ValidDateTimeStrings))]
			public async ValueTask StringValue_ToDateTime(string data)
			{
				var source = CreateSourceWithStringConst(data, "System.DateTime");

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(ValidDateTimeStrings))]
			public async ValueTask StringValue_ToDateTimeOffset(string data)
			{
				var source = CreateSourceWithStringConst(data, "System.DateTimeOffset");

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(ValidDateTimeStrings))]
			public async ValueTask StringValue_ToDateTimeOffset_Pre240(string data)
			{
				var source = CreateSourceWithStringConst("{|#0:" + data + "|}", "System.DateTimeOffset");
				var expected = Verify_v2_Pre240.Diagnostic("xUnit1010").WithLocation(0).WithArguments("parameter", "System.DateTimeOffset");

				await Verify_v2_Pre240.VerifyAnalyzer(source, expected);
			}

			static string CreateSourceWithStringConst(
				string data,
				string parameterType) => string.Format(/* lang=c#-test */ """
				public class TestClass {{
					const string MyConstString = "some string";

					[Xunit.Theory]
					[Xunit.InlineData({0})]
					public void TestMethod({1} parameter) {{ }}
				}}
				""", data, parameterType);
		}

		public class GuidParameter
		{
			public static IEnumerable<TheoryDataRow<string>> ValidGuidStrings =
			[
				"\"\"",
				"\"{5B21E154-15EB-4B1E-BC30-127E8A41ECA1}\"",
				"\"4EBCD32C-A2B8-4600-9E72-3873347E285C\"",
				"\"39A3B4C85FEF43A988EB4BB4AC4D4103\"",
				"\"obviously-rubbish-guid-value\""
			];

			[Theory]
			[MemberData(nameof(ValueTypedValues), MemberType = typeof(X1010_IncompatibleValueType))]
			[InlineData("MyConstInt")]
			public async ValueTask NonStringValue(string data)
			{
				var source = string.Format(/* lang=c#-test */ """
					public class TestClass {{
						private const int MyConstInt = 1;

						[Xunit.Theory]
						[Xunit.InlineData({{|#0:{0}|}})]
						public void TestMethod(System.Guid parameter) {{ }}
					}}
					""", data);
				var expected = Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("parameter", "System.Guid");

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Theory]
			[MemberData(nameof(ValidGuidStrings))]
			public async ValueTask StringValue(string inlineData)
			{
				var source = CreateSource(inlineData);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(ValidGuidStrings))]
			public async ValueTask StringValue_Pre240(string data)
			{
				var source = CreateSource("{|#0:" + data + "|}");
				var expected = Verify_v2_Pre240.Diagnostic("xUnit1010").WithLocation(0).WithArguments("parameter", "System.Guid");

				await Verify_v2_Pre240.VerifyAnalyzer(source, expected);
			}

			static string CreateSource(string data) => string.Format(/* lang=c#-test */ """
				public class TestClass {{
					[Xunit.Theory]
					[Xunit.InlineData({0})]
					public void TestMethod(System.Guid parameter) {{ }}
				}}
				""", data);
		}

		public class UserDefinedConversionOperators
		{
			[Fact]
			public async ValueTask V2_and_V3()
			{
				var source = /* lang=c#-test */ """
					using Xunit;

					public class TestClass {
						[Theory]
						[InlineData("abc")]
						public void ParameterDeclaredImplicitConversion(Implicit i) => Assert.Equal("abc", i.Value);

						public class Implicit {
							public string Value { get; set; }
							public static implicit operator Implicit(string value) => new Implicit() { Value = value };
							public static implicit operator string(Implicit i) => i.Value;
						}

						[Theory]
						[InlineData("abc")]
						public void ParameterDeclaredExplicitConversion(Explicit i) => Assert.Equal("abc", i.Value);

						public class Explicit {
							public string Value { get; set; }
							public static explicit operator Explicit(string value) => new Explicit() { Value = value };
							public static explicit operator string(Explicit i) => i.Value;
						}
					}
					""";

				await Verify.VerifyAnalyzerNonAot(source);
			}

#if NETCOREAPP && ROSLYN_LATEST

			[Fact]
			public async ValueTask V3_only_AOT()
			{
				var source = /* lang=c#-test */ """
					using Xunit;

					public class TestClass {
						[Theory]
						[InlineData({|#0:"abc"|})]
						public void ParameterDeclaredImplicitConversion(Implicit i) => Assert.Equal("abc", i.Value);

						public class Implicit {
							public string Value { get; set; }
							public static implicit operator Implicit(string value) => new Implicit() { Value = value };
							public static implicit operator string(Implicit i) => i.Value;
						}

						[Theory]
						[InlineData({|#1:"abc"|})]
						public void ParameterDeclaredExplicitConversion(Explicit i) => Assert.Equal("abc", i.Value);

						public class Explicit {
							public string Value { get; set; }
							public static explicit operator Explicit(string value) => new Explicit() { Value = value };
							public static explicit operator string(Explicit i) => i.Value;
						}
					}
					""";
				var expected = new[] {
					Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("i", "TestClass.Implicit"),
					Verify.Diagnostic("xUnit1010").WithLocation(1).WithArguments("i", "TestClass.Explicit"),
				};

				await Verify.VerifyAnalyzerV3Aot(source, expected);
			}

#endif  // NETCOREAPP && ROSLYN_LATEST
		}

		// Note: decimal literal 42M is not valid as an attribute argument

		public static IEnumerable<TheoryDataRow<string>> BoolValues =
		[
			"true",
			"false"
		];

		public static IEnumerable<TheoryDataRow<string>> FloatingPointValues =
		[
			"42f",
			"42d"
		];

		public static IEnumerable<TheoryDataRow<string>> IntegerValues =
		[
			"42",
			"42L",
			"42u",
			"42ul",
			"(short)42",
			"(byte)42",
			"(ushort)42",
			"(sbyte)42"
		];

		public static IEnumerable<TheoryDataRow<string>> NumericValues =
			IntegerValues.Concat(FloatingPointValues);

		public static IEnumerable<TheoryDataRow<string>> ValueTypedValues =
			IntegerValues.Concat(FloatingPointValues).Concat(BoolValues).Append(new("typeof(int)"));

		public static IEnumerable<TheoryDataRow<string>> SignedIntegralTypes =
			["int", "long", "short", "sbyte"];

		public static IEnumerable<TheoryDataRow<string>> UnsignedIntegralTypes =
			["uint", "ulong", "ushort", "byte"];

		public static readonly MatrixTheoryData<string, string> SignedIntAndUnsignedInt =
			new(
				SignedIntegralTypes.Select(r => r.Data),
				UnsignedIntegralTypes.Select(r => r.Data)
			);
	}

	public class X1011_ExtraValue
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

	public class X1012_NullShouldNotBeUsedForIncompatibleParameter
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

	internal class Analyzer_v2_Pre240 : InlineDataMustMatchTheoryParameters
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 3, 999));
	}
}
