using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1016_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public static TheoryData<int> PublicData = null;

				[MemberData(nameof(PublicData))]
				public void TestMethod1a(int _) { }

				const string PrivateDataNameConst = "PrivateData";
				const string PrivateDataNameofConst = nameof(PrivateData);
				private static TheoryData<int> PrivateData = null;

				[{|xUnit1016:MemberData(nameof(PrivateData))|}]
				public void TestMethod2a(int _) { }

				[{|xUnit1016:MemberData(PrivateDataNameConst)|}]
				public void TestMethod2b(int _) { }

				[{|xUnit1016:MemberData(PrivateDataNameofConst)|}]
				public void TestMethod2c(int _) { }

				internal static TheoryData<int> InternalData = null;

				[{|xUnit1016:MemberData(nameof(InternalData))|}]
				public void TestMethod3(int _) { }

				protected static TheoryData<int> ProtectedData = null;

				[{|xUnit1016:MemberData(nameof(ProtectedData))|}]
				public void TestMethod4(int _) { }

				protected internal static TheoryData<int> ProtectedInternalData = null;

				[{|xUnit1016:MemberData(nameof(ProtectedInternalData))|}]
				public void TestMethod5(int _) { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
