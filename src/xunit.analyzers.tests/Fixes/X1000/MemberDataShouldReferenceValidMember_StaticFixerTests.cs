using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_StaticFixerTests
{
	[Fact]
	public async Task FixAll_MarksMultipleDataMembersAsStatic()
	{
		var before = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public TheoryData<int> TestData1 => null;
				public TheoryData<string> TestData2 => null;

				[Theory]
				[{|xUnit1017:MemberData(nameof(TestData1))|}]
				public void TestMethod1(int x) { }

				[Theory]
				[{|xUnit1017:MemberData(nameof(TestData2))|}]
				public void TestMethod2(string x) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData1 => null;
				public static TheoryData<string> TestData2 => null;

				[Theory]
				[MemberData(nameof(TestData1))]
				public void TestMethod1(int x) { }

				[Theory]
				[MemberData(nameof(TestData2))]
				public void TestMethod2(string x) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, MemberDataShouldReferenceValidMember_StaticFixer.Key_MakeMemberStatic);
	}

	[Fact]
	public async Task MarksDataMemberAsStatic()
	{
		var before = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public TheoryData<int> TestData => null;

				[Theory]
				[{|xUnit1017:MemberData(nameof(TestData))|}]
				public void TestMethod(int x) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData => null;

				[Theory]
				[MemberData(nameof(TestData))]
				public void TestMethod(int x) { }
			}
			""";

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_StaticFixer.Key_MakeMemberStatic);
	}
}
