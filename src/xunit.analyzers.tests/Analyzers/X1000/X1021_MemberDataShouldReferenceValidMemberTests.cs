using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1021_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			#pragma warning disable xUnit1053

			using Xunit;

			public class TestClassBase {
				public static TheoryData<int> BaseTestData(int n) => new TheoryData<int> { n };
			}

			public class TestClass : TestClassBase {
				private static void TestData() { }

				public static TheoryData<int> SingleData(int n) => new TheoryData<int> { n };

				[MemberData(nameof(SingleData), 1)]
				[MemberData(nameof(SingleData), new object[] { 1 })]
				public void TestMethod1(int n) { }

				public static TheoryData<int> ParamsData(params int[] n) => new TheoryData<int> { n[0] };

				[MemberData(nameof(ParamsData), 1, 2)]
				[MemberData(nameof(ParamsData), new object[] { 1, 2 })]
				public void TestMethod2(int n) { }

				[MemberData(nameof(BaseTestData), 1)]
				[MemberData(nameof(BaseTestData), new object[] { 1 })]
				public void TestMethod3(int n) { }

				public static TheoryData<int> FieldData;

				[MemberData(nameof(FieldData), {|xUnit1021:'a', 123|})]
				public void TestMethod4a(int _) { }

				[MemberData(nameof(FieldData), {|xUnit1021:new object[] { 'a', 123 }|})]
				public void TestMethod4b(int _) { }

				public static TheoryData<int> PropertyData { get; set; }

				[MemberData(nameof(PropertyData), {|xUnit1021:'a', 123|})]
				public void TestMethod5a(int _) { }

				[MemberData(nameof(PropertyData), {|xUnit1021:new object[] { 'a', 123 }|})]
				public void TestMethod5b(int _) { }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
