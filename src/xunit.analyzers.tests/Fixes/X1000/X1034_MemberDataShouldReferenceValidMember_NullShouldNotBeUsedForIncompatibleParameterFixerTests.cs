using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1034_MemberDataShouldReferenceValidMember_NullShouldNotBeUsedForIncompatibleParameterFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			#nullable enable

			using Xunit;

			public class TestClass {
				public static TheoryData<int> NullableValueType(int n, int k) => new TheoryData<int>();
				public static TheoryData<int> NullableReferenceType(int n, string k) => new TheoryData<int>();

				[Theory]
				[MemberData(nameof(NullableValueType), 42, {|xUnit1034:null|})]
				public void TestMethod1(int a) { }

				[Theory]
				[MemberData(nameof(NullableReferenceType), 42, {|xUnit1034:null|})]
				public void TestMethod2(int a) { }
			}
			""";
		var after = /* lang=c#-test */ """
			#nullable enable

			using Xunit;

			public class TestClass {
				public static TheoryData<int> NullableValueType(int n, int? k) => new TheoryData<int>();
				public static TheoryData<int> NullableReferenceType(int n, string? k) => new TheoryData<int>();

				[Theory]
				[MemberData(nameof(NullableValueType), 42, null)]
				public void TestMethod1(int a) { }

				[Theory]
				[MemberData(nameof(NullableReferenceType), 42, null)]
				public void TestMethod2(int a) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(LanguageVersion.CSharp8, before, after, MemberDataShouldReferenceValidMember_NullShouldNotBeUsedForIncompatibleParameterFixer.Key_MakeParameterNullable);
	}
}
