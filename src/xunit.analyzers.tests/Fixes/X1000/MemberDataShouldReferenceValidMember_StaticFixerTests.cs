using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_StaticFixerTests
{
	[Fact]
	public async void MarksDataMemberAsStatic()
	{
		var before = @"
using System.Collections.Generic;
using Xunit;

public class TestClass {
    public TheoryData<int> TestData => null;

    [Theory]
    [{|xUnit1017:MemberData(nameof(TestData))|}]
    public void TestMethod(int x) { }
}";

		var after = @"
using System.Collections.Generic;
using Xunit;

public class TestClass {
    public static TheoryData<int> TestData => null;

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestMethod(int x) { }
}";

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_StaticFixer.Key_MakeMemberStatic);
	}
}
