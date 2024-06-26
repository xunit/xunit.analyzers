using System.Threading.Tasks;
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
		public async Task NameofOnSameClass_DoesNotTrigger()
		{
			var source = @"
public partial class TestClass {
    [Xunit.MemberData(nameof(Data))]
    public void TestMethod(int _) { }
}";

			await Verify.VerifyAnalyzer(new[] { source, sharedCode });
		}

		[Fact]
		public async Task NameofOnOtherClass_DoesNotTrigger()
		{
			var source = @"
public partial class TestClass {
    [Xunit.MemberData(nameof(OtherClass.OtherData), MemberType = typeof(OtherClass))]
    public void TestMethod(int _) { }
}";

			await Verify.VerifyAnalyzer(new[] { source, sharedCode });
		}

		[Fact]
		public async Task StringNameOnSameClass_Triggers()
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
		public async Task StringNameOnOtherClass_Triggers()
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
		public async Task InvalidStringNameOnSameClass_Triggers(string memberType)
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
		public async Task InvalidStringNameOnOtherClass_Triggers()
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
		public async Task InvalidNameofOnOtherClass_Triggers()
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
		public async Task PublicMember_DoesNotTrigger()
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
		public async Task NonPublicNameExpression_Triggers(
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
		public async Task StaticMember_DoesNotTrigger()
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
		public async Task InstanceMember_Triggers()
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
		public async Task ValidMemberKind_DoesNotTrigger(string member)
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
		public async Task InvalidMemberKind_Triggers(string member)
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
		// The base type of IEnumerable<object[]> and IEnumerable<ITheoryDataRow> trigger xUnit1042,
		// which is covered below in X1042_MemberDataTheoryDataIsRecommendedForStronglyTypedAnalysis,
		// so we'll only test TheoryData<> and IEnumerable<TheoryDataRow<>> here.

		[Fact]
		public async Task TheoryData_DoesNotTrigger()
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

		[Theory]
		[InlineData("Task")]
		[InlineData("ValueTask")]
		public async Task Async_TheoryData_TriggersInV2_DoesNotTriggerInV3(string taskType)
		{
			var source = @$"
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    public static {taskType}<TheoryData<int>> Data;

    [MemberData(nameof(Data))]
    public void TestMethod(int _) {{ }}
}}";
			var expectedV2 =
				Verify
					.Diagnostic("xUnit1019")
					.WithSpan(9, 6, 9, 30)
					.WithArguments("'System.Collections.Generic.IEnumerable<object[]>'", $"System.Threading.Tasks.{taskType}<Xunit.TheoryData<int>>");

			await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp9, source, expectedV2);
			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source);
		}

		[Theory]
		[InlineData("List<TheoryDataRow<int, string>>")]
		[InlineData("IAsyncEnumerable<TheoryDataRow<int, string>>")]
		public async Task GenericTheoryDataRow_DoesNotTrigger(string dataType)
		{
			var source = @$"
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

public class TestClass
{{
    [Theory]
    [MemberData(nameof(DataRowSource))]
    public void SkippedDataRow(int x, string y)
    {{ }}

    public static {dataType} DataRowSource() {{
        throw new NotImplementedException();
    }}
}}";

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source);
		}

		[Theory]
		[InlineData("Task<List<TheoryDataRow<int, string>>>")]
		[InlineData("ValueTask<IAsyncEnumerable<TheoryDataRow<int, string>>>")]
		public async Task Async_GenericTheoryDataRow_DoesNotTrigger(string taskType)
		{
			var source = @$"
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class TestClass
{{
    [Theory]
    [MemberData(nameof(DataRowSource))]
    public void SkippedDataRow(int x, string y)
    {{ }}

    public static async {taskType} DataRowSource() {{
        throw new NotImplementedException();
    }}
}}";

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source);
		}

		[Theory]
		[InlineData("System.Collections.Generic.IEnumerable<object>")]
		[InlineData("object[]")]
		[InlineData("object")]
		[InlineData("System.Tuple<string, int>")]
		[InlineData("System.Tuple<string, int>[]")]
		public async Task InvalidMemberType_Triggers(string memberType)
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
					.WithArguments("'System.Collections.Generic.IEnumerable<object[]>', 'System.Collections.Generic.IAsyncEnumerable<object[]>', 'System.Collections.Generic.IEnumerable<Xunit.ITheoryDataRow>', or 'System.Collections.Generic.IAsyncEnumerable<Xunit.ITheoryDataRow>'", memberType);

			await Verify.VerifyAnalyzerV3(source, expectedV3);
		}
	}

	public class X1020_MemberDataPropertyMustHaveGetter
	{
		[Fact]
		public async Task PropertyWithoutGetter_Triggers()
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
		public async Task PropertyWithNonPublicGetter_Triggers(string visibility)
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
		public async Task MethodMemberWithParameters_DoesNotTrigger(string parameter)
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
		public async Task MethodMemberWithParamsArrayParameters_DoesNotTrigger(string parameters)
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
		public async Task MethodMemberOnBaseType_DoesNotTrigger(string parameter)
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
		public async Task FieldMemberWithParameters_Triggers(string paramsArgument)
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
		public async Task PropertyMemberWithParameters_Triggers(string paramsArgument)
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
		public async Task PassingNullForNullableReferenceType_DoesNotTrigger(
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
		public async Task PassingNullForStructType_Triggers()
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
		public async Task PassingNullForNonNullableReferenceType_Triggers()
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
		public async Task ValidEnumValue_DoesNotTrigger(string enumValue)
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
		public async Task ArrayInitializerWithCorrectType_DoesNotTrigger(string header)
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
		public async Task ArrayInitializerWithIncorrectType_Triggers(string header)
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
		public async Task ValidMemberWithIncorrectArgumentTypes_Triggers()
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
		public async Task ValidMemberWithIncorrectArgumentTypesParams_Triggers()
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
		public async Task ValidArgumentCount_DoesNotTrigger(string parameter)
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
		public async Task ValidArgumentCount_InNullableContext_DoesNotTrigger(string parameter)
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
		public async Task TooManyArguments_Triggers(
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

	public class X1037_TheoryDataTypeArgumentsMustMatchTestMethodParameters_TooFewTypeParameters
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
		public async Task ValidTheoryDataMemberWithNotEnoughTypeParameters_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
using Xunit;

public class TestClass {{
    public static TheoryData<int> TestData{memberSyntax}new TheoryData<int>();

    [MemberData(nameof(TestData){memberArgs})]
    public void TestMethod(int n, string f) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1037")
					.WithSpan(7, 6, 7, 34 + memberArgs.Length)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Xunit.TheoryData");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowMemberWithNotEnoughTypeParameters_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static IEnumerable<TheoryDataRow<int>> TestData{memberSyntax}null;

    [MemberData(nameof(TestData){memberArgs})]
    public void TestMethod(int n, string f) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1037")
					.WithSpan(8, 6, 8, 34 + memberArgs.Length)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Xunit.TheoryDataRow");

			await Verify.VerifyAnalyzerV3(source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidSubclassedTheoryDataMemberWithNotEnoughTypeParameters_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
using Xunit;

public class DerivedTheoryData<T, U> : TheoryData<T> {{ }}

public class TestClass {{
    public static DerivedTheoryData<int, string> TestData{memberSyntax}new DerivedTheoryData<int, string>();

    [MemberData(nameof(TestData){memberArgs})]
    public void TestMethod(int n, string f) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1037")
					.WithSpan(9, 6, 9, 34 + memberArgs.Length)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Xunit.TheoryData");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class X1038_TheoryDataTypeArgumentsMustMatchTestMethodParameters_ExtraTypeParameters
	{
		public static TheoryData<string, string> MemberSyntaxAndArgs() =>
			new() {
				{ " = ", "" },              // Field
				{ " => ", "" },             // Property
				{ "() => ", "" },           // Method w/o args
				{ "(int n) => ", ", 42" },  // Method w/ args
			};

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
		public async Task ValidTheoryData_DoesNotTrigger(
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
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRow_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static IEnumerable<TheoryDataRow<int>> TestData{memberSyntax}new List<TheoryDataRow<int>>();

    [MemberData(nameof(TestData){memberArgs})]
    public void TestMethod(int n) {{ }}
}}";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataWithOptionalParameters_DoesNotTrigger(
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
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowWithOptionalParameters_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static TheoryDataRow<int>[] TestData{memberSyntax}new TheoryDataRow<int>[0];

    [MemberData(nameof(TestData){memberArgs})]
    public void TestMethod(int n, int a = 0) {{ }}
}}";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataWithNoValuesForParamsArray_DoesNotTrigger(
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
    public void TestMethod(int n, params int[] a) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowWithNoValuesForParamsArray_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static ICollection<TheoryDataRow<int>> TestData{memberSyntax}new List<TheoryDataRow<int>>();

    [MemberData(nameof(TestData){memberArgs})]
    public void TestMethod(int n, params int[] a) {{ }}
}}";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int, int", DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataWithSingleValueForParamsArray_DoesNotTrigger(
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
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowWithSingleValueForParamsArray_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static IEnumerable<TheoryDataRow<int, int>> TestData{memberSyntax}new List<TheoryDataRow<int, int>>();

    [MemberData(nameof(TestData){memberArgs})]
    public void TestMethod(int n, params int[] a) {{ }}
}}";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataWithGenericTestParameter_DoesNotTrigger(
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
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowWithGenericTestParameter_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static ISet<TheoryDataRow<int>> TestData{memberSyntax}new HashSet<TheoryDataRow<int>>();

    [MemberData(nameof(TestData){memberArgs})]
    public void TestMethod<T>(T n) {{ }}
}}";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int", DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataWithNullableGenericTestParameter_DoesNotTrigger(
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
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowWithNullableGenericTestParameter_DoesNotTrigger(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
#nullable enable

using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static IEnumerable<TheoryDataRow<int>> TestData{memberSyntax}new List<TheoryDataRow<int>>();

    [Xunit.MemberData(nameof(TestData){memberArgs})]
    public void TestMethod<T>(T? n) {{ }}
}}";

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataDoubleGenericSubclassMember_DoesNotTrigger(
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
		public async Task WithIntArrayArguments_DoesNotTrigger()
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
		public async Task ValidSubclassTheoryDataMemberWithTooManyTypeParameters_Triggers(
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
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Xunit.TheoryData");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidSubclassTheoryDataRowMemberWithTooManyTypeParameters_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static IEnumerable<TheoryDataRow<int, string>> TestData{memberSyntax}new List<TheoryDataRow<int, string>>();

    [MemberData(nameof(TestData){memberArgs})]
    public void TestMethod(int n) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1038")
					.WithSpan(8, 6, 8, 34 + memberArgs.Length)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Xunit.TheoryDataRow");

			await Verify.VerifyAnalyzerV3(source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs_WithTheoryDataType), "int, string[], string", DisableDiscoveryEnumeration = true)]
		public async Task ExtraTheoryDataTypeExistsPastArrayForParamsArray_Triggers(
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
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Xunit.TheoryData");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ExtraTheoryDataRowTypeExistsPastArrayForParamsArray_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static ICollection<TheoryDataRow<int, string[], string>> TestData{memberSyntax}new TheoryDataRow<int, string[], string>[0];

    [MemberData(nameof(TestData){memberArgs})]
    public void PuzzleOne(int _1, params string[] _2) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1038")
					.WithSpan(8, 6, 8, 34 + memberArgs.Length)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Xunit.TheoryDataRow");

			await Verify.VerifyAnalyzerV3(source, expected);
		}
	}

	public class X1039_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes
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
		public async Task WhenPassingMultipleValuesForParamsArray_TheoryData_DoesNotTrigger()
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
		public async Task WhenPassingMultipleValuesForParamsArray_TheoryDataRow_DoesNotTrigger()
		{
			var source = @"
using System.Collections.Generic;
using Xunit;

public class TestClass {
	public static IEnumerable<TheoryDataRow<int, string, string>> TestData = new List<TheoryDataRow<int, string, string>>();

    [MemberData(nameof(TestData))]
    public void PuzzleOne(int _1, params string[] _2) { }
}";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Fact]
		public async Task WhenPassingArrayForParamsArray_TheoryData_DoesNotTrigger()
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
		public async Task WhenPassingArrayForParamsArray_TheoryDataRow_DoesNotTrigger()
		{
			var source = @"
using Xunit;

public class TestClass {
	public static TheoryDataRow<int, string[]>[] TestData = new TheoryDataRow<int, string[]>[0];

    [MemberData(nameof(TestData))]
    public void PuzzleOne(int _1, params string[] _2) { }
}";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Fact]
		public async Task WhenPassingTupleWithoutFieldNames_TheoryData_DoesNotTrigger()
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
		public async Task WhenPassingTupleWithoutFieldNames_TheoryDataRow_DoesNotTrigger()
		{
			var source = @"
using System.Collections.Generic;
using Xunit;

public class TestClass {
	public static IList<TheoryDataRow<(int, int)>> TestData = new List<TheoryDataRow<(int, int)>>();

    [MemberData(nameof(TestData))]
    public void TestMethod((int a, int b) x) { }
}";

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
		}

		[Fact]
		public async Task WhenPassingTupleWithDifferentFieldNames_TheoryData_DoesNotTrigger()
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
		public async Task WhenPassingTupleWithDifferentFieldNames_TheoryDataRow_DoesNotTrigger()
		{
			var source = @"
using System.Collections.Generic;
using Xunit;

public class TestClass {
	public static IEnumerable<TheoryDataRow<(int c, int d)>> TestData = new List<TheoryDataRow<(int, int)>>();

    [MemberData(nameof(TestData))]
    public void TestMethod((int a, int b) x) { }
}";

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
		}

		[Fact]
		public async Task WithExtraValueNotCompatibleWithParamsArray_TheoryData_Triggers()
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
					.WithArguments("int", "TestClass.TestData", "_2");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async Task WithExtraValueNotCompatibleWithParamsArray_TheoryDataRow_Triggers()
		{
			var source = @"
using System.Collections.Generic;
using Xunit;

public class TestClass {
	public static IEnumerable<TheoryDataRow<int, string, int>> TestData = new List<TheoryDataRow<int, string, int>>();

    [MemberData(nameof(TestData))]
    public void PuzzleOne(int _1, params string[] _2) { }
}";

			var expected =
				Verify
					.Diagnostic("xUnit1039")
					.WithSpan(9, 42, 9, 50)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("int", "TestClass.TestData", "_2");

			await Verify.VerifyAnalyzerV3(source, expected);
		}

		[Theory]
		[MemberData(nameof(TypeWithMemberSyntaxAndArgs), DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataMemberWithIncompatibleTypeParameters_Triggers(
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
					.WithArguments(type, "TestClass.TestData", "f");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(TypeWithMemberSyntaxAndArgs), DisableDiscoveryEnumeration = true)]
		public async Task ValidTheoryDataRowMemberWithIncompatibleTypeParameters_Triggers(
			(string syntax, string args) member,
			string type)
		{
			var source = $@"
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static IList<TheoryDataRow<{type}>> TestData{member.syntax}new List<TheoryDataRow<{type}>>();

    [MemberData(nameof(TestData){member.args})]
    public void TestMethod(string f) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1039")
					.WithSpan(9, 28, 9, 34)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments(type, "TestClass.TestData", "f");

			await Verify.VerifyAnalyzerV3(source, expected);
		}
	}

	public class X1040_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability
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
		public async Task ValidTheoryDataMemberWithMismatchedNullability_Triggers(
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
					.WithArguments("string?", "TestClass.TestData", "f");

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}

		[Theory]
		[MemberData(nameof(MemberSyntaxAndArgs))]
		public async Task ValidTheoryDataRowMemberWithMismatchedNullability_Triggers(
			string memberSyntax,
			string memberArgs)
		{
			var source = $@"
#nullable enable

using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static IEnumerable<TheoryDataRow<string?>> TestData{memberSyntax}new List<TheoryDataRow<string?>>();

    [MemberData(nameof(TestData){memberArgs})]
    public void TestMethod(string f) {{ }}
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1040")
					.WithSpan(11, 28, 11, 34)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("string?", "TestClass.TestData", "f");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
		}
	}

	public class X1042_MemberDataTheoryDataIsRecommendedForStronglyTypedAnalysis
	{
		[Fact]
		public async Task TheoryData_DoesNotTrigger()
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
		public async Task MatrixTheoryData_DoesNotTrigger()
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
		[InlineData("IEnumerable<TheoryDataRow<int>>")]
		[InlineData("IAsyncEnumerable<TheoryDataRow<int>>")]
		[InlineData("List<TheoryDataRow<int>>")]
		[InlineData("TheoryDataRow<int>[]")]
		public async Task GenericTheoryDataRow_DoesNotTrigger(string memberType)
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
		public async Task ValidTypesWhichAreNotTheoryData_Trigger(string memberType)
		{
			var source = $@"
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    public static {memberType} Data;

    [MemberData(nameof(Data))]
    public void TestMethod(int _) {{ }}
}}";

			var expectedV2 =
				Verify
					.Diagnostic("xUnit1042")
					.WithSpan(8, 6, 8, 30)
					.WithSeverity(DiagnosticSeverity.Info)
					.WithArguments("TheoryData<>");
			var expectedV3 =
				Verify
					.Diagnostic("xUnit1042")
					.WithSpan(8, 6, 8, 30)
					.WithSeverity(DiagnosticSeverity.Info)
					.WithArguments("TheoryData<> or IEnumerable<TheoryDataRow<>>");

			await Verify.VerifyAnalyzerV2(source, expectedV2);
			await Verify.VerifyAnalyzerV3(source, expectedV3);
		}

		// For v2, we test for xUnit1019 above, since it's incompatible rather than "compatible,
		// but you could do better".
		[Theory]
		[InlineData("IAsyncEnumerable<object[]>")]
		[InlineData("Task<IEnumerable<object[]>>")]
		[InlineData("ValueTask<List<object[]>>")]
		[InlineData("IEnumerable<TheoryDataRow>")]
		[InlineData("IAsyncEnumerable<TheoryDataRow>")]
		[InlineData("Task<IEnumerable<TheoryDataRow>>")]
		[InlineData("Task<IAsyncEnumerable<TheoryDataRow>>")]
		[InlineData("ValueTask<List<TheoryDataRow>>")]
		[InlineData("IEnumerable<ITheoryDataRow>")]
		[InlineData("Task<IEnumerable<ITheoryDataRow>>")]
		[InlineData("Task<IAsyncEnumerable<ITheoryDataRow>>")]
		[InlineData("ValueTask<EnumerableOfITheoryDataRow>")]
		public async Task ValidTypesWhichAreNotTheoryDataOrGenericTheoryDataRow_TriggersInV3(string memberType)
		{
			var source = $@"
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class TestClass {{
    public static {memberType} Data;

    [MemberData(nameof(Data))]
    public void TestMethod(int _) {{ }}
}}

public class EnumerableOfITheoryDataRow : IEnumerable<ITheoryDataRow>
{{
    public IEnumerator<ITheoryDataRow> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}}";

			var expectedV2 =
				Verify
					.Diagnostic("xUnit1042")
					.WithSpan(10, 6, 10, 30)
					.WithSeverity(DiagnosticSeverity.Info)
					.WithArguments("TheoryData<> or IEnumerable<TheoryDataRow<>>");

			await Verify.VerifyAnalyzerV3(source, expectedV2);
		}
	}
}
