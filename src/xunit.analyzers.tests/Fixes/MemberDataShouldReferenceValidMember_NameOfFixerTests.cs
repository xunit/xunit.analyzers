using Xunit;
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
    public static IEnumerable<object[]> DataSource = Array.Empty<object[]>();

    [Theory]
    [MemberData({|xUnit1014:""DataSource""|})]
    public void TestMethod(int a) { }
}";

		var after = @"
using System;
using System.Collections.Generic;
using Xunit;

public class TestClass {
    public static IEnumerable<object[]> DataSource = Array.Empty<object[]>();

    [Theory]
    [MemberData(nameof(DataSource))]
    public void TestMethod(int a) { }
}";

		await Verify.VerifyCodeFixAsync(before, after);
	}
}
