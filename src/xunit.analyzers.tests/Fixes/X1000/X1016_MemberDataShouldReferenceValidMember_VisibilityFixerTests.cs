using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1016_MemberDataShouldReferenceValidMember_VisibilityFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				static TheoryData<int> TestData1 => null;
				internal static TheoryData<string> TestData2 => null;
				protected static TheoryData<double> TestData3 => null;

				[Theory]
				[{|xUnit1016:MemberData(nameof(TestData1))|}]
				public void TestMethod1(int x) { }

				[Theory]
				[{|xUnit1016:MemberData(nameof(TestData2))|}]
				public void TestMethod2(string x) { }

				[Theory]
				[{|xUnit1016:MemberData(nameof(TestData3))|}]
				public void TestMethod3(double x) { }
			}
			""";
#if ROSLYN_LATEST
		var after = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData1 => null;
				public static TheoryData<string> TestData2 => null;
				public static TheoryData<double> TestData3 => null;

				[Theory]
				[MemberData(nameof(TestData1))]
				public void TestMethod1(int x) { }

				[Theory]
				[MemberData(nameof(TestData2))]
				public void TestMethod2(string x) { }

				[Theory]
				[MemberData(nameof(TestData3))]
				public void TestMethod3(double x) { }
			}
			""";
#else
		// Roslyn 3.11 inserts a blank line after a member whose accessibility modifiers are changed
		var after = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData1 => null;

				public static TheoryData<string> TestData2 => null;

				public static TheoryData<double> TestData3 => null;

				[Theory]
				[MemberData(nameof(TestData1))]
				public void TestMethod1(int x) { }

				[Theory]
				[MemberData(nameof(TestData2))]
				public void TestMethod2(string x) { }

				[Theory]
				[MemberData(nameof(TestData3))]
				public void TestMethod3(double x) { }
			}
			""";
#endif

		await Verify.VerifyCodeFixFixAll(before, after, MemberDataShouldReferenceValidMember_VisibilityFixer.Key_MakeMemberPublic);
	}
}
