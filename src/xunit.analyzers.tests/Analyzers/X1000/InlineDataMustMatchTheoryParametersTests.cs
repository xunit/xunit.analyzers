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
		public async Task MethodUsingParamsArgument()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData("abc", "xyz")]
				    public void TestMethod(params string[] args) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingNullParamsArgument_NonNullable()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData(null)]
				    public void TestMethod(params string[] args) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingNullParamsArgument_Nullable()
		{
			var source = /* lang=c#-test */ """
				#nullable enable

				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData(null)]
				    public void TestMethod(params string[]? args) { }
				}
				""";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source);
		}

		[Fact]
		public async Task MethodUsingNormalAndParamsArgument()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData("abc", "xyz")]
				    public void TestMethod(string first, params string[] args) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingNormalAndNullParamsArgument_NonNullable()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData("abc", null)]
				    public void TestMethod(string first, params string[] args) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingNormalAndNullParamsArgument_Nullable()
		{
			var source = /* lang=c#-test */ """
				#nullable enable

				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData("abc", null)]
				    public void TestMethod(string first, params string[]? args) { }
				}
				""";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source);
		}

		[Fact]
		public async Task MethodUsingNormalAndUnusedParamsArgument()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData("abc")]
				    public void TestMethod(string first, params string[] args) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingEmptyArrayForParams()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData(new int[] { })]
				    public void VariableArgumentsTest(params int[] sq) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingMixedArgumentsAndEmptyArrayForParams()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData(21.12, new int[] { })]
				    public void VariableArgumentsTest(double d, params int[] sq) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingNonEmptyArrayForParams()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData(new int[] { 1, 2, 3 })]
				    public void VariableArgumentsTest(params int[] sq) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingMixedArgumentsAndNonEmptyArrayForParams()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData(21.12, new int[] { 1, 2, 3 })]
				    public void VariableArgumentsTest(double d, params int[] sq) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingParameters()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData("abc", 1, null)]
				    public void TestMethod(string a, int b, object c) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingParametersWithDefaultValues()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData("abc")]
				    public void TestMethod(string a, string b = "default", string c = null) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingParametersWithDefaultValuesAndParamsArgument()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData("abc")]
				    public void TestMethod(string a, string b = "default", string c = null, params string[] d) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingParameterWithOptionalAttribute()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData("abc")]
				    public void TestMethod(string a, [System.Runtime.InteropServices.Optional] string b) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingMultipleParametersWithOptionalAttributes()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData]
				    [Xunit.InlineData("abc")]
				    [Xunit.InlineData("abc", "def")]
				    public void TestMethod([System.Runtime.InteropServices.Optional] string a,
				                           [System.Runtime.InteropServices.Optional] string b) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingExplicitArray()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData(new object[] { "abc", 1, null })]
				    public void TestMethod(string a, int b, object c) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingExplicitNamedArray()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData(data: new object[] { "abc", 1, null })]
				    public void TestMethod(string a, int b, object c) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingImplicitArray()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData(new[] { (object)"abc", 1, null })]
				    public void TestMethod(string a, int b, object c) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingImplicitNamedArray()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData(data: new[] { (object)"abc", 1, null })]
				    public void TestMethod(string a, int b, object c) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task EmptyArray()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData(new byte[0])]
				    public void TestMethod(byte[] input) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		// https://github.com/xunit/xunit/issues/3000
		[Fact]
		public async Task DecimalValue()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public sealed class ReproClass {
					[Theory]
					[InlineData({|CS0182:0.1m|})]
					public void ReproMethod(decimal m)
					{ }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class X1009_TooFewValues
	{
		[Fact]
		public async Task IgnoresFact()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Fact]
				    [Xunit.InlineData]
				    public void TestMethod(string a) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("Xunit.InlineData()")]
		[InlineData("Xunit.InlineData")]
		public async Task NoArguments(string attribute)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    [Xunit.Theory]
				    [{{|xUnit1009:{0}|}}]
				    public void TestMethod(int a) {{ }}
				}}
				""", attribute);

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task TooFewArguments()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [{|xUnit1009:Xunit.InlineData(1)|}]
				    public void TestMethod(int a, int b, string c) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task TooFewArguments_WithParams()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [{|xUnit1009:Xunit.InlineData(1)|}]
				    public void TestMethod(int a, int b, params string[] value) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class X1010_IncompatibleValueType
	{
		[Fact]
		public async Task MethodUsingIncompatibleExplicitArrayForParams()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData(21.12, {|xUnit1010:new object[] { }|})]
				    public void VariableArgumentsTest(double d, params int[] sq) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		public class NumericParameter : X1010_IncompatibleValueType
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
			public async Task CompatibleNumericValue_NonNullableType(
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
			public async Task CompatibleNumericValue_NullableType(
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
			public async Task BooleanValue_NumericType(
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
			public async Task CharValue_NumericType(string type)
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
			public async Task EnumValue_NumericType(string type)
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

		public class BooleanParameter : X1010_IncompatibleValueType
		{
			[Theory]
			[MemberData(nameof(BoolValues))]
			public async Task FromBooleanValue_ToNonNullable(string value)
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
			[MemberData(nameof(BoolValues))]
			public async Task FromBooleanValue_ToNullable(string value)
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
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async Task FromIncompatibleValue(string value)
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

		public class CharParameter : X1010_IncompatibleValueType
		{
			[Theory]
			[InlineData("'a'")]
			[MemberData(nameof(IntegerValues))]
			public async Task FromCharOrIntegerValue_ToNonNullable(string value)
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
			[MemberData(nameof(IntegerValues))]
			public async Task FromCharOrIntegerValue_ToNullable(string value)
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
			[MemberData(nameof(FloatingPointValues))]
			[MemberData(nameof(BoolValues))]
			[InlineData("\"abc\"")]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("typeof(string)")]
			public async Task FromIncompatibleValue(string value)
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

		public class EnumParameter : X1010_IncompatibleValueType
		{
			[Fact]
			public async Task FromEnumValue_ToNonNullable()
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
			public async Task FromEnumValue_ToNullable()
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
			[MemberData(nameof(NumericValues))]
			[MemberData(nameof(BoolValues))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async Task FromIncompatibleValue(string value)
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

		public class TypeParameter : X1010_IncompatibleValueType
		{
			[Theory]
			[InlineData("typeof(string)")]
			[InlineData("null")]
			public async Task FromTypeValue(string value)
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
			public async Task FromTypeValue_ToParams(string value)
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
			[MemberData(nameof(NumericValues))]
			[MemberData(nameof(BoolValues))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("System.StringComparison.Ordinal")]
			public async Task FromIncompatibleValue(string value)
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
			[MemberData(nameof(NumericValues))]
			[MemberData(nameof(BoolValues))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("System.StringComparison.Ordinal")]
			public async Task FromIncompatibleValue_ToParams(string value)
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

		public class StringParameter : X1010_IncompatibleValueType
		{
			[Theory]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			public async Task FromStringValue(string value)
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
			[MemberData(nameof(NumericValues))]
			[MemberData(nameof(BoolValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("typeof(string)")]
			public async Task FromIncompatibleValue(string value)
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

		public class InterfaceParameter : X1010_IncompatibleValueType
		{
			[Theory]
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("null")]
			public async Task FromTypeImplementingInterface(string value)
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
			[MemberData(nameof(BoolValues))]
			public async Task FromIncompatibleValue(string value)
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

		public class ObjectParameter : X1010_IncompatibleValueType
		{
			[Theory]
			[MemberData(nameof(BoolValues))]
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			[InlineData("typeof(string)")]
			public async Task FromAnyValue(string value)
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
			[MemberData(nameof(BoolValues))]
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			[InlineData("typeof(string)")]
			public async Task FromAnyValue_ToParams(string value)
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

		public class GenericParameter : X1010_IncompatibleValueType
		{
			[Theory]
			[MemberData(nameof(BoolValues))]
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			[InlineData("typeof(string)")]
			public async Task FromAnyValue_NoConstraint(string value)
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
			[MemberData(nameof(BoolValues))]
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			public async Task FromValueTypeValue_WithStructConstraint(string value)
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
			public async Task FromReferenceTypeValue_WithStructConstraint(string value)
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
			public async Task FromReferenceTypeValue_WithClassConstraint(string value)
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
			[MemberData(nameof(BoolValues))]
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			public async Task FromValueTypeValue_WithClassConstraint(string value)
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
			[MemberData(nameof(NumericValues))]
			public async Task FromCompatibleValue_WithTypeConstraint(string value)
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

			[Theory]
#if NETFRAMEWORK
			[InlineData("'a'")]
#endif
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			[MemberData(nameof(BoolValues))]
			public async Task FromIncompatibleValue_WithTypeConstraint(string value)
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
			public async Task FromIncompatibleArray()
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
			public async Task FromCompatibleArray()
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
		}

		public class DateTimeLikeParameter : X1010_IncompatibleValueType
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
			public async Task NonStringValue(
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
			public async Task StringValue_ToDateTime(string data)
			{
				var source = CreateSourceWithStringConst(data, "System.DateTime");

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(ValidDateTimeStrings))]
			public async Task StringValue_ToDateTimeOffset(string data)
			{
				var source = CreateSourceWithStringConst(data, "System.DateTimeOffset");

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(ValidDateTimeStrings))]
			public async Task StringValue_ToDateTimeOffset_Pre240(string data)
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

		public class GuidParameter : X1010_IncompatibleValueType
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
			[MemberData(nameof(ValueTypedValues))]
			[InlineData("MyConstInt")]
			public async Task NonStringValue(string data)
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
			public async Task StringValue(string inlineData)
			{
				var source = CreateSource(inlineData);

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(ValidGuidStrings))]
			public async Task StringValue_Pre240(string data)
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

		public class UserDefinedConversionOperators : X1010_IncompatibleValueType
		{
			[Fact]
			public async Task SupportsImplicitConversion()
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
					}
					""";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task SupportsExplicitConversion()
			{
				var source = /* lang=c#-test */ """
					using Xunit;

					public class TestClass {
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

				await Verify.VerifyAnalyzer(source);
			}
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
	}

	public class X1011_ExtraValue
	{
		[Fact]
		public async Task IgnoresFact()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Fact]
				    [Xunit.InlineData(1, 2, "abc")]
				    public void TestMethod(int a) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task ExtraArguments()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Theory]
				    [Xunit.InlineData(1, {|#0:2|}, {|#1:"abc"|})]
				    public void TestMethod(int a) { }
				}
				""";
			var expected = new[]
			{
				Verify.Diagnostic("xUnit1011").WithLocation(0).WithArguments("2"),
				Verify.Diagnostic("xUnit1011").WithLocation(1).WithArguments("\"abc\""),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1012_NullShouldNotBeUsedForIncompatibleParameter
	{
		[Fact]
		public async Task IgnoresFact()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Fact]
				    [Xunit.InlineData(null)]
				    public void TestMethod(int a) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("int")]
		[InlineData("params int[]")]
		public async Task SingleNullValue(string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    [Xunit.Theory]
				    [Xunit.InlineData({{|#0:null|}})]
				    public void TestMethod({0} a) {{ }}
				}}
				""", type);
			var expected = Verify.Diagnostic("xUnit1012").WithLocation(0).WithArguments("a", "int");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(ValueTypes))]
		public async Task NonNullableValueTypes(string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    [Xunit.Theory]
				    [Xunit.InlineData(1, {{|#0:null|}}, {{|#1:null|}})]
				    public void TestMethod(int a, {0} b, params {0}[] c) {{ }}
				}}
				""", type);
			var expected = new[]
			{
				Verify.Diagnostic("xUnit1012").WithLocation(0).WithArguments("b", type),
				Verify.Diagnostic("xUnit1012").WithLocation(1).WithArguments("c", type),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(ValueTypes))]
		public async Task NullableValueTypes(string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    [Xunit.Theory]
				    [Xunit.InlineData(1, null)]
				    public void TestMethod(int a, {0}? b) {{ }}
				}}
				""", type);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("object")]
		[InlineData("string")]
		[InlineData("System.Exception")]
		public async Task ReferenceTypes(string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				public class TestClass {{
				    [Xunit.Theory]
				    [Xunit.InlineData(1, null)]
				    public void TestMethod(int a, {0} b) {{ }}
				}}
				""", type);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("object")]
		[InlineData("string")]
		[InlineData("System.Exception")]
		public async Task NonNullableReferenceTypes(string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				#nullable enable

				public class TestClass {{
				    [Xunit.Theory]
				    [Xunit.InlineData(1, {{|#0:null|}})]
				    public void TestMethod(int a, {0} b) {{ }}
				}}
				""", type);
			var expected = Verify.Diagnostic("xUnit1012").WithLocation(0).WithArguments("b", type);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}

		[Theory]
		[InlineData("object")]
		[InlineData("string")]
		[InlineData("System.Exception")]
		public async Task NullableReferenceTypes(string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				#nullable enable

				public class TestClass {{
				    [Xunit.Theory]
				    [Xunit.InlineData(1, null)]
				    public void TestMethod(int a, {0}? b) {{ }}
				}}
				""", type);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Theory]
		[InlineData("1", "object")]
		[InlineData("\"bob\"", "string")]
		public async Task NullableParamsReferenceTypes(
			string param,
			string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				#nullable enable

				public class TestClass {{
				    [Xunit.Theory]
				    [Xunit.InlineData(1, {0}, null, null)]
				    public void TestMethod(int a, params {1}?[] b) {{ }}
				}}
				""", param, type);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Theory]
		[InlineData("1", "object")]
		[InlineData("\"bob\"", "string")]
		public async Task NonNullableParamsReferenceTypes(
			string param,
			string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				#nullable enable

				public class TestClass {{
				    [Xunit.Theory]
				    [Xunit.InlineData(1, {0}, {{|#0:null|}}, {{|#1:null|}})]
				    public void TestMethod(int a, params {1}[] b) {{ }}
				}}
				""", param, type);
			var expected = new[]
			{
				Verify.Diagnostic("xUnit1012").WithLocation(0).WithArguments("b", type),
				Verify.Diagnostic("xUnit1012").WithLocation(1).WithArguments("b", type),
			};

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}

		public static IEnumerable<TheoryDataRow<string>> ValueTypes =
		[
			"bool",
			"int",
			"byte",
			"short",
			"long",
			"decimal",
			"double",
			"float",
			"char",
			"ulong",
			"uint",
			"ushort",
			"sbyte",
			"System.StringComparison",
			"System.Guid",
		];
	}

	internal class Analyzer_v2_Pre240 : InlineDataMustMatchTheoryParameters
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 3, 999));
	}
}
