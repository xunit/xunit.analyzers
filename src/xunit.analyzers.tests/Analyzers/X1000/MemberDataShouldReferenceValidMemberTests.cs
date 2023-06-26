using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
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
		var expected =
			Verify
				.Diagnostic("xUnit1019")
				.WithSpan(5, 6, 5, 36)
				.WithSeverity(DiagnosticSeverity.Error)
				.WithArguments("System.Collections.Generic.IEnumerable<object[]>", memberType);

		await Verify.VerifyAnalyzer(source, expected);
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
	[InlineData("new object[] { 'a', 123 }")]
	[InlineData("parameters: new object[] { 'a', 123 }")]
	public async void FindsWarning_ForMemberDataParametersForFieldMember(string paramsArgument)
	{
		var source = $@"
public class TestClass {{
    public static System.Collections.Generic.IEnumerable<object[]> Data;

    [Xunit.MemberData(nameof(Data), {paramsArgument}, MemberType = typeof(TestClass))]
    public void TestMethod() {{ }}
}}";
		var expected =
			Verify
				.Diagnostic("xUnit1021")
				.WithSpan(5, 37, 5, 37 + paramsArgument.Length)
				.WithSeverity(DiagnosticSeverity.Warning);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData("'a', 123")]
	[InlineData("new object[] { 'a', 123 }")]
	[InlineData("parameters: new object[] { 'a', 123 }")]
	public async void FindsWarning_ForMemberDataParametersForPropertyMember(string paramsArgument)
	{
		var source = $@"
public class TestClass {{
    public static System.Collections.Generic.IEnumerable<object[]> Data {{ get; set; }}

    [Xunit.MemberData(nameof(Data), {paramsArgument}, MemberType = typeof(TestClass))]
    public void TestMethod() {{ }}
}}";
		var expected =
			Verify
				.Diagnostic("xUnit1021")
				.WithSpan(5, 37, 5, 37 + paramsArgument.Length)
				.WithSeverity(DiagnosticSeverity.Warning);

		await Verify.VerifyAnalyzer(source, expected);
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
}
