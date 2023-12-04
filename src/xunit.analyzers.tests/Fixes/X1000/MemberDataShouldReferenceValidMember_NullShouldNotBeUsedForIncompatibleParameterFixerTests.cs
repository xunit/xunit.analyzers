using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_NullShouldNotBeUsedForIncompatibleParameterFixerTests
{
	[Fact]
	public async void MakesParameterNullable()
	{
		var before = @"
using Xunit;

public class TestClass {
    public static TheoryData<int> TestData(int n, int k) => new TheoryData<int>();

    [Theory]
    [MemberData(nameof(TestData), 42, {|xUnit1034:null|})]
    public void TestMethod(int a) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    public static TheoryData<int> TestData(int n, int? k) => new TheoryData<int>();

    [Theory]
    [MemberData(nameof(TestData), 42, null)]
    public void TestMethod(int a) { }
}";

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_NullShouldNotBeUsedForIncompatibleParameterFixer.Key_MakeParameterNullable);
	}

	[Fact]
	public async void MakesReferenceParameterNullable()
	{
		var before = @"
#nullable enable

using Xunit;

public class TestClass {
    public static TheoryData<int> TestData(int n, string k) => new TheoryData<int> { n };

    [Theory]
    [MemberData(nameof(TestData), 42, {|xUnit1034:null|})]
    public void TestMethod(int a) { }
}";

		var after = @"
#nullable enable

using Xunit;

public class TestClass {
    public static TheoryData<int> TestData(int n, string? k) => new TheoryData<int> { n };

    [Theory]
    [MemberData(nameof(TestData), 42, null)]
    public void TestMethod(int a) { }
}";

		await Verify.VerifyCodeFix(LanguageVersion.CSharp8, before, after, MemberDataShouldReferenceValidMember_NullShouldNotBeUsedForIncompatibleParameterFixer.Key_MakeParameterNullable);
	}
}
