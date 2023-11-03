using Microsoft;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMemberTests
{
	static readonly string sharedCode = @"
using System.Collections.Generic;

public partial class TestClass {
    public static IEnumerable<object[]> Data { get; set; }
}

public class OtherClass {
    public static IEnumerable<object[]> OtherData { get; set; }
}";

	[Fact]
	public async void DoesNotFindError_ForNameofOnSameClass()
	{
		var source = @"
public partial class TestClass {
    [Xunit.MemberData(nameof(Data))]
    public void TestMethod() { }
}";

		await Verify.VerifyAnalyzer(new[] { source, sharedCode });
	}

	[Fact]
	public async void DoesNotFindError_ForNameofOnOtherClass()
	{
		var source = @"
public partial class TestClass {
    [Xunit.MemberData(nameof(OtherClass.OtherData), MemberType = typeof(OtherClass))]
    public void TestMethod() { }
}";

		await Verify.VerifyAnalyzer(new[] { source, sharedCode });
	}

	[Fact]
	public async void FindsError_ForStringReferenceOnSameClass()
	{
		var source = @"
public partial class TestClass {
    [Xunit.MemberData(""Data"")]
    public void TestMethod() { }
}";
		var expected =
			Verify
				.Diagnostic("xUnit1014")
				.WithSpan(3, 23, 3, 29)
				.WithArguments("Data", "TestClass");

		await Verify.VerifyAnalyzer(new[] { source, sharedCode }, expected);
	}

	[Fact]
	public async void FindsError_ForStringReferenceOnOtherClass()
	{
		var source = @"
public partial class TestClass {
    [Xunit.MemberData(""OtherData"", MemberType = typeof(OtherClass))]
    public void TestMethod() { }
}";
		var expected =
			Verify
				.Diagnostic("xUnit1014")
				.WithSpan(3, 23, 3, 34)
				.WithArguments("OtherData", "OtherClass");

		await Verify.VerifyAnalyzer(new[] { source, sharedCode }, expected);
	}

	[Fact]
	public async void FindsError_ForInvalidNameString()
	{
		var source = @"
public class TestClass {
    [Xunit.MemberData(""BogusName"")]
    public void TestMethod() { }
}";
		var expected =
			Verify
				.Diagnostic("xUnit1015")
				.WithSpan(3, 6, 3, 35)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("BogusName", "TestClass");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void FindsError_ForInvalidNameString_UsingMemberType()
	{
		var source = @"
public class TestClass {
    [Xunit.MemberData(""BogusName"", MemberType = typeof(TestClass))]
    public void TestMethod() { }
}";
		var expected =
			Verify
				.Diagnostic("xUnit1015")
				.WithSpan(3, 6, 3, 67)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("BogusName", "TestClass");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void FindsError_ForInvalidNameString_UsingMemberTypeWithOtherType()
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
	public async void FindsError_ForValidNameofExpression_UsingMemberTypeSpecifyingOtherType()
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

	[Theory]
	[InlineData("")]
	[InlineData("private")]
	[InlineData("protected")]
	[InlineData("internal")]
	[InlineData("protected internal")]
	public async void FindsError_ForNonPublicMember(string accessModifier)
	{
		var source = $@"
public class TestClass {{
    {accessModifier} static System.Collections.Generic.IEnumerable<object[]> Data = null;

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod() {{ }}
}}";
		var expected =
			Verify
				.Diagnostic("xUnit1016")
				.WithSpan(5, 6, 5, 36)
				.WithSeverity(DiagnosticSeverity.Error);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void DoesNotFindError_ForPublicMember()
	{
		var source = @"
public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> Data = null;

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod() { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("{|xUnit1014:\"Data\"|}")]
	[InlineData("DataNameConst")]
	[InlineData("DataNameofConst")]
	[InlineData("nameof(Data)")]
	[InlineData("nameof(TestClass.Data)")]
	[InlineData("OtherClass.Data")]
	[InlineData("nameof(OtherClass.Data)")]
	public async void FindsError_ForNameExpressions(string dataNameExpression)
	{
		TestFileMarkupParser.GetPositionsAndSpans(dataNameExpression, out var parsedDataNameExpression, out _, out _);
		var dataNameExpressionLength = parsedDataNameExpression.Length;

		var source1 = $@"
public class TestClass {{
    const string DataNameConst = ""Data"";
    const string DataNameofConst = nameof(Data);

    private static System.Collections.Generic.IEnumerable<object[]> Data = null;

    [Xunit.MemberData({dataNameExpression})]
    public void TestMethod() {{ }}
}}";
		var source2 = @"public static class OtherClass { public const string Data = ""Data""; }";
		var expected =
			Verify
				.Diagnostic("xUnit1016")
				.WithSpan(8, 6, 8, 24 + dataNameExpressionLength)
				.WithSeverity(DiagnosticSeverity.Error);

		await Verify.VerifyAnalyzer(new[] { source1, source2 }, expected);
	}

	[Fact]
	public async void FindsError_ForInstanceMember()
	{
		var source = @"
public class TestClass {
    public System.Collections.Generic.IEnumerable<object[]> Data = null;

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod() { }
}";
		var expected =
			Verify
				.Diagnostic("xUnit1017")
				.WithSpan(5, 6, 5, 36)
				.WithSeverity(DiagnosticSeverity.Error);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void DoesNotFindError_ForStaticMember()
	{
		var source = @"
public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> Data = null;

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod() { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("public delegate System.Collections.Generic.IEnumerable<object[]> Data();")]
	[InlineData("public static class Data { }")]
	[InlineData("public static event System.EventHandler Data;")]
	public async void FindsError_ForInvalidMemberKind(string member)
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

	[Theory]
	[InlineData("public static System.Collections.Generic.IEnumerable<object[]> Data;")]
	[InlineData("public static System.Collections.Generic.IEnumerable<object[]> Data { get; set; }")]
	[InlineData("public static System.Collections.Generic.IEnumerable<object[]> Data() { return null; }")]
	public async void DoesNotFindError_ForValidMemberKind(string member)
	{
		var source = $@"
public class TestClass {{
    {member}

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod() {{ }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[InlineData("System.Collections.Generic.IEnumerable<object>")]
	[InlineData("object[]")]
	[InlineData("object")]
	[InlineData("System.Tuple<string, int>")]
	[InlineData("System.Tuple<string, int>[]")]
	public async void FindsError_ForInvalidMemberType(string memberType)
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

	[Theory]
	[InlineData("System.Collections.Generic.IEnumerable<object[]>")]
	[InlineData("System.Collections.Generic.List<object[]>")]
	[InlineData("Xunit.TheoryData<int>")]
	public async void DoesNotFindError_ForCompatibleMemberType(string memberType)
	{
		var source = $@"
public class TestClass {{
    public static {memberType} Data;

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod() {{ }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotFindError_ForITheoryDataRow_V3()
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

	[Fact]
	public async void FindsError_ForMemberPropertyWithoutGetter()
	{
		var source = @"
public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> Data { set { } }

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod() { }
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
	public async void FindsError_ForMemberPropertyWithNonPublicGetter(string visibility)
	{
		var source = $@"
public class TestClass {{
    public static System.Collections.Generic.IEnumerable<object[]> Data {{ {visibility} get {{ return null; }} set {{ }} }}

    [Xunit.MemberData(nameof(Data))]
    public void TestMethod() {{ }}
}}";
		var expected =
			Verify
				.Diagnostic("xUnit1020")
				.WithSpan(5, 6, 5, 36)
				.WithSeverity(DiagnosticSeverity.Error);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData("'a', 123")]
	[InlineData("new object[] {{ 'a', 123 }}")]
	[InlineData("{0}: new object[] {{ 'a', 123 }}")]
	public async void FindsWarning_ForMemberDataParametersForFieldMember(string paramsArgument)
	{
		var sourceTemplate = @"
public class TestClass {{
    public static System.Collections.Generic.IEnumerable<object[]> Data;

    [Xunit.MemberData(nameof(Data), {0}, MemberType = typeof(TestClass))]
    public void TestMethod() {{ }}
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
	public async void FindsWarning_ForMemberDataParametersForPropertyMember(string paramsArgument)
	{
		var sourceTemplate = @"
public class TestClass {{
    public static System.Collections.Generic.IEnumerable<object[]> Data {{ get; set; }}

    [Xunit.MemberData(nameof(Data), {0}, MemberType = typeof(TestClass))]
    public void TestMethod() {{ }}
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

	[Fact]
	public async void DoesNotFindWarning_ForMemberDataAttributeWithNamedParameter()
	{
		var source = @"
public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> Data;

    [Xunit.MemberData(nameof(Data), MemberType = typeof(TestClass))]
    public void TestMethod() { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotFindWarning_IfHasValidMember()
	{
		var source = @"
public class TestClass {
    private static void TestData() { }

    public static System.Collections.Generic.IEnumerable<object[]> TestData(int n) { yield return new object[] { n }; }

    [Xunit.MemberData(nameof(TestData), new object[] { 1 })]
    public void TestMethod(int n) { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotFindWarning_IfHasValidMemberWithParams()
	{
		var source = @"
public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> TestData(params int[] n) { yield return new object[] { n[0] }; }

    [Xunit.MemberData(nameof(TestData), new object[] { 1, 2 })]
    public void TestMethod(int n) { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void FindWarning_IfHasValidMemberWithIncorrectArgumentTypes()
	{
		var source = @"
public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> TestData(string n) { yield return new object[] { n }; }

    [Xunit.MemberData(nameof(TestData), new object[] { 1 })]
    public void TestMethod(int n) { }
}"
		;

		DiagnosticResult[] expected =
		{
			Verify
				.Diagnostic("xUnit1035")
				.WithSpan(5, 56, 5, 57)
				.WithArguments("n", "string")
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void FindWarning_IfHasValidMemberWithIncorrectArgumentTypesParams()
	{
		var source = @"
public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> TestData(params int[] n) { yield return new object[] { n }; }

    [Xunit.MemberData(nameof(TestData), new object[] { 1, ""bob"" })]
    public void TestMethod(int n) { }
}"
		;

		DiagnosticResult[] expected =
		{
			Verify
				.Diagnostic("xUnit1035")
				.WithSpan(5, 59, 5, 64)
				.WithArguments("n", "int")
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void FindWarning_IfHasValidMemberWithIncorrectArgumentCount()
	{
		var source = @"
public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> TestData(int n) { yield return new object[] { n }; }

    [Xunit.MemberData(nameof(TestData), new object[] { 1, 2 })]
    public void TestMethod(int n) { }
}"
		;

		DiagnosticResult[] expected =
		{
			Verify
				.Diagnostic("xUnit1036")
				.WithSpan(5, 59, 5, 60)
				.WithArguments("2")
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void FindWarning_IfHasValidMemberWithIncorrectParamsArgumentCount()
	{
		var source = @"
public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> TestData(int n) { yield return new object[] { n }; }

    [Xunit.MemberData(nameof(TestData), 1, 2)]
    public void TestMethod(int n) { }
}"
		;

		DiagnosticResult[] expected =
		{
			Verify
				.Diagnostic("xUnit1036")
				.WithSpan(5, 44, 5, 45)
				.WithArguments("2")
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async void DoesNotFindWarning_IfHasValidListMember()
	{
		var source = @"
public class TestClass {
    private static void TestData() { }

    public static System.Collections.Generic.List<object[]> TestData(int n) { return new System.Collections.Generic.List<object[]> { new object[] { n } }; }

    [Xunit.MemberData(nameof(TestData), new object[] { 1 })]
    public void TestMethod(int n) { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotFindWarning_IfHasValidNonNullableListMember_InNullableContext()
	{
		var source = @"
#nullable enable
public class TestClass {
    public static System.Collections.Generic.List<object[]> TestData(int n) { return new System.Collections.Generic.List<object[]> { new object[] { n } }; }

    [Xunit.MemberData(nameof(TestData), new object[] { 1 })]
    public void TestMethod(int n) { }
}
#nullable restore
";

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
	}

	[Fact]
	public async void FindWarning_IfPassingNullToNonNullableMethodParameter_InNullableContext()
	{
		var source = @$"
#nullable enable
public class TestClass {{
    public static System.Collections.Generic.List<object[]> TestData(int n, string f) {{ return new System.Collections.Generic.List<object[]?> {{ new object[] {{ f }} }}; }}

    [Xunit.MemberData(nameof(TestData), new object[] {{ null, null }})]
    public void TestMethod(string n) {{ }}
}}
#nullable restore";

		DiagnosticResult[] expected =
		{
			Verify
				.Diagnostic("xUnit1034")
				.WithSpan(6, 56, 6, 60)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("n", "int"),
			Verify
				.Diagnostic("xUnit1034")
				.WithSpan(6, 62, 6, 66)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("f", "string"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
	}

	[Fact]
	public async void DoesNotFindWarning_IfHasValidMemberInBaseClass()
	{
		var source = @"
public class TestClassBase {
    public static System.Collections.Generic.IEnumerable<object[]> TestData(int n) {
        yield return new object[] { n };
    }
}

public class TestClass : TestClassBase {
    private static void TestData() { }

    [Xunit.MemberData(nameof(TestData), new object[] { 1 })]
    public void TestMethod(int n) { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void DoesNotFindWarning_IfHasValidTheoryDataMember()
	{
		var source = @"
public class TestClass {
    public static Xunit.TheoryData<int> TestData(int n) => new();

    [Xunit.MemberData(nameof(TestData), new object[] { 1 })]
    public void TestMethod(int n) { }
}";

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp10, source);
	}

	[Fact]
	public async void DoesNotFindWarning_IfHasValidTheoryDataMemberWithOptionalParameters()
	{
		var source = @"
public class TestClass {
    public static Xunit.TheoryData<int> TestData(int n) => new();

    [Xunit.MemberData(nameof(TestData), new object[] { 1 })]
    public void TestMethod(int n, int a = 0) { }
}";

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp10, source);
	}

	[Fact]
	public async void FindWarning_IfHasValidTheoryDataMemberWithTooManyTypeParameters()
	{
		var source = @"
public class TestClass {
    public static Xunit.TheoryData<int, string> TestData(int n) => new();

    [Xunit.MemberData(nameof(TestData), new object[] { 1 })]
    public void TestMethod(int n) { }
}";

		DiagnosticResult[] expected =
		{
			Verify
				.Diagnostic("xUnit1037")
				.WithSpan(5, 6, 5, 60)
				.WithSeverity(DiagnosticSeverity.Error)
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp10, source, expected);
	}

	[Fact]
	public async void FindWarning_IfHasValidTheoryDataMemberWithNotEnoughTypeParameters()
	{
		var source = @"
public class TestClass {
    public static Xunit.TheoryData<int> TestData(int n) => new();

    [Xunit.MemberData(nameof(TestData), new object[] { 1 })]
    public void TestMethod(int n, string f) { }
}";

		DiagnosticResult[] expected =
		{
			Verify
				.Diagnostic("xUnit1037")
				.WithSpan(5, 6, 5, 60)
				.WithSeverity(DiagnosticSeverity.Error)
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp10, source, expected);
	}

	[Theory]
	[InlineData("int")]
	[InlineData("System.Exception")]
	public async void FindWarning_IfHasValidTheoryDataMemberWithIncompatibleTypeParameters(string type)
	{
		var source = @$"
public class TestClass {{
    public static Xunit.TheoryData<{type}> TestData(int n) => new();

    [Xunit.MemberData(nameof(TestData), new object[] {{ 1 }})]
    public void TestMethod(string f) {{ }}
}}";

		DiagnosticResult[] expected =
		{
			Verify
				.Diagnostic("xUnit1038")
				.WithSpan(6, 28, 6, 34)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments(type, "f")
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp10, source, expected);
	}

	[Fact]
	public async void FindWarning_IfHasValidTheoryDataMemberWithMismatchedNullability()
	{
		var source = @"
#nullable enable
public class TestClass {
    public static Xunit.TheoryData<int?, string?> TestData(int n) => new();

    [Xunit.MemberData(nameof(TestData), new object[] { 1 })]
    public void TestMethod(int n, string f) { }
}
#nullable restore";

		DiagnosticResult[] expected =
		{
			Verify
				.Diagnostic("xUnit1038")
				.WithSpan(7, 28, 7, 31)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("int?", "n"),
			Verify
				.Diagnostic("xUnit1039")
				.WithSpan(7, 35, 7, 41)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("string?", "f")
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp10, source, expected);
	}
}
