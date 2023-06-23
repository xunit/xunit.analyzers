using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_ParamsForNonMethodFixerTests
{
	[Fact]
	public async void RemovesParametersFromNonMethodMemberData()
	{
		var before = @"
using System;
using System.Collections.Generic;
using Xunit;

public class TestClass
{
    public static IEnumerable<object[]> DataSource = Array.Empty<object[]>();

    [Theory]
    [MemberData(nameof(DataSource), {|xUnit1021:""abc"", 123|})]
    public void TestMethod(int a) { }
}";

		var after = @"
using System;
using System.Collections.Generic;
using Xunit;

public class TestClass
{
    public static IEnumerable<object[]> DataSource = Array.Empty<object[]>();

    [Theory]
    [MemberData(nameof(DataSource))]
    public void TestMethod(int a) { }
}";

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}
}
