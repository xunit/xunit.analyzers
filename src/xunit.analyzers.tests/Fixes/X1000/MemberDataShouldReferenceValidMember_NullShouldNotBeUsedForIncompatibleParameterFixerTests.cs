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
    public static System.Collections.Generic.IEnumerable<object[]> TestData(int n, int k) { yield return new object[] { n }; }

    [Theory]
    [MemberData(nameof(TestData), 42, {|xUnit1037:null|})]
    public void TestMethod(int a) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> TestData(int n, int? k) { yield return new object[] { n }; }

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
using Xunit;

#nullable enable
public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> TestData(int n, string k) { yield return new object[] { n }; }

    [Theory]
    [MemberData(nameof(TestData), 42, {|xUnit1037:null|})]
    public void TestMethod(int a) { }
#nullable restore
}";

		var after = @"
using Xunit;

#nullable enable
public class TestClass {
    public static System.Collections.Generic.IEnumerable<object[]> TestData(int n, string? k) { yield return new object[] { n }; }

    [Theory]
    [MemberData(nameof(TestData), 42, null)]
    public void TestMethod(int a) { }
#nullable restore
}";

		await Verify.VerifyCodeFix(LanguageVersion.CSharp8, before, after, MemberDataShouldReferenceValidMember_NullShouldNotBeUsedForIncompatibleParameterFixer.Key_MakeParameterNullable);
	}
}
