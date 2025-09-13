using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_NameOfFixerTests
{
	[Fact]
	public async Task ConvertStringToNameOf()
	{
		var before = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static TheoryData<int> DataSource = new TheoryData<int>();

				[Theory]
				[MemberData({|xUnit1014:"DataSource"|})]
				public void TestMethod(int a) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static TheoryData<int> DataSource = new TheoryData<int>();

				[Theory]
				[MemberData(nameof(DataSource))]
				public void TestMethod(int a) { }
			}
			""";

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_NameOfFixer.Key_UseNameof);
	}
}
