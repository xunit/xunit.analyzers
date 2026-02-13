using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_ParamsForNonMethodFixerTests
{
	[Fact]
	public async Task FixAll_RemovesParametersFromNonMethodMemberData()
	{
		var before = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public class TestClass
			{
				public static TheoryData<int> DataSource1 = new TheoryData<int>();
				public static TheoryData<string> DataSource2 = new TheoryData<string>();

				[Theory]
				[MemberData(nameof(DataSource1), {|xUnit1021:"abc", 123|})]
				public void TestMethod1(int a) { }

				[Theory]
				[MemberData(nameof(DataSource2), {|xUnit1021:"xyz"|})]
				public void TestMethod2(string b) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			public class TestClass
			{
				public static TheoryData<int> DataSource1 = new TheoryData<int>();
				public static TheoryData<string> DataSource2 = new TheoryData<string>();

				[Theory]
				[MemberData(nameof(DataSource1))]
				public void TestMethod1(int a) { }

				[Theory]
				[MemberData(nameof(DataSource2))]
				public void TestMethod2(string b) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, MemberDataShouldReferenceValidMember_ParamsForNonMethodFixer.Key_RemoveArgumentsFromMemberData);
	}
}
