using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMemberTests
{
	public class X1014_MemberDataShouldUseNameOfOperator
	{
		static readonly string sharedCode = @"
using System.Collections.Generic;
using Xunit;

public partial class TestClass {
    public static TheoryData<int> Data { get; set; }
}

public class OtherClass {
    public static TheoryData<int> OtherData { get; set; }
}";

		[Fact]
		public async void NameofOnSameClass_DoesNotTrigger()
		{
			var source = @"
public partial class TestClass {
    [Xunit.MemberData(nameof(Data))]
    public void TestMethod(int _) { }
}";

			await Verify.VerifyAnalyzer(new[] { source, sharedCode });
		}

		[Fact]
		public async void NameofOnOtherClass_DoesNotTrigger()
		{
			var source = @"
public partial class TestClass {
    [Xunit.MemberData(nameof(OtherClass.OtherData), MemberType = typeof(OtherClass))]
    public void TestMethod(int _) { }
}";

			await Verify.VerifyAnalyzer(new[] { source, sharedCode });
		}

		[Fact]
		public async void StringNameOnSameClass_Triggers()
		{
			var source = @"
public partial class TestClass {
    [Xunit.MemberData(""Data"")]
    public void TestMethod(int _) { }
}";
			var expected =
				Verify
					.Diagnostic("xUnit1014")
					.WithSpan(3, 23, 3, 29)
					.WithArguments("Data", "TestClass");

			await Verify.VerifyAnalyzer(new[] { source, sharedCode }, expected);
		}

		[Fact]
		public async void StringNameOnOtherClass_Triggers()
		{
			var source = @"
public partial class TestClass {
    [Xunit.MemberData(""OtherData"", MemberType = typeof(OtherClass))]
    public void TestMethod(int _) { }
}";
			var expected =
				Verify
					.Diagnostic("xUnit1014")
					.WithSpan(3, 23, 3, 34)
					.WithArguments("OtherData", "OtherClass");

			await Verify.VerifyAnalyzer(new[] { source, sharedCode }, expected);
		}
	}

	public class X1015_MemberDataMustReferenceExistingMember
	{
		[Theory]
		[InlineData("")]
		[InlineData(", MemberType = typeof(TestClass)")]
		public async void InvalidStringNameOnSameClass_Triggers(string memberType)
		{
			var source = @$"
public class TestClass {{
    [Xunit.MemberData(""BogusName""{memberType})]
    public void TestMethod() {{ }}
}}";
			var expected =
				Verify
					.Diagnostic("xUnit1015")
					.WithSpan(3, 6, 3, 35 + memberType.Length)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("BogusName", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void InvalidStringNameOnOtherClass_Triggers()
		{
			var source1 = @"
public class TestClass {
    [Xunit.MemberData(""BogusName"", MemberType = typeof(OtherClass))]
    public void TestMethod() { }
}";
			var source2 = "public class OtherClass { }";
			var expected =
				Verify
					.Diagnostic("xUnit1015")
					.WithSpan(3, 6, 3, 68)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("BogusName", "OtherClass");

			await Verify.VerifyAnalyzer(new[] { source1, source2 }, expected);
		}

		[Fact]
		public async void InvalidNameofOnOtherClass_Triggers()
		{
			var source1 = @"
public class TestClass {
    [Xunit.MemberData(nameof(TestClass.TestMethod), MemberType = typeof(OtherClass))]
    public void TestMethod() { }
}";
			var source2 = "public class OtherClass { }";
			var expected =
				Verify
					.Diagnostic("xUnit1015")
					.WithSpan(3, 6, 3, 85)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("TestMethod", "OtherClass");

			await Verify.VerifyAnalyzer(new[] { source1, source2 }, expected);
		}
	}

	public class X1016_MemberDataMustReferencePublicMember
	{
		[Fact]
		public async void PublicMember_DoesNotTrigger()
		{
			var source = @"
public class TestClass {
    public static Xunit.TheoryData<int> Data = null;

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod(int _) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		public static MatrixTheoryData<string, string> NonPublicTestData =>
			new(
				new[] { "", "private", "protected", "internal", "protected internal" },
				new[] { "{|xUnit1014:\"Data\"|}", "DataNameConst", "DataNameofConst", "nameof(Data)", "nameof(TestClass.Data)", "OtherClass.Data", "nameof(OtherClass.Data)" }
			);

		[Theory]
		[MemberData(nameof(NonPublicTestData))]
		public async void NonPublicNameExpression_Triggers(
			string accessModifier,
			string dataNameExpression)
		{
			TestFileMarkupParser.GetPositionsAndSpans(dataNameExpression, out var parsedDataNameExpression, out _, out _);
			var dataNameExpressionLength = parsedDataNameExpression.Length;

			var source1 = $@"
public class TestClass {{
    const string DataNameConst = ""Data"";
    const string DataNameofConst = nameof(Data);

    {accessModifier} static Xunit.TheoryData<int> Data = null;

    [Xunit.MemberData({dataNameExpression})]
    public void TestMethod(int _) {{ }}
}}";
			var source2 = @"public static class OtherClass { public const string Data = ""Data""; }";
			var expected =
				Verify
					.Diagnostic("xUnit1016")
					.WithSpan(8, 6, 8, 24 + dataNameExpressionLength)
					.WithSeverity(DiagnosticSeverity.Error);

			await Verify.VerifyAnalyzer(new[] { source1, source2 }, expected);
		}
	}

	public class X1017_MemberDataMustReferenceStaticMember
	{
		[Fact]
		public async void StaticMember_DoesNotTrigger()
		{
			var source = @"
public class TestClass {
    public static Xunit.TheoryData<int> Data = null;

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod(int _) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void InstanceMember_Triggers()
		{
			var source = @"
public class TestClass {
    public Xunit.TheoryData<int> Data = null;

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod(int _) { }
}";
			var expected =
				Verify
					.Diagnostic("xUnit1017")
					.WithSpan(5, 6, 5, 36)
					.WithSeverity(DiagnosticSeverity.Error);

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1018_MemberDataMustReferenceValidMemberKind
	{
		[Theory]
		[InlineData("Data;")]
		[InlineData("Data { get; set; }")]
		[InlineData("Data() { return null; }")]
		public async void ValidMemberKind_DoesNotTrigger(string member)
		{
			var source = $@"
public class TestClass {{
    public static Xunit.TheoryData<int> {member}

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod(int _) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("public delegate System.Collections.Generic.IEnumerable<object[]> Data();")]
		[InlineData("public static class Data { }")]
		[InlineData("public static event System.EventHandler Data;")]
		public async void InvalidMemberKind_Triggers(string member)
		{
			var source = $@"
public class TestClass {{
    {member}

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod() {{ }}
}}";
			var expected =
				Verify
					.Diagnostic("xUnit1018")
					.WithSpan(5, 6, 5, 36)
					.WithSeverity(DiagnosticSeverity.Error);

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1019_MemberDataMustReferenceMemberOfValidType
	{
		// The base type of IEnumerable<object[]> triggers xUnit1042, which is covered in the tests
		// below in X1042_MemberDataTheoryDataIsRecommendedForStronglyTypedAnalysis, so we'll only
		// test TheoryData<> and IEnumerable<ITheoryDataRow> here.

		[Fact]
		public async void TheoryData_DoesNotTrigger()
		{
			var source = @"
using System.Collections.Generic;
using Xunit;

public class TestClass {
    public static TheoryData<int> Data;

    [MemberData(nameof(Data))]
    public void TestMethod(int _) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void ITheoryDataRow_DoesNotTrigger()
		{
			var source = @"
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

public class TestClass
{
    [Theory]
    [MemberData(nameof(DataRowSource))]
    public void SkippedDataRow(int x, string y)
    { }

    public static List<TheoryDataRow> DataRowSource() =>
        new List<TheoryDataRow>()
        {
            new TheoryDataRow(42, ""Hello, world!""),
            new TheoryDataRow(0, null) { Skip = ""Don't run this!"" },
        };
}";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[InlineData("System.Collections.Generic.IEnumerable<object>")]
		[InlineData("object[]")]
		[InlineData("object")]
		[InlineData("System.Tuple<string, int>")]
		[InlineData("System.Tuple<string, int>[]")]
		public async void InvalidMemberType_Triggers(string memberType)
		{
			var source = $@"
public class TestClass {{
    public static {memberType} Data;

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod() {{ }}
}}";
			var expectedV2 =
				Verify
					.Diagnostic("xUnit1019")
					.WithSpan(5, 6, 5, 36)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("'System.Collections.Generic.IEnumerable<object[]>'", memberType);

			await Verify.VerifyAnalyzerV2(source, expectedV2);

			var expectedV3 =
				Verify
					.Diagnostic("xUnit1019")
					.WithSpan(5, 6, 5, 36)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("'System.Collections.Generic.IEnumerable<object[]>' or 'System.Collections.Generic.IEnumerable<Xunit.ITheoryDataRow>'", memberType);

			await Verify.VerifyAnalyzerV3(source, expectedV3);
		}
	}

	public class X1020_MemberDataPropertyMustHaveGetter
	{
		[Fact]
		public async void PropertyWithoutGetter_Triggers()
		{
			var source = @"
public class TestClass {
    public static Xunit.TheoryData<int> Data { set { } }

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod(int _) { }
}";
			var expected =
				Verify
					.Diagnostic("xUnit1020")
					.WithSpan(5, 6, 5, 36)
					.WithSeverity(DiagnosticSeverity.Error);

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[InlineData("internal")]
		[InlineData("protected")]
		[InlineData("private")]
		public async void PropertyWithNonPublicGetter_Triggers(string visibility)
		{
			var source = $@"
public class TestClass {{
    public static Xunit.TheoryData<int> Data {{ {visibility} get {{ return null; }} set {{ }} }}

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod(int _) {{ }}
}}";
			var expected =
				Verify
					.Diagnostic("xUnit1020")
					.WithSpan(5, 6, 5, 36)
					.WithSeverity(DiagnosticSeverity.Error);

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1021_MemberDataNonMethodShouldNotHaveParameters
	{
		[Theory]
		[InlineData("1")]                   // implicit params
		[InlineData("new object[] { 1 }")]  // explicit params
		public async void MethodMemberWithParameters_DoesNotTrigger(string parameter)
		{
			var source = @$"
public class TestClass {{
    private static void TestData() {{ }}

    public static Xunit.TheoryData<int> TestData(int n) => new Xunit.TheoryData<int> {{ n }};

    [Xunit.MemberData(nameof(TestData), {parameter})]
    public void TestMethod(int n) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("1, 2")]                   // implicit params
		[InlineData("new object[] { 1, 2 }")]  // explicit params
		public async void MethodMemberWithParamsArrayParameters_DoesNotTrigger(string parameters)
		{
			var source = @$"
public class TestClass {{
    public static Xunit.TheoryData<int> TestData(params int[] n) => new Xunit.TheoryData<int> {{ n[0] }};

    [Xunit.MemberData(nameof(TestData), {parameters})]
    public void TestMethod(int n) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("1")]                   // implicit params
		[InlineData("new object[] { 1 }")]  // explicit params
		public async void MethodMemberOnBaseType_DoesNotTrigger(string parameter)
		{
			var source = $@"
public class TestClassBase {{
    public static Xunit.TheoryData<int> TestData(int n) => new Xunit.TheoryData<int> {{ n }};
}}

public class TestClass : TestClassBase {{
    private static void TestData() {{ }}

    [Xunit.MemberData(nameof(TestData), {parameter})]
    public void TestMethod(int n) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("'a', 123")]
		[InlineData("new object[] {{ 'a', 123 }}")]
		[InlineData("{0}: new object[] {{ 'a', 123 }}")]
		public async void FieldMemberWithParameters_Triggers(string paramsArgument)
		{
			var sourceTemplate = @"
public class TestClass {{
    public static Xunit.TheoryData<int> Data;

    [Xunit.MemberData(nameof(Data), {0}, MemberType = typeof(TestClass))]
    public void TestMethod(int _) {{ }}
}}";

			var argV2 = string.Format(paramsArgument, "parameters");
			var sourceV2 = string.Format(sourceTemplate, argV2);
			var expectedV2 =
				Verify
					.Diagnostic("xUnit1021")
					.WithSpan(5, 37, 5, 37 + argV2.Length)
					.WithSeverity(DiagnosticSeverity.Warning);

			await Verify.VerifyAnalyzerV2(sourceV2, expectedV2);

			var argV3 = string.Format(paramsArgument, "arguments");
			var sourceV3 = string.Format(sourceTemplate, argV3);
			var expectedV3 =
				Verify
					.Diagnostic("xUnit1021")
					.WithSpan(5, 37, 5, 37 + argV3.Length)
					.WithSeverity(DiagnosticSeverity.Warning);

			await Verify.VerifyAnalyzerV3(sourceV3, expectedV3);
		}

		[Theory]
		[InlineData("'a', 123")]
		[InlineData("new object[] {{ 'a', 123 }}")]
		[InlineData("{0}: new object[] {{ 'a', 123 }}")]
		public async void PropertyMemberWithParameters_Triggers(string paramsArgument)
		{
			var sourceTemplate = @"
public class TestClass {{
    public static Xunit.TheoryData<int> Data {{ get; set; }}

    [Xunit.MemberData(nameof(Data), {0}, MemberType = typeof(TestClass))]
    public void TestMethod(int _) {{ }}
}}";

			var argV2 = string.Format(paramsArgument, "parameters");
			var sourceV2 = string.Format(sourceTemplate, argV2);
			var expectedV2 =
				Verify
					.Diagnostic("xUnit1021")
					.WithSpan(5, 37, 5, 37 + argV2.Length)
					.WithSeverity(DiagnosticSeverity.Warning);

			await Verify.VerifyAnalyzerV2(sourceV2, expectedV2);

			var argV3 = string.Format(paramsArgument, "arguments");
			var sourceV3 = string.Format(sourceTemplate, argV3);
			var expectedV3 =
				Verify
					.Diagnostic("xUnit1021")
					.WithSpan(5, 37, 5, 37 + argV3.Length)
					.WithSeverity(DiagnosticSeverity.Warning);

			await Verify.VerifyAnalyzerV3(sourceV3, expectedV3);
		}
	}

	public class X1034_MemberDataArgumentsMustMatchMethodParameters_NullShouldNotBeUsedForIncompatibleParameter
	{
		[Theory]
		[InlineData("", "string")]
		[InlineData("#nullable enable", "string?")]
		public async void PassingNullForNullableReferenceType_DoesNotTrigger(
			string header,
			string argumentType)
		{
			var source = $@"
{header}
public class TestClass {{
    public static Xunit.TheoryData<int> TestData({argumentType} f) => new Xunit.TheoryData<int> {{ 42 }};

    [Xunit.MemberData(nameof(TestData), new object[] {{ null }})]
    public void TestMethod(int _) {{ }}
}}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Fact]
		public async void PassingNullForStructType_Triggers()
		{
			var source = @"
public class TestClass {
    public static Xunit.TheoryData<int> TestData(int n) => new Xunit.TheoryData<int> { n };

    [Xunit.MemberData(nameof(TestData), new object[] { null })]
    public void TestMethod(int _) { }
}";

			var expected =
				Verify
					.Diagnostic("xUnit1034")
					.WithSpan(5, 56, 5, 60)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("n", "int");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void PassingNullForNonNullableReferenceType_Triggers()
		{
			var source = @"
#nullable enable
public class TestClass {
    public static Xunit.TheoryData<string> TestData(string f) => new Xunit.TheoryData<string> { f };

    [Xunit.MemberData(nameof(TestData), new object[] { null })]
    public void TestMethod(string _) { }
}
#nullable restore";

			var expected =
				Verify
					.Diagnostic("xUnit1034")
					.WithSpan(6, 56, 6, 60)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("f", "string");

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}
	}

	public class X1035_MemberDataArgumentsMustMatchMethodParameters_IncompatibleValueType
	{
		// https://github.com/xunit/xunit/issues/2817
		[Theory]
		[InlineData("Foo.Bar")]
		[InlineData("(Foo)42")]
		public async void ValidEnumValue_DoesNotTrigger(string enumValue)
		{
			var source = $@"
using System;
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    [Theory]
    [MemberData(nameof(SomeData), {enumValue})]
    public void TestMethod(int _) {{ }}

    public enum Foo {{ Bar }}

    public static Xunit.TheoryData<int> SomeData(Foo foo) => new Xunit.TheoryData<int>();
}}";

			await Verify.VerifyAnalyzer(source);
		}

		// https://github.com/xunit/xunit/issues/2852
		[Theory]
		[InlineData("")]
		[InlineData("#nullable enable")]
		public async void ArrayInitializerWithCorrectType_DoesNotTrigger(string header)
		{
			var source = $@"
{header}

using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static TheoryData<int> GetSequences(IEnumerable<int> seq) => new TheoryData<int> {{ 42, 2112 }};

    [Theory]
    [MemberData(nameof(GetSequences), new int[] {{ 1, 2 }})]
    public void Test(int value) {{ }}
}}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Theory]
		[InlineData("")]
		[InlineData("#nullable enable")]
		public async void ArrayInitializerWithIncorrectType_Triggers(string header)
		{
			var source = $@"
{header}

using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static TheoryData<int> GetSequences(IEnumerable<int> seq) => new TheoryData<int> {{ 42, 2112 }};

    [Theory]
    [MemberData(nameof(GetSequences), new char[] {{ 'a', 'b' }})]
    public void Test(int value) {{ }}
}}";
			var expected =
				Verify
					.Diagnostic("xUnit1035")
					.WithSpan(11, 39, 11, 62)
					.WithArguments("seq", "System.Collections.Generic.IEnumerable<int>");

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}

		[Fact]
		public async void ValidMemberWithIncorrectArgumentTypes_Triggers()
		{
			var source = @"
public class TestClass {
    public static Xunit.TheoryData<int> TestData(string n) => new Xunit.TheoryData<int> { n.Length };

    [Xunit.MemberData(nameof(TestData), new object[] { 1 })]
    public void TestMethod(int n) { }
}";

			var expected =
				Verify
					.Diagnostic("xUnit1035")
					.WithSpan(5, 56, 5, 57)
					.WithArguments("n", "string");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void ValidMemberWithIncorrectArgumentTypesParams_Triggers()
		{
			var source = @"
public class TestClass {
    public static Xunit.TheoryData<int> TestData(params int[] n) => new Xunit.TheoryData<int> { n[0] };

    [Xunit.MemberData(nameof(TestData), new object[] { 1, ""bob"" })]
    public void TestMethod(int n) { }
}";

			var expected =
				Verify
					.Diagnostic("xUnit1035")
					.WithSpan(5, 59, 5, 64)
					.WithArguments("n", "int");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1036_MemberDataArgumentsMustMatchMethodParameters_ExtraValue
	{
		[Theory]
		[InlineData("1")]
		[InlineData("new object[] { 1 }")]
		public async void ValidArgumentCount_DoesNotTrigger(string parameter)
		{
			var source = $@"
public class TestClass {{
    private static void TestData() {{ }}

    public static Xunit.TheoryData<int> TestData(int n) => new Xunit.TheoryData<int> {{ n }};

    [Xunit.MemberData(nameof(TestData), {parameter})]
    public void TestMethod(int n) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("1")]
		[InlineData("new object[] { 1 }")]
		public async void ValidArgumentCount_InNullableContext_DoesNotTrigger(string parameter)
		{
			var source = $@"
#nullable enable
public class TestClass {{
    public static Xunit.TheoryData<int> TestData(int n) => new Xunit.TheoryData<int> {{ n }};

    [Xunit.MemberData(nameof(TestData), {parameter})]
    public void TestMethod(int n) {{ }}
}}
#nullable restore";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Theory]
		[InlineData("1, 2", 44)]
		[InlineData("new object[] { 1, 2 }", 59)]
		public async void TooManyArguments_Triggers(
			string parameters,
			int startColumn)
		{
			var source = $@"
public class TestClass {{
    public static Xunit.TheoryData<int> TestData(int n) => new Xunit.TheoryData<int> {{ n }};

    [Xunit.MemberData(nameof(TestData), {parameters})]
    public void TestMethod(int n) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1036")
						.WithSpan(5, startColumn, 5, startColumn + 1)
						.WithArguments("2");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1037_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_TooFewTypeParameters
	{
		public static TheoryData<string, string> MemberSyntaxAndArgs = new()
		{
			{ " = ", "" },              // Field
			{ " => ", "" },             // Property
			{ "() => ", "" },           // Method w/o args
			{ "(int n) => ", ", 42" },  // Method w/ args
		};

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async void ValidTheoryDataMemberWithNotEnoughTypeParameters_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
public class TestClass {{
    public static Xunit.TheoryData<int> TestData{memberSyntax}new Xunit.TheoryData<int>();

    [Xunit.MemberData(nameof(TestData){memberArgs})]
    public void TestMethod(int n, string f) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1037")
					.WithSpan(5, 6, 5, 40 + memberArgs.Length)
					.WithSeverity(DiagnosticSeverity.Error);

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async void ValidSubclassedTheoryDataMemberWithNotEnoughTypeParameters_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
using Xunit;

public class DerivedTheoryData<T, U> : TheoryData<T> {{ }}

public class TestClass {{
    public static DerivedTheoryData<int, string> TestData{memberSyntax}new DerivedTheoryData<int, string>();

    [Xunit.MemberData(nameof(TestData){memberArgs})]
    public void TestMethod(int n, string f) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1037")
					.WithSpan(9, 6, 9, 40 + memberArgs.Length)
					.WithSeverity(DiagnosticSeverity.Error);

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1038_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_ExtraTypeParameters
	{
		public static MatrixTheoryData<(string syntax, string args), string> MemberSyntaxAndArgs_WithTheoryDataType(string theoryDataTypes) =>
			new(
				new[]
				{
					( " = ", "" ),              // Field
					( " => ", "" ),             // Property
					( "() => ", "" ),           // Method w/o args
					( "(int n) => ", ", 42" ),  // Method w/ args
				},
				new[]
				{
					$"TheoryData<{theoryDataTypes}>",
					"DerivedTheoryData",
					$"DerivedTheoryData<{theoryDataTypes}>"
				}
			);

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async void ValidTheoryData_DoesNotTrigger(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = $@"
using Xunit;

public class DerivedTheoryData : TheoryData<int> {{ }}
public class DerivedTheoryData<T> : TheoryData<T> {{ }}

public class TestClass {{
    public static {theoryDataType} TestData{member.syntax}new {theoryDataType}();

    [MemberData(nameof(TestData){member.args})]
    public void TestMethod(int n) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async void ValidTheoryDataWithOptionalParameters_DoesNotTrigger(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = $@"
using Xunit;

public class DerivedTheoryData : TheoryData<int> {{ }}
public class DerivedTheoryData<T> : TheoryData<T> {{ }}

public class TestClass {{
    public static {theoryDataType} TestData{member.syntax}new {theoryDataType}();

    [MemberData(nameof(TestData){member.args})]
    public void TestMethod(int n, int a = 0) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async void ValidTheoryDataWithNoValuesForParamsArray_DoesNotTrigger(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = $@"
using Xunit;

public class DerivedTheoryData : TheoryData<int> {{ }}
public class DerivedTheoryData<T> : TheoryData<T> {{ }}

public class TestClass {{
    public static {theoryDataType} TestData{member.syntax}new {theoryDataType}();

    [Xunit.MemberData(nameof(TestData){member.args})]
    public void TestMethod(int n, params int[] a) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int, int", DisableDiscoveryEnumeration = true)]
		public async void ValidTheoryDataWithSingleValueForParamsArray_DoesNotTrigger(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = $@"
using Xunit;

public class DerivedTheoryData : TheoryData<int, int> {{ }}
public class DerivedTheoryData<T1, T2> : TheoryData<T1, T2> {{ }}

public class TestClass {{
    public static {theoryDataType} TestData{member.syntax}new {theoryDataType}();

    [MemberData(nameof(TestData){member.args})]
    public void TestMethod(int n, params int[] a) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async void ValidTheoryDataWithGenericTestParameter_DoesNotTrigger(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = $@"
using Xunit;

public class DerivedTheoryData : TheoryData<int> {{ }}
public class DerivedTheoryData<T> : TheoryData<T> {{ }}

public class TestClass {{
    public static {theoryDataType} TestData{member.syntax}new {theoryDataType}();

    [MemberData(nameof(TestData){member.args})]
    public void TestMethod<T>(T n) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async void ValidTheoryDataWithNullableGenericTestParameter_DoesNotTrigger(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = $@"
#nullable enable

using Xunit;

public class DerivedTheoryData : TheoryData<int> {{ }}
public class DerivedTheoryData<T> : TheoryData<T> {{ }}

public class TestClass {{
    public static {theoryDataType} TestData{member.syntax}new {theoryDataType}();

    [Xunit.MemberData(nameof(TestData){member.args})]
    public void TestMethod<T>(T? n) {{ }}
}}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source);
		}

		[Theory]
		[InlineData(" = ", "")]              // Field
		[InlineData(" => ", "")]             // Property
		[InlineData("() => ", "")]           // Method w/o args
		[InlineData("(int n) => ", ", 42")]  // Method w/ args
		public async void ValidTheoryDataDoubleGenericSubclassMember_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
using Xunit;

public class DerivedTheoryData<T, U> : TheoryData<T> {{ }}

public class TestClass {{
    public static DerivedTheoryData<int, string> TestData{memberSyntax}new DerivedTheoryData<int, string>();

    [MemberData(nameof(TestData){memberArgs})]
    public void TestMethod(int n) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void WithIntArrayArguments_DoesNotTrigger()
		{
			var source = @"
using System.Collections.Generic;
using Xunit;

public class TestClass {
    public static TheoryData<IEnumerable<int>> GetSequences(IEnumerable<int> seq) => new TheoryData<IEnumerable<int>> { seq };

    [Theory]
    [MemberData(nameof(GetSequences), new[] { 1, 2 })]
    [MemberData(nameof(GetSequences), new[] { 3, 4, 5 })]
    public void Test(IEnumerable<int> seq) {
        Assert.NotEmpty(seq);
    }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int, string", DisableDiscoveryEnumeration = true)]
		public async void ValidSubclassTheoryDataMemberWithTooManyTypeParameters_Triggers(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = $@"
using Xunit;

public class DerivedTheoryData : TheoryData<int, string> {{ }}
public class DerivedTheoryData<T1, T2> : TheoryData<T1, T2> {{ }}

public class TestClass {{
    public static {theoryDataType} TestData{member.syntax}new {theoryDataType}();

    [MemberData(nameof(TestData){member.args})]
    public void TestMethod(int n) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1038")
					.WithSpan(10, 6, 10, 34 + member.args.Length)
					.WithSeverity(DiagnosticSeverity.Error);

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int, string[], string", DisableDiscoveryEnumeration = true)]
		public async void ExtraTypeExistsPastArrayForParamsArray_Triggers(
			(string syntax, string args) member,
			string theoryDataType)
		{
			var source = $@"
using Xunit;

public class DerivedTheoryData : TheoryData<int, string[], string> {{ }}
public class DerivedTheoryData<T1, T2, T3> : TheoryData<T1, T2, T3> {{ }}

public class TestClass {{
	public static {theoryDataType} TestData{member.syntax}new {theoryDataType}();

    [MemberData(nameof(TestData){member.args})]
    public void PuzzleOne(int _1, params string[] _2) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1038")
					.WithSpan(10, 6, 10, 34 + member.args.Length)
					.WithSeverity(DiagnosticSeverity.Error);

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1039_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes
	{
		public static MatrixTheoryData<(string syntax, string args), string> TypeWithMemberSyntaxAndArgs =
			new(
				new[]
				{
					( " = ", "" ),              // Field
					( " => ", "" ),             // Property
					( "() => ", "" ),           // Method w/o args
					( "(int n) => ", ", 42" ),  // Method w/ args
				},
				new[]
				{
					"int",
					"System.Exception",
				}
			);

		[Fact]
		public async void DoesNotFindWarning_WhenPassingMultipleValuesForParamsArray()
		{
			var source = @"
using Xunit;

public class TestClass {
	public static TheoryData<int, string, string> TestData = new TheoryData<int, string, string>();

    [MemberData(nameof(TestData))]
    public void PuzzleOne(int _1, params string[] _2) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void DoesNotFindWarning_WhenPassingArrayForParamsArray()
		{
			var source = @"
using Xunit;

public class TestClass {
	public static TheoryData<int, string[]> TestData = new TheoryData<int, string[]>();

    [MemberData(nameof(TestData))]
    public void PuzzleOne(int _1, params string[] _2) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void DoesNotFindWarning_WhenPassingTupleWithoutFieldNames()
		{
			var source = @"
using Xunit;

public class TestClass {
	public static TheoryData<(int, int)> TestData = new TheoryData<(int, int)>();

    [MemberData(nameof(TestData))]
    public void TestMethod((int a, int b) x) { }
}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Fact]
		public async void DoesNotFindWarning_WhenPassingTupleWithDifferentFieldNames()
		{
			var source = @"
using Xunit;

public class TestClass {
	public static TheoryData<(int c, int d)> TestData = new TheoryData<(int, int)>();

    [MemberData(nameof(TestData))]
    public void TestMethod((int a, int b) x) { }
}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Fact]
		public async void FindWarning_WithExtraValueNotCompatibleWithParamsArray()
		{
			var source = @"
using Xunit;

public class TestClass {
	public static TheoryData<int, string, int> TestData = new TheoryData<int, string, int>();

    [MemberData(nameof(TestData))]
    public void PuzzleOne(int _1, params string[] _2) { }
}";

			var expected =
				Verify
					.Diagnostic("xUnit1039")
					.WithSpan(8, 42, 8, 50)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("int", "TestClass", "TestData", "_2");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(TypeWithMemberSyntaxAndArgs), DisableDiscoveryEnumeration = true)]
		public async void FindWarning_IfHasValidTheoryDataMemberWithIncompatibleTypeParameters(
			(string syntax, string args) member,
			string type)
		{
			var source = $@"
using Xunit;

public class TestClass {{
    public static TheoryData<{type}> TestData{member.syntax}new TheoryData<{type}>();

    [MemberData(nameof(TestData){member.args})]
    public void TestMethod(string f) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1039")
					.WithSpan(8, 28, 8, 34)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments(type, "TestClass", "TestData", "f");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1040_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability
	{
		public static TheoryData<string, string> MemberSyntaxAndArgs = new()
		{
			{ " = ", "" },              // Field
			{ " => ", "" },             // Property
			{ "() => ", "" },           // Method w/o args
			{ "(int n) => ", ", 42" },  // Method w/ args
		};

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async void ValidTheoryDataMemberWithMismatchedNullability_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
#nullable enable

using Xunit;

public class TestClass {{
    public static TheoryData<string?> TestData{memberSyntax}new TheoryData<string?>();

    [MemberData(nameof(TestData){memberArgs})]
    public void TestMethod(string f) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1040")
					.WithSpan(10, 28, 10, 34)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("string?", "TestClass", "TestData", "f");

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}
	}

	public class X1042_MemberDataTheoryDataIsRecommendedForStronglyTypedAnalysis
	{
		[Fact]
		public async void TheoryData_DoesNotTrigger()
		{
			var source = @"
using System.Collections.Generic;
using Xunit;

public class TestClass {
    public static TheoryData<int> Data;

    [MemberData(nameof(Data))]
    public void TestMethod(int _) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void MatrixTheoryData_DoesNotTrigger()
		{
			var source = @"
using System.Collections.Generic;
using Xunit;

public class TestClass {
    public static MatrixTheoryData<int, string> Data;

    [MemberData(nameof(Data))]
    public void TestMethod(int _1, string _2) { }
}";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[InlineData("IEnumerable<ITheoryDataRow>")]
		[InlineData("List<ITheoryDataRow>")]
		[InlineData("ITheoryDataRow[]")]
		public async void TheoryDataRow_DoesNotTrigger(string memberType)
		{
			var source = $@"
using System.Collections.Generic;
using Xunit;
using Xunit.v3;

public class TestClass {{
    public static {memberType} Data;

    [MemberData(nameof(Data))]
    public void TestMethod(int _) {{ }}
}}";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[InlineData("IEnumerable<object[]>")]
		[InlineData("List<object[]>")]
		public async void ValidTypesWhichAreNotTheoryData_Trigger(string memberType)
		{
			var source = $@"
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static {memberType} Data;

    [MemberData(nameof(Data))]
    public void TestMethod(int _) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1042")
					.WithSpan(8, 6, 8, 30)
					.WithSeverity(DiagnosticSeverity.Info);

			await Verify.VerifyAnalyzer(source, expected);
		}
	}
}
