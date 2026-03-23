using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1015_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source1 = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				[{|#0:MemberData("BogusName")|}]
				public void MissingLocalDataSource_ByName_Triggers() { }

				[{|#1:MemberData("BogusName", MemberType = typeof(TestClass))|}]
				public void MissingLocalDataSource_ByNameAndType_Triggers() { }

				[{|#2:MemberData("BogusName", MemberType = typeof(OtherClass))|}]
				public void MissingRemoteDataSource_ByNameAndType_Triggers() { }

				[{|#3:MemberData(nameof(MissingRemoteDataSource_ByNameofAndType_Triggers), MemberType = typeof(OtherClass))|}]
				public void MissingRemoteDataSource_ByNameofAndType_Triggers() { }
			}

			public abstract class BaseClassWithTestWithoutData
			{
				[Theory]
				[MemberData(nameof(SubClassWithTestData.TestData))]
				public void WhenMemberExistsOnDerivedType_AndBaseTypeIsAbstract_DoesNotTrigger(int x) { }
			}

			public class SubClassWithTestData : BaseClassWithTestWithoutData
			{
				public static IEnumerable<object?[]> TestData()
				{
					yield return new object?[] { 42 };
				}
			}
			""";
		var source2 = /* lang=c#-test */ "public class OtherClass { }";
		var expected = new[] {
			Verify.Diagnostic("xUnit1015").WithLocation(0).WithArguments("BogusName", "TestClass"),
			Verify.Diagnostic("xUnit1015").WithLocation(1).WithArguments("BogusName", "TestClass"),
			Verify.Diagnostic("xUnit1015").WithLocation(2).WithArguments("BogusName", "OtherClass"),
			Verify.Diagnostic("xUnit1015").WithLocation(3).WithArguments("MissingRemoteDataSource_ByNameofAndType_Triggers", "OtherClass"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, [source1, source2], expected);
	}
}
