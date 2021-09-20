using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;
using Verify_v2_Pre240 = CSharpVerifier<InlineDataMustMatchTheoryParametersTests.Analyzer_v2_Pre240>;

public class InlineDataMustMatchTheoryParametersTests
{
	public class NonErrors
	{
		[Fact]
		public async void MethodUsingParamsArgument()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"", ""xyz"")]
    public void TestMethod(params string[] args) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Fact]
		public async void MethodUsingNormalAndParamsArgument()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"", ""xyz"")]
    public void TestMethod(string first, params string[] args) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Fact]
		public async void MethodUsingNormalAndUnusedParamsArgument()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"")]
    public void TestMethod(string first, params string[] args) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Fact]
		public async void UsingParameters()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"", 1, null)]
    public void TestMethod(string a, int b, object c) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Fact]
		public async void UsingParametersWithDefaultValues()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"")]
    public void TestMethod(string a, string b = ""default"", string c = null) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Fact]
		public async void UsingParametersWithDefaultValuesAndParamsArgument()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"")]
    public void TestMethod(string a, string b = ""default"", string c = null, params string[] d) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Fact]
		public async void UsingParameterWithOptionalAttribute()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"")]
    public void TestMethod(string a, [System.Runtime.InteropServices.Optional] string b) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Fact]
		public async void UsingMultipleParametersWithOptionalAttributes()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData]
    [Xunit.InlineData(""abc"")]
    [Xunit.InlineData(""abc"", ""def"")]
    public void TestMethod([System.Runtime.InteropServices.Optional] string a,
                           [System.Runtime.InteropServices.Optional] string b) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Fact]
		public async void UsingExplicitArray()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new object[] { ""abc"", 1, null })]
    public void TestMethod(string a, int b, object c) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Fact]
		public async void UsingExplicitNamedArray()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(data: new object[] { ""abc"", 1, null })]
    public void TestMethod(string a, int b, object c) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Fact]
		public async void UsingImplicitArray()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new[] { (object)""abc"", 1, null })]
    public void TestMethod(string a, int b, object c) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Fact]
		public async void UsingImplicitNamedArray()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(data: new[] { (object)""abc"", 1, null })]
    public void TestMethod(string a, int b, object c) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Fact]
		public async void EmptyArray()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new byte[0])]
    public void TestMethod(byte[] input) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}
	}

	public class X1009_TooFewValues
	{
		[Fact]
		public async void IgnoresFact()
		{
			var source = @"
public class TestClass {
    [Xunit.Fact]
    [Xunit.InlineData]
    public void TestMethod(string a) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Theory]
		[InlineData("Xunit.InlineData()")]
		[InlineData("Xunit.InlineData")]
		public async void NoArguments(string attribute)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [{attribute}]
    public void TestMethod(int a) {{ }}
}}";
			var expected =
				Verify
					.Diagnostic("xUnit1009")
					.WithSpan(4, 6, 4, 6 + attribute.Length)
					.WithSeverity(DiagnosticSeverity.Error);

			await Verify.VerifyAnalyzerAsyncV2(source, expected);
		}

		[Fact]
		public async void TooFewArguments()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(1)]
    public void TestMethod(int a, int b, string c) { }
}";
			var expected =
				Verify
					.Diagnostic("xUnit1009")
					.WithSpan(4, 6, 4, 25)
					.WithSeverity(DiagnosticSeverity.Error);

			await Verify.VerifyAnalyzerAsyncV2(source, expected);
		}

		[Fact]
		public async void TooFewArguments_WithParams()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(1)]
    public void TestMethod(int a, int b, params string[] value) { }
}";
			var expected =
				Verify
					.Diagnostic("xUnit1009")
					.WithSpan(4, 6, 4, 25)
					.WithSeverity(DiagnosticSeverity.Error);

			await Verify.VerifyAnalyzerAsyncV2(source, expected);
		}
	}

	public class X1010_IncompatibleValueType
	{
		public class NumericParameter : X1010_IncompatibleValueType
		{
			public static readonly TheoryData<string> NumericTypes = new()
			{
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
			};

			public static IEnumerable<object[]> NumericValuesAndNumericTypes =>
				from value in NumericValues()
				from type in NumericTypes
				select new[] { value[0], type[0] };

			public static IEnumerable<object[]> BoolValuesAndNumericTypes =>
				from value in BoolValues
				from type in NumericTypes
				select new[] { value[0], type[0] };

			[Theory]
			[MemberData(nameof(NumericValuesAndNumericTypes))]
			public async void CompatibleNumericValue_NonNullableType(
				string value,
				string type)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod({type} a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(NumericValuesAndNumericTypes))]
			public async void CompatibleNumericValue_NullableType(
				string value,
				string type)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod({type}? a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(BoolValuesAndNumericTypes))]
			public async void BooleanValue_NumericType(
				string value,
				string type)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod({type} a) {{ }}
}}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(4, 23, 4, 23 + value.Length)
						.WithArguments("a", type);

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}

			[Theory]
			[MemberData(nameof(NumericTypes))]
			public async void CharValue_NumericType(string type)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData('a')]
    public void TestMethod({type} a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(NumericTypes))]
			public async void EnumValue_NumericType(string type)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(System.StringComparison.InvariantCulture)]
    public void TestMethod({type} a) {{ }}
}}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(4, 23, 4, 63)
						.WithArguments("a", type);

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}
		}

		public class BooleanParameter : X1010_IncompatibleValueType
		{
			[Theory]
			[MemberData(nameof(BoolValues))]
			public async void FromBooleanValue_ToNonNullable(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(bool a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(BoolValues))]
			public async void FromBooleanValue_ToNullable(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(bool? a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async void FromIncompatibleValue(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(bool a) {{ }}
}}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(4, 23, 4, 23 + value.Length)
						.WithArguments("a", "bool");

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}
		}

		public class CharParameter : X1010_IncompatibleValueType
		{
			[Theory]
			[InlineData("'a'")]
			[MemberData(nameof(IntegerValues))]
			public async void FromCharOrIntegerValue_ToNonNullable(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(char a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[InlineData("'a'")]
			[MemberData(nameof(IntegerValues))]
			public async void FromCharOrIntegerValue_ToNullable(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(char? a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(FloatingPointValues))]
			[MemberData(nameof(BoolValues))]
			[InlineData("\"abc\"")]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("typeof(string)")]
			public async void FromIncompatibleValue(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(char a) {{ }}
}}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(4, 23, 4, 23 + value.Length)
						.WithArguments("a", "char");

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}
		}

		public class EnumParameter : X1010_IncompatibleValueType
		{
			[Fact]
			public async void FromEnumValue_ToNonNullable()
			{
				var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(System.StringComparison.Ordinal)]
    public void TestMethod(System.StringComparison a) { }
}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Fact]
			public async void FromEnumValue_ToNullable()
			{
				var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(System.StringComparison.Ordinal)]
    public void TestMethod(System.StringComparison? a) { }
}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(NumericValues))]
			[MemberData(nameof(BoolValues))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async void FromIncompatibleValue(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(System.StringComparison a) {{ }}
}}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(4, 23, 4, 23 + value.Length)
						.WithArguments("a", "System.StringComparison");

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}
		}

		public class TypeParameter : X1010_IncompatibleValueType
		{
			[Theory]
			[InlineData("typeof(string)")]
			[InlineData("null")]
			public async void FromTypeValue(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(System.Type a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[InlineData("typeof(string)")]
			[InlineData("null")]
			public async void FromTypeValue_ToParams(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(params System.Type[] a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(NumericValues))]
			[MemberData(nameof(BoolValues))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("System.StringComparison.Ordinal")]
			public async void FromIncompatibleValue(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(System.Type a) {{ }}
}}";

				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(4, 23, 4, 23 + value.Length)
						.WithArguments("a", "System.Type");

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}

			[Theory]
			[MemberData(nameof(NumericValues))]
			[MemberData(nameof(BoolValues))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("System.StringComparison.Ordinal")]
			public async void FromIncompatibleValue_ToParams(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(params System.Type[] a) {{ }}
}}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(4, 23, 4, 23 + value.Length)
						.WithArguments("a", "System.Type");

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}
		}

		public class StringParameter : X1010_IncompatibleValueType
		{
			[Theory]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			public async void FromStringValue(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(string a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(NumericValues))]
			[MemberData(nameof(BoolValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("typeof(string)")]
			public async void FromIncompatibleValue(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(string a) {{ }}
}}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(4, 23, 4, 23 + value.Length)
						.WithArguments("a", "string");

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}
		}

		public class InterfaceParameter : X1010_IncompatibleValueType
		{
			[Theory]
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("null")]
			public async void FromTypeImplementingInterface(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(System.IFormattable a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(BoolValues))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async void FromIncompatibleValue(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(System.IFormattable a) {{ }}
}}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(4, 23, 4, 23 + value.Length)
						.WithArguments("a", "System.IFormattable");

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
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
			public async void FromAnyValue(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(object a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(BoolValues))]
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			[InlineData("typeof(string)")]
			public async void FromAnyValue_ToParams(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(params object[] a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
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
			public async void FromAnyValue_NoConstraint(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod<T>(T a) {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(BoolValues))]
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			public async void FromValueTypeValue_WithStructConstraint(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod<T>(T a) where T: struct {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async void FromReferenceTypeValue_WithStructConstraint(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod<T>(T a) where T: struct {{ }}
}}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(4, 23, 4, 23 + value.Length)
						.WithArguments("a", "T");

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}

			[Theory]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			[InlineData("null")]
			public async void FromReferenceTypeValue_WithClassConstraint(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod<T>(T a) where T: class {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(BoolValues))]
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			public async void FromValueTypeValue_WithClassConstraint(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod<T>(T a) where T: class {{ }}
}}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(4, 23, 4, 23 + value.Length)
						.WithArguments("a", "T");

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}

			[Theory]
			[InlineData("null")]
			[InlineData("System.StringComparison.Ordinal")]
			[MemberData(nameof(NumericValues))]
			public async void FromCompatibleValue_WithTypeConstraint(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod<T>(T a) where T: System.IConvertible, System.IFormattable {{ }}
}}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[InlineData("typeof(string)")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[MemberData(nameof(BoolValues))]
			public async void FromIncompatibleValue_WithTypeConstraint(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod<T>(T a) where T: System.IConvertible, System.IFormattable {{ }}
}}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(4, 23, 4, 23 + value.Length)
						.WithArguments("a", "T");

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}

			[Fact]
			public async void FromIncompatibleArray()
			{
				var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new int[] { 1, 2, 3 })]
    public void TestMethod<T>(T a) where T: System.IConvertible, System.IFormattable { }
}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(4, 35, 4, 36)
						.WithArguments("a", "T");

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}

			[Fact]
			public async void FromCompatibleArray()
			{
				var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new int[] { 1, 2, 3 })]
    public void TestMethod<T>(T[] a) { }
}";

				await Verify.VerifyAnalyzerAsyncV2(source);
			}
		}

		public class DateTimeLikeParameter : X1010_IncompatibleValueType
		{
			public static readonly TheoryData<string> ValidDateTimeStrings = new()
			{
				"\"\"",
				"\"2018-01-02\"",
				"\"2018-01-02 12:34\"",
				"\"obviously-rubbish-datetime-value\"",
				"MyConstString"
			};

			public static IEnumerable<object[]> ValueTypedArgumentsCombinedWithDateTimeLikeTypes =>
				ValueTypedValues().SelectMany(v =>
					new string[] { "System.DateTime", "System.DateTimeOffset" }
						.Select(dateTimeLikeType => new[] { v[0], dateTimeLikeType })
				);

			[Theory]
			[MemberData(nameof(ValueTypedArgumentsCombinedWithDateTimeLikeTypes))]
			[InlineData("MyConstInt", "System.DateTime")]
			[InlineData("MyConstInt", "System.DateTimeOffset")]
			public async void NonStringValue(
				string data,
				string parameterType)
			{
				var source = $@"
public class TestClass
{{
    const int MyConstInt = 1;

    [Xunit.Theory]
    [Xunit.InlineData({data})]
    public void TestMethod({parameterType} parameter) {{ }}
}}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(7, 23, 7, 23 + data.Length)
						.WithArguments("parameter", parameterType);

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}

			[Theory]
			[MemberData(nameof(ValidDateTimeStrings))]
			public async void StringValue_ToDateTime(string data)
			{
				var source = CreateSourceWithStringConst(data, "System.DateTime");

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(ValidDateTimeStrings))]
			public async void StringValue_ToDateTimeOffset(string data)
			{
				var source = CreateSourceWithStringConst(data, "System.DateTimeOffset");

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(ValidDateTimeStrings))]
			public async void StringValue_ToDateTimeOffset_Pre240(string data)
			{
				var source = CreateSourceWithStringConst(data, "System.DateTimeOffset");
				var expected =
					Verify_v2_Pre240
						.Diagnostic("xUnit1010")
						.WithSpan(7, 23, 7, 23 + data.Length)
						.WithArguments("parameter", "System.DateTimeOffset");

				await Verify_v2_Pre240.VerifyAnalyzerAsyncV2(source, expected);
			}

			static string CreateSourceWithStringConst(
				string data,
				string parameterType) => $@"
public class TestClass
{{
    const string MyConstString = ""some string"";

    [Xunit.Theory]
    [Xunit.InlineData({data})]
    public void TestMethod({parameterType} parameter) {{ }}
}}";
		}

		public class GuidParameter : X1010_IncompatibleValueType
		{
			public static TheoryData<string> ValidGuidStrings = new()
			{
				"\"\"",
				"\"{5B21E154-15EB-4B1E-BC30-127E8A41ECA1}\"",
				"\"4EBCD32C-A2B8-4600-9E72-3873347E285C\"",
				"\"39A3B4C85FEF43A988EB4BB4AC4D4103\"",
				"\"obviously-rubbish-guid-value\""
			};

			[Theory]
			[MemberData(nameof(ValueTypedValues))]
			[InlineData("MyConstInt")]
			public async void NonStringValue(string data)
			{
				var source = $@"
public class TestClass
{{
    private const int MyConstInt = 1;

    [Xunit.Theory]
    [Xunit.InlineData({data})]
    public void TestMethod(System.Guid parameter) {{ }}
}}";
				var expected =
					Verify
						.Diagnostic("xUnit1010")
						.WithSpan(7, 23, 7, 23 + data.Length)
						.WithArguments("parameter", "System.Guid");

				await Verify.VerifyAnalyzerAsyncV2(source, expected);
			}

			[Theory]
			[MemberData(nameof(ValidGuidStrings))]
			public async void StringValue(string inlineData)
			{
				var source = CreateSource(inlineData);

				await Verify.VerifyAnalyzerAsyncV2(source);
			}

			[Theory]
			[MemberData(nameof(ValidGuidStrings))]
			public async void StringValue_Pre240(string data)
			{
				var source = CreateSource(data);
				var expected =
					Verify_v2_Pre240
						.Diagnostic("xUnit1010")
						.WithSpan(5, 23, 5, 23 + data.Length)
						.WithArguments("parameter", "System.Guid");

				await Verify_v2_Pre240.VerifyAnalyzerAsyncV2(source, expected);
			}

			static string CreateSource(string data) => $@"
public class TestClass
{{
    [Xunit.Theory]
    [Xunit.InlineData({data})]
    public void TestMethod(System.Guid parameter) {{ }}
}}";
		}

		// Note: decimal literal 42M is not valid as an attribute argument

		public static TheoryData<string> BoolValues = new()
		{
			"true",
			"false"
		};

		public static TheoryData<string> FloatingPointValues = new()
		{
			"42f",
			"42d"
		};

		public static TheoryData<string> IntegerValues = new()
		{
			"42",
			"42L",
			"42u",
			"42ul",
			"(short)42",
			"(byte)42",
			"(ushort)42",
			"(sbyte)42"
		};

		public static IEnumerable<object[]> NumericValues()
		{
			foreach (var integerValue in IntegerValues)
				yield return integerValue;
			foreach (var floatingPointValue in FloatingPointValues)
				yield return floatingPointValue;
		}

		public static IEnumerable<object[]> ValueTypedValues()
		{
			foreach (var numericValue in NumericValues())
				yield return numericValue;
			foreach (var boolValue in BoolValues)
				yield return boolValue;

			yield return new[] { "typeof(int)" };
		}
	}

	public class X1011_ExtraValue
	{
		[Fact]
		public async void IgnoresFact()
		{
			var source = @"
public class TestClass {
    [Xunit.Fact]
    [Xunit.InlineData(1, 2, ""abc"")]
    public void TestMethod(int a) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Fact]
		public async void ExtraArguments()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(1, 2, ""abc"")]
    public void TestMethod(int a) { }
}";
			DiagnosticResult[] expected =
			{
					Verify
						.Diagnostic("xUnit1011")
						.WithSpan(4, 26, 4, 27)
						.WithSeverity(DiagnosticSeverity.Error)
						.WithArguments("2"),
					Verify
						.Diagnostic("xUnit1011")
						.WithSpan(4, 29, 4, 34)
						.WithSeverity(DiagnosticSeverity.Error)
						.WithArguments("\"abc\""),
				};

			await Verify.VerifyAnalyzerAsyncV2(source, expected);
		}
	}

	public class X1012_NullShouldNotBeUsedForIncompatibleParameter
	{
		[Fact]
		public async void IgnoresFact()
		{
			var source = @"
public class TestClass {
    [Xunit.Fact]
    [Xunit.InlineData(null)]
    public void TestMethod(int a) { }
}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Theory]
		[InlineData("int")]
		[InlineData("params int[]")]
		public async void SingleNullValue(string type)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(null)]
    public void TestMethod({type} a) {{ }}
}}";
			var expected =
				Verify
					.Diagnostic("xUnit1012")
					.WithSpan(4, 23, 4, 27)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("a", "int");

			await Verify.VerifyAnalyzerAsyncV2(source, expected);
		}

		[Theory]
		[MemberData(nameof(ValueTypes))]
		public async void NonNullableValueTypes(string type)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(1, null, null)]
    public void TestMethod(int a, {type} b, params {type}[] c) {{ }}
}}";
			DiagnosticResult[] expected =
			{
					Verify
						.Diagnostic("xUnit1012")
						.WithSpan(4, 26, 4, 30)
						.WithSeverity(DiagnosticSeverity.Warning)
						.WithArguments("b", type),
					Verify
						.Diagnostic("xUnit1012")
						.WithSpan(4, 32, 4, 36)
						.WithSeverity(DiagnosticSeverity.Warning)
						.WithArguments("c", type),
				};

			await Verify.VerifyAnalyzerAsyncV2(source, expected);
		}

		[Theory]
		[MemberData(nameof(ValueTypes))]
		public async void NullableValueTypes(string type)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(1, null)]
    public void TestMethod(int a, {type}? b) {{ }}
}}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		[Theory]
		[InlineData("object")]
		[InlineData("string")]
		[InlineData("System.Exception")]
		public async void ReferenceTypes(string type)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(1, null)]
    public void TestMethod(int a, {type} b) {{ }}
}}";

			await Verify.VerifyAnalyzerAsyncV2(source);
		}

		public static TheoryData<string> ValueTypes = new()
		{
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
		};
	}

	internal class Analyzer_v2_Pre240 : InlineDataMustMatchTheoryParameters
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2Core(compilation, new Version(2, 3, 999));
	}
}
