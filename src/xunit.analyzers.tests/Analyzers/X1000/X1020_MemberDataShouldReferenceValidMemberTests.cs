using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1020_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			#pragma warning disable xUnit1053

			using Xunit;

			public class TestClass {
				public static TheoryData<int> PublicWithGetter => new();
				public static TheoryData<int> PublicWithoutGetter { set { } }
				public static TheoryData<int> ProtectedGetter { protected get { return null; } set { } }
				public static TheoryData<int> InternalGetter { internal get { return null; } set { } }
				public static TheoryData<int> PrivateGetter { private get { return null; } set { } }

				[MemberData(nameof(PublicWithGetter))]
				[{|xUnit1020:MemberData(nameof(PublicWithoutGetter))|}]
				[{|xUnit1020:MemberData(nameof(ProtectedGetter))|}]
				[{|xUnit1020:MemberData(nameof(InternalGetter))|}]
				[{|xUnit1020:MemberData(nameof(PrivateGetter))|}]
				public void TestMethod(int _) { }
			}
			""";

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source);
	}
}
