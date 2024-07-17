using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_ExtraValueFixerTests
{
	[Fact]
	public async Task RemovesUnusedData()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    public static TheoryData<int> TestData(int n) => new TheoryData<int>();

			    [Theory]
			    [MemberData(nameof(TestData), 42, {|xUnit1036:21.12|})]
			    public void TestMethod(int a) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    public static TheoryData<int> TestData(int n) => new TheoryData<int>();

			    [Theory]
			    [MemberData(nameof(TestData), 42)]
			    public void TestMethod(int a) { }
			}
			""";

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_ExtraValueFixer.Key_RemoveExtraDataValue);
	}

	[Theory]
	[InlineData("21.12", "double")]
	[InlineData(@"""Hello world""", "string")]
	public async Task AddsParameterWithCorrectType(
		string value,
		string valueType)
	{
		var before = string.Format(/* lang=c#-test */ """
			using Xunit;

			public class TestClass {{
			    public static TheoryData<int> TestData(int n) => new TheoryData<int>();

			    [Theory]
			    [MemberData(nameof(TestData), 42, {{|xUnit1036:{0}|}})]
			    public void TestMethod(int a) {{ }}
			}}
			""", value);
		var after = string.Format(/* lang=c#-test */ """
			using Xunit;

			public class TestClass {{
			    public static TheoryData<int> TestData(int n, {1} p) => new TheoryData<int>();

			    [Theory]
			    [MemberData(nameof(TestData), 42, {0})]
			    public void TestMethod(int a) {{ }}
			}}
			""", value, valueType);

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_ExtraValueFixer.Key_AddMethodParameter);
	}

	[Fact]
	public async Task AddsParameterWithNonConflictingName()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    public static TheoryData<int> TestData(int p) => new TheoryData<int>();

			    [Theory]
			    [MemberData(nameof(TestData), 42, {|xUnit1036:21.12|})]
			    public void TestMethod(int n) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    public static TheoryData<int> TestData(int p, double p_2) => new TheoryData<int>();

			    [Theory]
			    [MemberData(nameof(TestData), 42, 21.12)]
			    public void TestMethod(int n) { }
			}
			""";

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_ExtraValueFixer.Key_AddMethodParameter);
	}
}
