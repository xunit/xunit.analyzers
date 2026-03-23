using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1040_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using Xunit;

			public class TestClass {
				public static TheoryData<string?> FieldData = new TheoryData<string?>();
				public static TheoryData<string?> PropertyData => new TheoryData<string?>();
				public static TheoryData<string?> MethodData() => new TheoryData<string?>();
				public static TheoryData<string?> MethodWithArgsData(int _) => new TheoryData<string?>();

				[MemberData(nameof(FieldData))]
				[MemberData(nameof(PropertyData))]
				[MemberData(nameof(MethodData))]
				[MemberData(nameof(MethodWithArgsData), 42)]
				public void TestMethod({|#0:string|} _) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.FieldData", "_"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.PropertyData", "_"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.MethodData", "_"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.MethodWithArgsData", "_"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static IEnumerable<TheoryDataRow<string?>> FieldData = new List<TheoryDataRow<string?>>();
				public static IEnumerable<TheoryDataRow<string?>> PropertyData => new List<TheoryDataRow<string?>>();
				public static IEnumerable<TheoryDataRow<string?>> MethodData() => new List<TheoryDataRow<string?>>();
				public static IEnumerable<TheoryDataRow<string?>> MethodWithArgsData(int _) => new List<TheoryDataRow<string?>>();

				[MemberData(nameof(FieldData))]
				[MemberData(nameof(PropertyData))]
				[MemberData(nameof(MethodData))]
				[MemberData(nameof(MethodWithArgsData), 42)]
				public void TestMethod({|#0:string|} _) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.FieldData", "_"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.PropertyData", "_"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.MethodData", "_"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.MethodWithArgsData", "_"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source, expected);
	}
}
