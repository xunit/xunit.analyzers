using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_ExtraValueFixerTests
{
	[Fact]
	public async void RemovesUnusedData()
	{
		var before = @"
using Xunit;

public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> TestData(int n) { yield return new object[] { n }; }

    [Theory]
    [MemberData(nameof(TestData), 42, {|xUnit1036:21.12|})]
    public void TestMethod(int a) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> TestData(int n) { yield return new object[] { n }; }

    [Theory]
    [MemberData(nameof(TestData), 42)]
    public void TestMethod(int a) { }
}";

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_ExtraValueFixer.Key_RemoveExtraDataValue);
	}

	[Theory]
	[InlineData("21.12", "double")]
	[InlineData(@"""Hello world""", "string")]
	public async void AddsParameterWithCorrectType(
		string value,
		string valueType)
	{
		var before = $@"
using Xunit;

public class TestClass {{
    public static System.Collections.Generic.IEnumerable<object[]> TestData(int n) {{ yield return new object[] {{ n }}; }}

    [Theory]
    [MemberData(nameof(TestData), 42, {{|xUnit1036:{value}|}})]
    public void TestMethod(int a) {{ }}
}}";

		var after = $@"
using Xunit;

public class TestClass {{
    public static System.Collections.Generic.IEnumerable<object[]> TestData(int n, {valueType} p) {{ yield return new object[] {{ n }}; }}

    [Theory]
    [MemberData(nameof(TestData), 42, {value})]
    public void TestMethod(int a) {{ }}
}}";

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_ExtraValueFixer.Key_AddMethodParameter);
	}

	[Fact]
	public async void AddsParameterWithNonConflictingName()
	{
		var before = $@"
using Xunit;

public class TestClass {{
    public static System.Collections.Generic.IEnumerable<object[]> TestData(int p) {{ yield return new object[] {{ p }}; }}

    [Theory]
    [MemberData(nameof(TestData), 42, {{|xUnit1036:21.12|}})]
    public void TestMethod(int n) {{ }}
}}";

		var after = $@"
using Xunit;

public class TestClass {{
    public static System.Collections.Generic.IEnumerable<object[]> TestData(int p, double p_2) {{ yield return new object[] {{ p }}; }}

    [Theory]
    [MemberData(nameof(TestData), 42, 21.12)]
    public void TestMethod(int n) {{ }}
}}";

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_ExtraValueFixer.Key_AddMethodParameter);
	}
}
