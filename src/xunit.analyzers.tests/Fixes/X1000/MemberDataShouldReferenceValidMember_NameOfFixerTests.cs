using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_NameOfFixerTests
{
	[Fact]
	public async void ConvertStringToNameOf()
	{
		var before = @"
using System;
using System.Collections.Generic;
using Xunit;

public class TestClass {
    public static TheoryData<int> DataSource;

    [Theory]
    [MemberData({|xUnit1014:""DataSource""|})]
    public void TestMethod(int a) { }
}";

		var after = @"
using System;
using System.Collections.Generic;
using Xunit;

public class TestClass {
    public static TheoryData<int> DataSource;

    [Theory]
    [MemberData(nameof(DataSource))]
    public void TestMethod(int a) { }
}";

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_NameOfFixer.Key_UseNameof);
	}
}
