using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_ReturnTypeFixerTests
{
	[Fact]
	public async void ChangesReturnType_ObjectArray()
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

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_ReturnTypeFixer.Key_ChangeMemberReturnType_ObjectArray);
	}

	[Fact]
	public async void ChangesReturnType_TheoryDataRow()
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
    public static IEnumerable<ITheoryDataRow> Data => null;

    [Theory]
    [MemberData(nameof(Data))]
    public void TestMethod(int a) { }
}";

		await Verify.VerifyCodeFixV3(before, after, MemberDataShouldReferenceValidMember_ReturnTypeFixer.Key_ChangeMemberReturnType_ITheoryDataRow);
	}
}
