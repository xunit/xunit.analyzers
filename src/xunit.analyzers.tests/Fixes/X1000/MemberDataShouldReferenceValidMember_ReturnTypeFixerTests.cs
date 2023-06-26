using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_ReturnTypeFixerTests
{
	[Fact]
	public async void ChangesReturnType()
	{
		var before = @"
using System.Collections.Generic;
using Xunit;

public class TestClass {
    public static IEnumerable<object> Data => null;

    [Theory]
    [{|xUnit1019:MemberData(nameof(Data))|}]
    public void TestMethod(int a) { }
}";

		var after = @"
using System.Collections.Generic;
using Xunit;

public class TestClass {
    public static IEnumerable<object[]> Data => null;

    [Theory]
    [MemberData(nameof(Data))]
    public void TestMethod(int a) { }
}";

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_ReturnTypeFixer.Key_ChangeMemberReturnType);
	}
}
