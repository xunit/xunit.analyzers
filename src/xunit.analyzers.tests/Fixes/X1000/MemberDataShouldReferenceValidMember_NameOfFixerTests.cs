using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_NameOfFixerTests
{
	[Fact]
	public async Task FixAll_ConvertsAllStringsToNameOf()
	{
		var before = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static TheoryData<int> DataSource1 = new TheoryData<int>();
				public static TheoryData<int> DataSource2 = new TheoryData<int>();

				[Theory]
				[MemberData({|xUnit1014:"DataSource1"|})]
				public void TestMethod1(int a) { }

				[Theory]
				[MemberData({|xUnit1014:"DataSource2"|})]
				public void TestMethod2(int a) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static TheoryData<int> DataSource1 = new TheoryData<int>();
				public static TheoryData<int> DataSource2 = new TheoryData<int>();

				[Theory]
				[MemberData(nameof(DataSource1))]
				public void TestMethod1(int a) { }

				[Theory]
				[MemberData(nameof(DataSource2))]
				public void TestMethod2(int a) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, MemberDataShouldReferenceValidMember_NameOfFixer.Key_UseNameof);
	}
}
