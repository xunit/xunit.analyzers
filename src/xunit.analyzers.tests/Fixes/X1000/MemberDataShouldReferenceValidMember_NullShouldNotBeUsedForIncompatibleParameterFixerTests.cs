using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class MemberDataShouldReferenceValidMember_NullShouldNotBeUsedForIncompatibleParameterFixerTests
{
	[Fact]
	public async Task FixAll_MakesMultipleParametersNullable()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData1(int n, int k) => new TheoryData<int>();
				public static TheoryData<int> TestData2(int n, int k) => new TheoryData<int>();

				[Theory]
				[MemberData(nameof(TestData1), 42, {|xUnit1034:null|})]
				public void TestMethod1(int a) { }

				[Theory]
				[MemberData(nameof(TestData2), 42, {|xUnit1034:null|})]
				public void TestMethod2(int a) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData1(int n, int? k) => new TheoryData<int>();
				public static TheoryData<int> TestData2(int n, int? k) => new TheoryData<int>();

				[Theory]
				[MemberData(nameof(TestData1), 42, null)]
				public void TestMethod1(int a) { }

				[Theory]
				[MemberData(nameof(TestData2), 42, null)]
				public void TestMethod2(int a) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, MemberDataShouldReferenceValidMember_NullShouldNotBeUsedForIncompatibleParameterFixer.Key_MakeParameterNullable);
	}

	[Fact]
	public async Task MakesParameterNullable()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData(int n, int k) => new TheoryData<int>();

				[Theory]
				[MemberData(nameof(TestData), 42, {|xUnit1034:null|})]
				public void TestMethod(int a) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData(int n, int? k) => new TheoryData<int>();

				[Theory]
				[MemberData(nameof(TestData), 42, null)]
				public void TestMethod(int a) { }
			}
			""";

		await Verify.VerifyCodeFix(before, after, MemberDataShouldReferenceValidMember_NullShouldNotBeUsedForIncompatibleParameterFixer.Key_MakeParameterNullable);
	}

	[Fact]
	public async Task MakesReferenceParameterNullable()
	{
		var before = /* lang=c#-test */ """
			#nullable enable

			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData(int n, string k) => new TheoryData<int> { n };

				[Theory]
				[MemberData(nameof(TestData), 42, {|xUnit1034:null|})]
				public void TestMethod(int a) { }
			}
			""";
		var after = /* lang=c#-test */ """
			#nullable enable

			using Xunit;

			public class TestClass {
				public static TheoryData<int> TestData(int n, string? k) => new TheoryData<int> { n };

				[Theory]
				[MemberData(nameof(TestData), 42, null)]
				public void TestMethod(int a) { }
			}
			""";

		await Verify.VerifyCodeFix(LanguageVersion.CSharp8, before, after, MemberDataShouldReferenceValidMember_NullShouldNotBeUsedForIncompatibleParameterFixer.Key_MakeParameterNullable);
	}
}
