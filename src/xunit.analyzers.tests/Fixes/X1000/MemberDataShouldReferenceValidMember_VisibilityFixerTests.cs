using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_VisibilityFixerTests
{
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
