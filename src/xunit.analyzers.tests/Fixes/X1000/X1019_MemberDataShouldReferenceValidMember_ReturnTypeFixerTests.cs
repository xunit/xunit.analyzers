using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1019_MemberDataShouldReferenceValidMember_ReturnTypeFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			#pragma warning disable xUnit1042

			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static IEnumerable<object> Data1 => null;
				public static IEnumerable<object> Data2 => null;

				[Theory]
				[{|xUnit1019:MemberData(nameof(Data1))|}]
				public void TestMethod1(int a) { }

				[Theory]
				[{|xUnit1019:MemberData(nameof(Data2))|}]
				public void TestMethod2(int a) { }
			}
			""";
		var afterObjectArray = /* lang=c#-test */ """
			#pragma warning disable xUnit1042

			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static IEnumerable<object[]> Data1 => null;
				public static IEnumerable<object[]> Data2 => null;

				[Theory]
				[MemberData(nameof(Data1))]
				public void TestMethod1(int a) { }

				[Theory]
				[MemberData(nameof(Data2))]
				public void TestMethod2(int a) { }
			}
			""";
		var afterITheoryDataRow = /* lang=c#-test */ """
			#pragma warning disable xUnit1042

			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static IEnumerable<ITheoryDataRow> Data1 => null;
				public static IEnumerable<ITheoryDataRow> Data2 => null;

				[Theory]
				[MemberData(nameof(Data1))]
				public void TestMethod1(int a) { }

				[Theory]
				[MemberData(nameof(Data2))]
				public void TestMethod2(int a) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, afterObjectArray, MemberDataShouldReferenceValidMember_ReturnTypeFixer.Key_ChangeMemberReturnType_ObjectArray);
		await Verify.VerifyCodeFixV3FixAll(before, afterITheoryDataRow, MemberDataShouldReferenceValidMember_ReturnTypeFixer.Key_ChangeMemberReturnType_ITheoryDataRow);
	}
}
