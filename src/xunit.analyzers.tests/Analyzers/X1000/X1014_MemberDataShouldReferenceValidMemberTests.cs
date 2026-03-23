using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1014_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			#pragma warning disable xUnit1053

			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static TheoryData<int> Data { get; set; }

				[MemberData(nameof(Data))]
				[MemberData(nameof(OtherClass.OtherData), MemberType = typeof(OtherClass))]
				public void TestMethod1(int _) { }

				[MemberData({|#0:"Data"|})]
				[MemberData({|#1:"OtherData"|}, MemberType = typeof(OtherClass))]
				public void TestMethod2(int _) { }
			}

			public class OtherClass {
				public static TheoryData<int> OtherData { get; set; }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1014").WithLocation(0).WithArguments("Data", "TestClass"),
			Verify.Diagnostic("xUnit1014").WithLocation(1).WithArguments("OtherData", "OtherClass"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
