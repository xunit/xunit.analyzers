using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_VisibilityFixerTests
{
	[Fact]
	public async Task FixAll_SetsPublicModifierOnMultipleMembers()
	{
		var before = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				static TheoryData<int> TestData1 => null;
				internal static TheoryData<string> TestData2 => null;

				[Theory]
				[{|xUnit1016:MemberData(nameof(TestData1))|}]
				public void TestMethod1(int x) { }

				[Theory]
				[{|xUnit1016:MemberData(nameof(TestData2))|}]
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

		await Verify.VerifyCodeFixFixAll(before, after, MemberDataShouldReferenceValidMember_VisibilityFixer.Key_MakeMemberPublic);
	}

	[Theory]
	[InlineData("")]
	[InlineData("protected ")]
	[InlineData("internal ")]
	public async Task SetsPublicModifier(string badModifier)
	{
		var before = string.Format(/* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {{
				{0}static TheoryData<int> TestData => null;

				[Theory]
				[{{|xUnit1016:MemberData(nameof(TestData))|}}]
				public void TestMethod(int x) {{ }}
			}}
			""", badModifier);
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

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_VisibilityFixer.Key_MakeMemberPublic);
	}
}
