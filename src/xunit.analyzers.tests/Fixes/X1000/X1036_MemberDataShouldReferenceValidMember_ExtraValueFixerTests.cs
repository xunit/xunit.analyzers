using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1036_MemberDataShouldReferenceValidMember_ExtraValueFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData1(int n) => new TheoryData<int>();
				public static TheoryData<int> TestData2(int p) => new TheoryData<int>();

				[Theory]
				[MemberData(nameof(TestData1), 42, {|xUnit1036:21.12|})]
				public void TestMethod1(int a) { }

				[Theory]
				[MemberData(nameof(TestData2), 42, {|xUnit1036:99.9|})]
				public void TestMethod2(int a) { }
			}
			""";
		var afterRemove = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData1(int n) => new TheoryData<int>();
				public static TheoryData<int> TestData2(int p) => new TheoryData<int>();

				[Theory]
				[MemberData(nameof(TestData1), 42)]
				public void TestMethod1(int a) { }

				[Theory]
				[MemberData(nameof(TestData2), 42)]
				public void TestMethod2(int a) { }
			}
			""";
		var afterAdd = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData1(int n, double p) => new TheoryData<int>();
				public static TheoryData<int> TestData2(int p, double p_2) => new TheoryData<int>();

				[Theory]
				[MemberData(nameof(TestData1), 42, 21.12)]
				public void TestMethod1(int a) { }

				[Theory]
				[MemberData(nameof(TestData2), 42, 99.9)]
				public void TestMethod2(int a) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, afterRemove, MemberDataShouldReferenceValidMember_ExtraValueFixer.Key_RemoveExtraDataValue);
		await Verify.VerifyCodeFixFixAll(before, afterAdd, MemberDataShouldReferenceValidMember_ExtraValueFixer.Key_AddMethodParameter);
	}
}
