using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1017_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public static TheoryData<int> StaticData = null;
				public TheoryData<int> NonStaticData = null;

				[MemberData(nameof(StaticData))]
				[{|xUnit1017:MemberData(nameof(NonStaticData))|}]
				public void TestMethod(int _) { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
