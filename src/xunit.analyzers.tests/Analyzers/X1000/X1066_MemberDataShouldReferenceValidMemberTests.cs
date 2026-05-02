using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1066_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			#pragma warning disable xUnit1042
			#pragma warning disable xUnit1067

			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static IEnumerable<object[]> DataMethod(params object[] a) => new object[][] { a };

				[Theory]
				[{|#0:MemberData(nameof(DataMethod))|}]
				public void TestMethod(string _) { }
			}
			""";

		await Verify.VerifyAnalyzerNonAot(source);
#if NETCOREAPP && ROSLYN_LATEST
		var expectedAot = Verify.Diagnostic("xUnit1066").WithLocation(0).WithArguments("a", "TestClass", "DataMethod");

		await Verify.VerifyAnalyzerV3Aot(source, expectedAot);
#endif
	}
}
