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
				public static IEnumerable<TheoryDataRow<string?, int>> FieldData_TheoryDataRow = new List<TheoryDataRow<string?, int>>();
				public static IEnumerable<TheoryDataRow<string?, int>> PropertyData_TheoryDataRow => new List<TheoryDataRow<string?, int>>();
				public static IEnumerable<TheoryDataRow<string?, int>> MethodData_TheoryDataRow() => new List<TheoryDataRow<string?, int>>();
				public static IEnumerable<TheoryDataRow<string?, int>> MethodWithArgsData_TheoryDataRow(int _) => new List<TheoryDataRow<string?, int>>();

				public static IEnumerable<(string?, int)> FieldData_Tuple = [];
				public static IEnumerable<(string?, int)> PropertyData_Tuple => [];
				public static IEnumerable<(string?, int)> MethodData_Tuple() => [];
				public static IEnumerable<(string?, int)> MethodWithArgsData_Tuple(int _) => [];

				[MemberData(nameof(FieldData_TheoryDataRow))]
				[MemberData(nameof(PropertyData_TheoryDataRow))]
				[MemberData(nameof(MethodData_TheoryDataRow))]
				[MemberData(nameof(MethodWithArgsData_TheoryDataRow), 42)]
				[MemberData(nameof(FieldData_Tuple))]
				[MemberData(nameof(PropertyData_Tuple))]
				[MemberData(nameof(MethodData_Tuple))]
				[MemberData(nameof(MethodWithArgsData_Tuple), 42)]
				public void TestMethod({|#0:string|} _1, int _2) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.FieldData_TheoryDataRow", "_1"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.PropertyData_TheoryDataRow", "_1"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.MethodData_TheoryDataRow", "_1"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.MethodWithArgsData_TheoryDataRow", "_1"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.FieldData_Tuple", "_1"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.PropertyData_Tuple", "_1"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.MethodData_Tuple", "_1"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "TestClass.MethodWithArgsData_Tuple", "_1"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp12, source, expected);
	}
}
