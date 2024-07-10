using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
		public async Task MethodUsingParamsArgument()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"", ""xyz"")]
    public void TestMethod(params string[] args) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingNullParamsArgument_NonNullable()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(null)]
    public void TestMethod(params string[] args) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingNullParamsArgument_Nullable()
		{
			var source = @"
#nullable enable

public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(null)]
    public void TestMethod(params string[]? args) { }
}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source);
		}

		[Fact]
		public async Task MethodUsingNormalAndParamsArgument()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"", ""xyz"")]
    public void TestMethod(string first, params string[] args) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingNormalAndNullParamsArgument_NonNullable()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"", null)]
    public void TestMethod(string first, params string[] args) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingNormalAndNullParamsArgument_Nullable()
		{
			var source = @"
#nullable enable

public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"", null)]
    public void TestMethod(string first, params string[]? args) { }
}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source);
		}

		[Fact]
		public async Task MethodUsingNormalAndUnusedParamsArgument()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"")]
    public void TestMethod(string first, params string[] args) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingEmptyArrayForParams()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new int[] { })]
    public void VariableArgumentsTest(params int[] sq) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingMixedArgumentsAndEmptyArrayForParams()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(21.12, new int[] { })]
    public void VariableArgumentsTest(double d, params int[] sq) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingNonEmptyArrayForParams()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new int[] { 1, 2, 3 })]
    public void VariableArgumentsTest(params int[] sq) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingMixedArgumentsAndNonEmptyArrayForParams()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(21.12, new int[] { 1, 2, 3 })]
    public void VariableArgumentsTest(double d, params int[] sq) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MethodUsingIncompatibleExplicitArrayForParams()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(21.12, {|xUnit1010:new object[] { }|})]
    public void VariableArgumentsTest(double d, params int[] sq) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingParameters()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"", 1, null)]
    public void TestMethod(string a, int b, object c) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingParametersWithDefaultValues()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"")]
    public void TestMethod(string a, string b = ""default"", string c = null) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingParametersWithDefaultValuesAndParamsArgument()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"")]
    public void TestMethod(string a, string b = ""default"", string c = null, params string[] d) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingParameterWithOptionalAttribute()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(""abc"")]
    public void TestMethod(string a, [System.Runtime.InteropServices.Optional] string b) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingMultipleParametersWithOptionalAttributes()
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

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingExplicitArray()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new object[] { ""abc"", 1, null })]
    public void TestMethod(string a, int b, object c) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingExplicitNamedArray()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(data: new object[] { ""abc"", 1, null })]
    public void TestMethod(string a, int b, object c) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingImplicitArray()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new[] { (object)""abc"", 1, null })]
    public void TestMethod(string a, int b, object c) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task UsingImplicitNamedArray()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(data: new[] { (object)""abc"", 1, null })]
    public void TestMethod(string a, int b, object c) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task EmptyArray()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new byte[0])]
    public void TestMethod(byte[] input) { }
}";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class X1009_TooFewValues
	{
		[Fact]
		public async Task IgnoresFact()
		{
			var source = @"
public class TestClass {
    [Xunit.Fact]
    [Xunit.InlineData]
    public void TestMethod(string a) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("Xunit.InlineData()")]
		[InlineData("Xunit.InlineData")]
		public async Task NoArguments(string attribute)
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

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task TooFewArguments()
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

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task TooFewArguments_WithParams()
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

			await Verify.VerifyAnalyzer(source, expected);
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
			public async Task CompatibleNumericValue_NonNullableType(
				string value,
				string type)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod({type} a) {{ }}
}}";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(NumericValuesAndNumericTypes))]
			public async Task CompatibleNumericValue_NullableType(
				string value,
				string type)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod({type}? a) {{ }}
}}";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(BoolValuesAndNumericTypes))]
			public async Task BooleanValue_NumericType(
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

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Theory]
			[MemberData(nameof(NumericTypes))]
			public async Task CharValue_NumericType(string type)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData('a')]
    public void TestMethod({type} a) {{ }}
}}";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(NumericTypes))]
			public async Task EnumValue_NumericType(string type)
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

				await Verify.VerifyAnalyzer(source, expected);
			}
		}

		public class BooleanParameter : X1010_IncompatibleValueType
		{
			[Theory]
			[MemberData(nameof(BoolValues))]
			public async Task FromBooleanValue_ToNonNullable(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(bool a) {{ }}
}}";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(BoolValues))]
			public async Task FromBooleanValue_ToNullable(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(bool? a) {{ }}
}}";

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
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(char a) {{ }}
}}";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("'a'")]
			[MemberData(nameof(IntegerValues))]
			public async Task FromCharOrIntegerValue_ToNullable(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(char? a) {{ }}
}}";

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

				await Verify.VerifyAnalyzer(source, expected);
			}
		}

		public class EnumParameter : X1010_IncompatibleValueType
		{
			[Fact]
			public async Task FromEnumValue_ToNonNullable()
			{
				var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(System.StringComparison.Ordinal)]
    public void TestMethod(System.StringComparison a) { }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task FromEnumValue_ToNullable()
			{
				var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(System.StringComparison.Ordinal)]
    public void TestMethod(System.StringComparison? a) { }
}";

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
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(System.Type a) {{ }}
}}";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("typeof(string)")]
			[InlineData("null")]
			public async Task FromTypeValue_ToParams(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(params System.Type[] a) {{ }}
}}";

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
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(string a) {{ }}
}}";

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
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(System.IFormattable a) {{ }}
}}";

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
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(object a) {{ }}
}}";

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
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod(params object[] a) {{ }}
}}";

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
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod<T>(T a) {{ }}
}}";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(BoolValues))]
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			public async Task FromValueTypeValue_WithStructConstraint(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod<T>(T a) where T: struct {{ }}
}}";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async Task FromReferenceTypeValue_WithStructConstraint(string value)
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

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Theory]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			[InlineData("null")]
			public async Task FromReferenceTypeValue_WithClassConstraint(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod<T>(T a) where T: class {{ }}
}}";

				await Verify.VerifyAnalyzer(source);
			}

			[Theory]
			[MemberData(nameof(BoolValues))]
			[MemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			public async Task FromValueTypeValue_WithClassConstraint(string value)
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

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Theory]
			[InlineData("null")]
			[InlineData("System.StringComparison.Ordinal")]
			[MemberData(nameof(NumericValues))]
			public async Task FromCompatibleValue_WithTypeConstraint(string value)
			{
				var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({value})]
    public void TestMethod<T>(T a) where T: System.IConvertible, System.IFormattable {{ }}
}}";

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

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Fact]
			public async Task FromIncompatibleArray()
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

				await Verify.VerifyAnalyzer(source, expected);
			}

			[Fact]
			public async Task FromCompatibleArray()
			{
				var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new int[] { 1, 2, 3 })]
    public void TestMethod<T>(T[] a) { }
}";

				await Verify.VerifyAnalyzer(source);
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
			public async Task NonStringValue(
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
				var source = CreateSourceWithStringConst(data, "System.DateTimeOffset");
				var expected =
					Verify_v2_Pre240
						.Diagnostic("xUnit1010")
						.WithSpan(7, 23, 7, 23 + data.Length)
						.WithArguments("parameter", "System.DateTimeOffset");

				await Verify_v2_Pre240.VerifyAnalyzer(source, expected);
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
			public async Task NonStringValue(string data)
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
				var source = CreateSource(data);
				var expected =
					Verify_v2_Pre240
						.Diagnostic("xUnit1010")
						.WithSpan(5, 23, 5, 23 + data.Length)
						.WithArguments("parameter", "System.Guid");

				await Verify_v2_Pre240.VerifyAnalyzer(source, expected);
			}

			static string CreateSource(string data) => $@"
public class TestClass
{{
    [Xunit.Theory]
    [Xunit.InlineData({data})]
    public void TestMethod(System.Guid parameter) {{ }}
}}";
		}

		public class UserDefinedConversionOperators : X1010_IncompatibleValueType
		{
			[Fact]
			public async Task SupportsImplicitConversion()
			{
				var source = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(""abc"")]
    public void ParameterDeclaredImplicitConversion(Implicit i) => Assert.Equal(""abc"", i.Value);

    public class Implicit {
        public string Value { get; set; }
        public static implicit operator Implicit(string value) => new Implicit() { Value = value };
        public static implicit operator string(Implicit i) => i.Value;
    }
}";

				await Verify.VerifyAnalyzer(source);
			}

			[Fact]
			public async Task SupportsExplicitConversion()
			{
				var source = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(""abc"")]
    public void ParameterDeclaredExplicitConversion(Explicit i) => Assert.Equal(""abc"", i.Value);

    public class Explicit {
        public string Value { get; set; }
        public static explicit operator Explicit(string value) => new Explicit() { Value = value };
        public static explicit operator string(Explicit i) => i.Value;
    }
}";

				await Verify.VerifyAnalyzer(source);
			}
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
		public async Task IgnoresFact()
		{
			var source = @"
public class TestClass {
    [Xunit.Fact]
    [Xunit.InlineData(1, 2, ""abc"")]
    public void TestMethod(int a) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task ExtraArguments()
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

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1012_NullShouldNotBeUsedForIncompatibleParameter
	{
		[Fact]
		public async Task IgnoresFact()
		{
			var source = @"
public class TestClass {
    [Xunit.Fact]
    [Xunit.InlineData(null)]
    public void TestMethod(int a) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("int")]
		[InlineData("params int[]")]
		public async Task SingleNullValue(string type)
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

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(ValueTypes))]
		public async Task NonNullableValueTypes(string type)
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

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(ValueTypes))]
		public async Task NullableValueTypes(string type)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(1, null)]
    public void TestMethod(int a, {type}? b) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("object")]
		[InlineData("string")]
		[InlineData("System.Exception")]
		public async Task ReferenceTypes(string type)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(1, null)]
    public void TestMethod(int a, {type} b) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("object")]
		[InlineData("string")]
		[InlineData("System.Exception")]
		public async Task NonNullableReferenceTypes(string type)
		{
			var source = $@"
#nullable enable
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(1, null)]
    public void TestMethod(int a, {type} b) {{ }}
#nullable restore
}}";

			DiagnosticResult[] expected =
			{
				Verify
					.Diagnostic("xUnit1012")
					.WithSpan(5, 26, 5, 30)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("b", type)
			};

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}

		[Theory]
		[InlineData("object")]
		[InlineData("string")]
		[InlineData("System.Exception")]
		public async Task NullableReferenceTypes(string type)
		{
			var source = $@"
#nullable enable
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(1, null)]
    public void TestMethod(int a, {type}? b) {{ }}
#nullable restore
}}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Theory]
		[InlineData("1", "object")]
		[InlineData("\"bob\"", "string")]
		public async Task NullableParamsReferenceTypes(string param, string type)
		{
			var source = $@"
#nullable enable
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(1, {param}, null, null)]
    public void TestMethod(int a, params {type}?[] b) {{ }}
#nullable restore
}}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Theory]
		[InlineData("1", "object")]
		[InlineData("\"bob\"", "string")]
		public async Task NonNullableParamsReferenceTypes(string param, string type)
		{
			var source = $@"
#nullable enable
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(1, {param}, null, null)]
    public void TestMethod(int a, params {type}[] b) {{ }}
#nullable restore
}}";

			DiagnosticResult[] expected =
			{
				Verify
					.Diagnostic("xUnit1012")
					.WithSpan(5, 28 + param.Length, 5, 32 + param.Length)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("b", type),
				Verify
					.Diagnostic("xUnit1012")
					.WithSpan(5, 34 + param.Length, 5, 38 + param.Length)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("b", type),
			};

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
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
