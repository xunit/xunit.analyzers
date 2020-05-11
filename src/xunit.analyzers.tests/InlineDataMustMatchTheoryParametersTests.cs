using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;
using Verify_2_3_1 = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParametersTests.Analyzer_2_3_1>;
using Verify_2_4 = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParametersTests.Analyzer_2_4_0>;
using Verify_2_5 = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParametersTests.Analyzer_2_5_0>;

namespace Xunit.Analyzers
{
	public abstract class InlineDataMustMatchTheoryParametersTests
	{
		public class ForFactMethod : InlineDataMustMatchTheoryParametersTests
		{
			[Fact]
			public async void DoesNotFindError_WhenNoDataAttributes()
			{
				var source = "public class TestClass { [Xunit.Fact] public void TestMethod() { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindError_WithAttribute()
			{
				var source = "public class TestClass { [Xunit.Fact, Xunit.InlineData] public void TestMethod(string a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}
		}

		public class ForTheoryWithArgumentMatch : InlineDataMustMatchTheoryParametersTests
		{
			[Fact]
			public async void DoesNotFindErrorFor_MethodUsingParamsArgument()
			{
				var source =
					"public class TestClass {" +
					"   [Xunit.Theory, Xunit.InlineData(\"abc\", \"xyz\")]" +
					"   public void TestMethod(params string[] args) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindErrorFor_MethodUsingNormalAndParamsArgument()
			{
				var source =
					"public class TestClass {" +
					"   [Xunit.Theory, Xunit.InlineData(\"abc\", \"xyz\")]" +
					"   public void TestMethod(string first, params string[] theRest) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindErrorFor_MethodUsingNormalAndUnusedParamsArgument()
			{
				var source =
					"public class TestClass {" +
					"   [Xunit.Theory, Xunit.InlineData(\"abc\")]" +
					"   public void TestMethod(string first, params string[] theRest) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindErrorFor_UsingParameters()
			{
				var source =
					"public class TestClass {" +
					"   [Xunit.Theory, Xunit.InlineData(\"abc\", 1, null)]" +
					"   public void TestMethod(string a, int b, object c) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindErrorFor_UsingParametersWithDefaultValues()
			{
				var source =
					"public class TestClass {" +
					"   [Xunit.Theory, Xunit.InlineData(\"abc\")]" +
					"   public void TestMethod(string a, string b = \"default\", string c = null) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindErrorFor_UsingParametersWithDefaultValuesAndParamsArgument()
			{
				var source =
					"public class TestClass {" +
					"   [Xunit.Theory, Xunit.InlineData(\"abc\")]" +
					"   public void TestMethod(string a, string b = \"default\", string c = null, params string[] d) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindErrorFor_UsingParameterWithOptionalAttribute()
			{
				var source =
					"public class TestClass {" +
					"   [Xunit.Theory, Xunit.InlineData(\"abc\")]" +
					"   public void TestMethod(string a, [System.Runtime.InteropServices.Optional] string b) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindErrorFor_UsingMultipleParametersWithOptionalAttributes()
			{
				var source =
					"public class TestClass {" +
					"   [Xunit.Theory]" +
					"   [Xunit.InlineData]" +
					"   [Xunit.InlineData(\"abc\")]" +
					"   [Xunit.InlineData(\"abc\", \"def\")]" +
					"   public void TestMethod([System.Runtime.InteropServices.Optional] string a," +
					"                          [System.Runtime.InteropServices.Optional] string b) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindError_UsingExplicitArray()
			{
				var source =
					"public class TestClass {" +
					"   [Xunit.Theory, Xunit.InlineData(new object[] {\"abc\", 1, null})]" +
					"   public void TestMethod(string a, int b, object c) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindError_UsingExplicitNamedArray()
			{
				var source =
					"public class TestClass {" +
					"   [Xunit.Theory, Xunit.InlineData(data: new object[] {\"abc\", 1, null})]" +
					"   public void TestMethod(string a, int b, object c) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindError_UsingImplicitArray()
			{
				var source =
					"public class TestClass {" +
					"   [Xunit.Theory, Xunit.InlineData(new [] {(object)\"abc\", 1, null})]" +
					"   public void TestMethod(string a, int b, object c) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindError_UsingImplicitNamedArray()
			{
				var source =
					"public class TestClass {" +
					"   [Xunit.Theory, Xunit.InlineData(data: new [] {(object)\"abc\", 1, null})]" +
					"   public void TestMethod(string a, int b, object c) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindError_EmptyArray()
			{
				var source =
					"public class TestClass {" +
					"   [Xunit.Theory, Xunit.InlineData(new byte[0])]" +
					"   public void TestMethod(byte[] input) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}
		}

		public class ForAttributeWithTooFewArguments : InlineDataMustMatchTheoryParametersTests
		{
			[Fact]
			public async void FindsError()
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(1)] public void TestMethod(int a, int b, string c) { } }";

				var expected = Verify.Diagnostic("xUnit1009").WithSpan(1, 41, 1, 60).WithSeverity(DiagnosticSeverity.Error);
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[InlineData("Xunit.InlineData()")]
			[InlineData("Xunit.InlineData")]
			public async void FindsError_ForAttributeWithNoArguments(string attribute)
			{
				var source =
					"public class TestClass {" +
					$"  [Xunit.Theory, {attribute}]" +
					"  public void TestMethod(int a) { }" +
					"}";

				var expected = Verify.Diagnostic("xUnit1009").WithSpan(1, 42, 1, 42 + attribute.Length).WithSeverity(DiagnosticSeverity.Error);
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsError_UsingParams()
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(1)] public void TestMethod(int a, int b, params string[] value) { } }";

				var expected = Verify.Diagnostic("xUnit1009").WithSpan(1, 41, 1, 60).WithSeverity(DiagnosticSeverity.Error);
				await Verify.VerifyAnalyzerAsync(source, expected);
			}
		}

		public class ForAttributeWithTooManyArguments : InlineDataMustMatchTheoryParametersTests
		{
			[Fact]
			public async void FindsError()
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, 2, \"abc\")] public void TestMethod(int a) { } }";

				DiagnosticResult[] expected =
				{
					Verify.Diagnostic("xUnit1011").WithSpan(1, 61, 1, 62).WithSeverity(DiagnosticSeverity.Error).WithArguments("2"),
					Verify.Diagnostic("xUnit1011").WithSpan(1, 64, 1, 69).WithSeverity(DiagnosticSeverity.Error).WithArguments("\"abc\""),
				};
				await Verify.VerifyAnalyzerAsync(source, expected);
			}
		}

		public class ForAttributeNullValue : InlineDataMustMatchTheoryParametersTests
		{
			public static TheoryData<string> ValueTypes { get; }
				= new TheoryData<string>
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

			[Theory]
			[InlineData("int")]
			[InlineData("params int[]")]
			public async void FindsWarning_ForSingleNullValue(string type)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(null)] public void TestMethod(" + type + " a) { } }";

				var expected = Verify.Diagnostic("xUnit1012").WithSpan(1, 58, 1, 62).WithSeverity(DiagnosticSeverity.Warning).WithArguments("a", "int");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[MemberData(nameof(ValueTypes))]
			public async void FindsWarning_ForValueTypeParameter(string type)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, null, null)] public void TestMethod(int a, " + type + " b, params " + type + "[] c) { } }";

				DiagnosticResult[] expected =
				{
					Verify.Diagnostic("xUnit1012").WithSpan(1, 61, 1, 65).WithSeverity(DiagnosticSeverity.Warning).WithArguments("b", type),
					Verify.Diagnostic("xUnit1012").WithSpan(1, 67, 1, 71).WithSeverity(DiagnosticSeverity.Warning).WithArguments("c", type),
				};
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[MemberData(nameof(ValueTypes))]
			public async void DoesNotFindWarning_ForNullableValueType(string type)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, null)] public void TestMethod(int a, " + type + "? b) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[InlineData("object")]
			[InlineData("string")]
			[InlineData("System.Exception")]
			public async void DoesNotFindWarning_ForReferenceParameterType(string type)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, null)] public void TestMethod(int a, " + type + " b) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}
		}

		public class ForConversionToNumericValue : InlineDataMustMatchTheoryParametersTests
		{
			public static readonly IEnumerable<Tuple<string>> NumericTypes
				= new[] { "int", "long", "short", "byte", "float", "double", "decimal", "uint", "ulong", "ushort", "sbyte", }.Select(t => Tuple.Create(t));

			public static IEnumerable<Tuple<string, string>> NumericValuesAndNumericTypes { get; }
				= from value in NumericValues
				  from type in NumericTypes
				  select Tuple.Create(value.Item1, type.Item1);

			public static IEnumerable<Tuple<string, string>> BoolValuesAndNumericTypes { get; }
				= from value in BoolValues
				  from type in NumericTypes
				  select Tuple.Create(value.Item1, type.Item1);

			[Theory]
			[TupleMemberData(nameof(NumericValuesAndNumericTypes))]
			public async void DoesNotFindError_FromAnyOtherNumericType(string value, string type)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(" + type + " a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(NumericValuesAndNumericTypes))]
			public async void DoesNotFindError_FromAnyOtherNumericType_ToNullable(string value, string type)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(" + type + "? a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(BoolValuesAndNumericTypes))]
			public async void FindsError_ForBoolArgument(string value, string type)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(" + type + " a) { } }";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(1, 58, 1, 58 + value.Length).WithArguments("a", type);
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[TupleMemberData(nameof(NumericTypes))]
			public async void DoesNotFindError_ForCharArgument(string type)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData('a')] public void TestMethod(" + type + " a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(NumericTypes))]
			public async void FindsError_ForEnumArgument(string type)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(System.StringComparison.InvariantCulture)] public void TestMethod(" + type + " a) { } }";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(1, 58, 1, 98).WithArguments("a", type);
				await Verify.VerifyAnalyzerAsync(source, expected);
			}
		}

		public class ForConversionToBoolValue : InlineDataMustMatchTheoryParametersTests
		{
			[Theory]
			[TupleMemberData(nameof(BoolValues))]
			public async void DoesNotFindError_FromBoolType(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(bool a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(BoolValues))]
			public async void DoesNotFindError_FromBoolType_ToNullable(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(bool? a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async void FindsError_ForOtherArguments(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(bool a) { } }";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(1, 58, 1, 58 + value.Length).WithArguments("a", "bool");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}
		}

		public class ForConversionToCharValue : InlineDataMustMatchTheoryParametersTests
		{
			[Theory]
			[InlineData("'a'")]
			[TupleMemberData(nameof(IntegerValues))]
			public async void DoesNotFindError_FromCharOrIntegerType(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(char a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[InlineData("'a'")]
			[TupleMemberData(nameof(IntegerValues))]
			public async void DoesNotFindError_FromCharType_ToNullable(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(char? a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(FloatingPointValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[TupleMemberData(nameof(BoolValues))]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async void FindsError_ForOtherArguments(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(char a) { } }";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(1, 58, 1, 58 + value.Length).WithArguments("a", "char");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}
		}

		public class ForConversionToEnum : InlineDataMustMatchTheoryParametersTests
		{
			[Fact]
			public async void DoesNotFindError_FromEnum()
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(System.StringComparison.Ordinal)] public void TestMethod(System.StringComparison a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindError_FromEnum_ToNullable()
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(System.StringComparison.Ordinal)] public void TestMethod(System.StringComparison? a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(NumericValues))]
			[TupleMemberData(nameof(BoolValues))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async void FindsError_ForOtherArguments(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.StringComparison a) { } }";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(1, 58, 1, 58 + value.Length).WithArguments("a", "System.StringComparison");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}
		}

		public class ForConversionToType : InlineDataMustMatchTheoryParametersTests
		{
			[Theory]
			[InlineData("typeof(string)")]
			[InlineData("null")]
			public async void DoesNotFindError_FromType(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.Type a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[InlineData("typeof(string)")]
			[InlineData("null")]
			public async void DoesNotFindError_FromType_UsingParams(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(params System.Type[] a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(NumericValues))]
			[TupleMemberData(nameof(BoolValues))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("System.StringComparison.Ordinal")]
			public async void FindsError_ForOtherArguments(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.Type a) { } }";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(1, 58, 1, 58 + value.Length).WithArguments("a", "System.Type");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[TupleMemberData(nameof(NumericValues))]
			[TupleMemberData(nameof(BoolValues))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("System.StringComparison.Ordinal")]
			public async void FindsError_ForOtherArguments_UsingParams(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(params System.Type[] a) { } }";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(1, 58, 1, 58 + value.Length).WithArguments("a", "System.Type");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}
		}

		public class ForConversionToString : InlineDataMustMatchTheoryParametersTests
		{
			[Theory]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			public async void DoesNotFindError_FromBoolType(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(string a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(NumericValues))]
			[TupleMemberData(nameof(BoolValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("typeof(string)")]
			public async void FindsError_ForOtherArguments(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(string a) { } }";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(1, 58, 1, 58 + value.Length).WithArguments("a", "string");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}
		}

		public class ForConversionToInterface : InlineDataMustMatchTheoryParametersTests
		{
			[Theory]
			[TupleMemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("null")]
			public async void DoesNotFindError_FromTypesImplementingInterface(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.IFormattable a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(BoolValues))]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async void FindsError_ForOtherArguments(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.IFormattable a) { } }";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(1, 58, 1, 58 + value.Length).WithArguments("a", "System.IFormattable");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}
		}

		public class ForConversionToObject : InlineDataMustMatchTheoryParametersTests
		{
			[Theory]
			[TupleMemberData(nameof(BoolValues))]
			[TupleMemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			[InlineData("typeof(string)")]
			public async void DoesNotFindError_FromAnyValue(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(object a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(BoolValues))]
			[TupleMemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			[InlineData("typeof(string)")]
			public async void DoesNotFindError_FromAnyValues_UsingParams(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(params object[] a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}
		}

		public class ForConversionToGeneric : InlineDataMustMatchTheoryParametersTests
		{
			[Theory]
			[TupleMemberData(nameof(BoolValues))]
			[TupleMemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[InlineData("null")]
			[InlineData("typeof(string)")]
			public async void DoesNotFindError_FromAnyValue_WithNoConstraint(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(BoolValues))]
			[TupleMemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			public async void DoesNotFindError_FromAnyValueType_WithStructConstraint(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) where T: struct { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			public async void FindsError_FromAnyReferenceType_WithStructConstraint(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) where T: struct { } }";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(1, 58, 1, 58 + value.Length).WithArguments("a", "T");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[InlineData("\"abc\"")]
			[InlineData("typeof(string)")]
			[InlineData("null")]
			public async void DoesNotFindError_FromAnyReferenceType_WithClassConstraint(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) where T: class { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(BoolValues))]
			[TupleMemberData(nameof(NumericValues))]
			[InlineData("System.StringComparison.Ordinal")]
			[InlineData("'a'")]
			public async void FindsError_FromAnyValueType_WithClassConstraint(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) where T: class { } }";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(1, 58, 1, 58 + value.Length).WithArguments("a", "T");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[InlineData("null")]
			[InlineData("System.StringComparison.Ordinal")]
			[TupleMemberData(nameof(NumericValues))]
			public async void DoesNotFindError_FromAnyMatchingType_WithTypeConstraint(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) where T: System.IConvertible, System.IFormattable { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[InlineData("typeof(string)")]
			[InlineData("'a'")]
			[InlineData("\"abc\"")]
			[TupleMemberData(nameof(BoolValues))]
			public async void FindsError_FromNonMatchingType_WithTypeConstraint(string value)
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) where T: System.IConvertible, System.IFormattable { } }";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(1, 58, 1, 58 + value.Length).WithArguments("a", "T");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsError_FromNonMatchingArrayType_WithTypeConstraint()
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(new int[] { 1, 2, 3 })] public void TestMethod<T>(T a) where T: System.IConvertible, System.IFormattable { } }";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(1, 70, 1, 71).WithArguments("a", "T");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void DoesNotFindError_FromMatchingArrayType()
			{
				var source = "public class TestClass { [Xunit.Theory, Xunit.InlineData(new int[] { 1, 3, 3 })] public void TestMethod<T>(T[] a) { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}
		}

		public class ForConversionToDateTimeAndDateTimeOffset : InlineDataMustMatchTheoryParametersTests
		{
			public static readonly string[] AcceptableDateTimeArguments =
			{
				"\"\"",
				"\"2018-01-02\"",
				"\"2018-01-02 12:34\"",
				"\"obviously-rubbish-datetime-value\"",
				"MyConstString"
			};

			// combinations of (value type value, date time like types)
			public static IEnumerable<Tuple<string, string>> ValueTypedArgumentsCombinedWithDateTimeLikeTypes { get; }
				= ValueTypedValues.SelectMany(v =>
					new string[] { "System.DateTime", "System.DateTimeOffset" }.Select(
						dateTimeLikeType => Tuple.Create(v.Item1, dateTimeLikeType)));

			public static IEnumerable<Tuple<string, string>> DateTimeValueStringsCombinedWithDateTimeType { get; }
				= AcceptableDateTimeArguments.Select(v => Tuple.Create(v, "System.DateTime"));

			public static IEnumerable<Tuple<string, string>> DateTimeValueStringsCombinedWithDateTimeOffsetType { get; }
				= AcceptableDateTimeArguments.Select(v => Tuple.Create(v, "System.DateTimeOffset"));

			[Theory]
			[TupleMemberData(nameof(ValueTypedArgumentsCombinedWithDateTimeLikeTypes))]
			[InlineData("MyConstInt", "System.DateTime")]
			[InlineData("MyConstInt", "System.DateTimeOffset")]
			public async void FindsError_FromNonString(string inlineData, string parameterType)
			{
				var source = @"
public class TestClass
{
    private const int MyConstInt = 1;

    [Xunit.Theory, Xunit.InlineData(" + inlineData + @")]
    public void TestMethod(" + parameterType + @" parameter)
    {
    }
}";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(6, 37, 6, 37 + inlineData.Length).WithArguments("parameter", parameterType);
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[TupleMemberData(nameof(DateTimeValueStringsCombinedWithDateTimeType))]
			public async void DoesNotFindError_ForDateTime_FromString(string inlineData, string parameterType)
			{
				var source = CreateSourceWithStringConst(inlineData, parameterType);

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[TupleMemberData(nameof(DateTimeValueStringsCombinedWithDateTimeOffsetType))]
			public async void FindsError_ForDateTimeOffsetAndAnalyzerLessThan_2_4_0v_FromString(string inlineData, string parameterType)
			{
				var source = CreateSourceWithStringConst(inlineData, parameterType);

				var expected = Verify_2_3_1.Diagnostic("xUnit1010").WithSpan(6, 37, 6, 37 + inlineData.Length).WithArguments("parameter", parameterType);
				await Verify_2_3_1.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[TupleMemberData(nameof(DateTimeValueStringsCombinedWithDateTimeOffsetType))]
			public async void DoesNotFindError_ForDateTimeOffsetAndForAnalyzerGreaterThanEqual_2_4_0v_FromString(string inlineData, string parameterType)
			{
				var source = CreateSourceWithStringConst(inlineData, parameterType);

				await Verify_2_4.VerifyAnalyzerAsync(source);
			}

			private static string CreateSourceWithStringConst(string inlineData, string parameterType)
			{
				return @"
public class TestClass
{
    private const string MyConstString = ""some string"";

    [Xunit.Theory, Xunit.InlineData(" + inlineData + @")]
    public void TestMethod(" + parameterType + @" parameter)
    {
    }
}";
			}
		}

		public class ForConversionToGuid : InlineDataMustMatchTheoryParametersTests
		{
			public static IEnumerable<Tuple<string>> AcceptableGuidStrings { get; } =
				new[]
				{
					"\"\"",
					"\"{5B21E154-15EB-4B1E-BC30-127E8A41ECA1}\"",
					"\"4EBCD32C-A2B8-4600-9E72-3873347E285C\"",
					"\"39A3B4C85FEF43A988EB4BB4AC4D4103\"",
					"\"obviously-rubbish-guid-value\""
				}.Select(x => Tuple.Create(x)).ToArray();

			[Theory]
			[TupleMemberData(nameof(ValueTypedValues))]
			[InlineData("MyConstInt")]
			public async void FindsError_FromNonString(string inlineData)
			{
				var source = @"
public class TestClass
{
    private const int MyConstInt = 1;

    [Xunit.Theory, Xunit.InlineData(" + inlineData + @")]
    public void TestMethod(System.Guid parameter)
    {
    }
}";

				var expected = Verify.Diagnostic("xUnit1010").WithSpan(6, 37, 6, 37 + inlineData.Length).WithArguments("parameter", "System.Guid");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[TupleMemberData(nameof(AcceptableGuidStrings))]
			public async void FindsError__ForAnalyzerLessThan_2_4_0v_FromString(string inlineData)
			{
				var source = CreateSource(inlineData);

				var expected = Verify_2_3_1.Diagnostic("xUnit1010").WithSpan(4, 37, 4, 37 + inlineData.Length).WithArguments("parameter", "System.Guid");
				await Verify_2_3_1.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[TupleMemberData(nameof(AcceptableGuidStrings))]
			public async void DoesNotFindError_ForAnalyzerGreaterThanEqual_2_4_0v_FromString(string inlineData)
			{
				var source = CreateSource(inlineData);

				await Verify_2_5.VerifyAnalyzerAsync(source);
			}

			private static string CreateSource(string inlineData)
			{
				return @"
public class TestClass
{
    [Xunit.Theory, Xunit.InlineData(" + inlineData + @")]
    public void TestMethod(System.Guid parameter)
    {
    }
}";
			}
		}

		// Note: decimal literal 42M is not valid as an attribute argument
		public static IEnumerable<Tuple<string>> IntegerValues { get; }
			= new[] { "42", "42L", "42u", "42ul", "(short)42", "(byte)42", "(ushort)42", "(sbyte)42", }.Select(v => Tuple.Create(v)).ToArray();

		public static IEnumerable<Tuple<string>> FloatingPointValues { get; }
			= new[] { "42f", "42d" }.Select(v => Tuple.Create(v)).ToArray();

		public static IEnumerable<Tuple<string>> NumericValues { get; }
			= IntegerValues.Concat(FloatingPointValues).ToArray();

		public static IEnumerable<Tuple<string>> BoolValues { get; }
			= new[] { "true", "false" }.Select(v => Tuple.Create(v)).ToArray();

		public static IEnumerable<Tuple<string>> ValueTypedValues { get; }
			= NumericValues.Concat(BoolValues).Concat(new[] { "typeof(int)" }.Select(v => Tuple.Create(v)));

		internal class Analyzer_2_3_1 : InlineDataMustMatchTheoryParameters
		{
			public Analyzer_2_3_1()
				: base("2.3.1")
			{ }
		}

		internal class Analyzer_2_4_0 : InlineDataMustMatchTheoryParameters
		{
			public Analyzer_2_4_0()
				: base("2.4.0")
			{ }
		}

		internal class Analyzer_2_5_0 : InlineDataMustMatchTheoryParameters
		{
			public Analyzer_2_5_0()
				: base("2.5.0")
			{ }
		}
	}
}
