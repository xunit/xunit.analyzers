using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1034_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using Xunit;

			public class TestClass {
				public static TheoryData<string?> NullableReferenceData(string? s) => new TheoryData<string?> { s };

				[MemberData(nameof(NullableReferenceData), default(string))]
				public void TestMethod1(string? _) { }

				public static TheoryData<string> NonNullableReferenceData(string s) => new TheoryData<string> { s };

				[MemberData(nameof(NonNullableReferenceData), {|#0:default(string)|})]
				public void TestMethod(string _) { }

			#nullable disable
				public static TheoryData<string> MaybeNullableReferenceData(string s) => new TheoryData<string> { s };
			#nullable enable

				[MemberData(nameof(MaybeNullableReferenceData), default(string))]
				public void TestMethod3(string? _) { }

				public static TheoryData<int?> NullableStructData(int? n) => new TheoryData<int?> { n };

				[MemberData(nameof(NullableStructData), new object[] { null })]
				public void TestMethod4(int? _) { }

				public static TheoryData<int> NonNullableStructData(int n) => new TheoryData<int> { n };

				[MemberData(nameof(NonNullableStructData), new object[] { {|#1:null|} })]
				public void TestMethod5(int _) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1034").WithLocation(0).WithArguments("s", "string"),
			Verify.Diagnostic("xUnit1034").WithLocation(1).WithArguments("n", "int"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
	}
}
