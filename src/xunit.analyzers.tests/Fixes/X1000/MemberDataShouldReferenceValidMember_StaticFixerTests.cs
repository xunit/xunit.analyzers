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
    public IEnumerable<object[]> TestData => null;

    [Theory]
    [{|xUnit1017:MemberData(nameof(TestData))|}]
    public void TestMethod(int x) { }
}";

		var after = @"
using System.Collections.Generic;
using Xunit;

public class TestClass {
    public static IEnumerable<object[]> TestData => null;

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestMethod(int x) { }
}";

		await Verify.VerifyCodeFixAsyncV2(before, after, MemberDataShouldReferenceValidMember_StaticFixer.Key_MakeMemberStatic);
	}
}
