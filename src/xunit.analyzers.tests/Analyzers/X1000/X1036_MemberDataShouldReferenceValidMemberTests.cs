using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1036_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData(int n) => new TheoryData<int> { n };

				[MemberData(nameof(TestData), 1)]
				public void TestMethod1(int _) { }

				[MemberData(nameof(TestData), new object[] { 1 })]
				public void TestMethod2(int _) { }

				[MemberData(nameof(TestData), 1, {|#0:2|})]
				public void TestMethod3(int _) { }

				[MemberData(nameof(TestData), new object[] { 1, {|#1:2|} })]
				public void TestMethod4(int _) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1036").WithLocation(0).WithArguments("2"),
			Verify.Diagnostic("xUnit1036").WithLocation(1).WithArguments("2"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
