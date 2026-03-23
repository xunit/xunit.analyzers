using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1042_MemberDataShouldReferenceValidMemberTests
{
	const string V2AllowedTypes = "TheoryData<>";
	const string V3AllowedTypes = "TheoryData<> or IEnumerable<TheoryDataRow<>>";

	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			#pragma warning disable xUnit1053

			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static TheoryData<int> FieldData;
				public static TheoryData<int> PropertyData { get; set; }
				public static TheoryData<int> MethodData() => null;
				public static TheoryData<int> MethodWithArgsData(int _) => null;

				[MemberData(nameof(FieldData))]
				[MemberData(nameof(PropertyData))]
				[MemberData(nameof(MethodData))]
				[MemberData(nameof(MethodWithArgsData), 42)]
				public void TestMethod1(int _) { }

				public static IEnumerable<object[]> FieldUntypedData;
				public static IEnumerable<object[]> PropertyUntypedData { get; set; }
				public static List<object[]> MethodUntypedData() => null;
				public static object[][] MethodWithArgsUntypedData(int _) => null;

				[{|#0:MemberData(nameof(FieldUntypedData))|}]
				[{|#1:MemberData(nameof(PropertyUntypedData))|}]
				[{|#2:MemberData(nameof(MethodUntypedData))|}]
				[{|#3:MemberData(nameof(MethodWithArgsUntypedData), 42)|}]

				public void TestMethod2(int _) { }
			}
			""";
		var expectedV2 = new[] {
			Verify.Diagnostic("xUnit1042").WithLocation(0).WithArguments(V2AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(1).WithArguments(V2AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(2).WithArguments(V2AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(3).WithArguments(V2AllowedTypes),
		};
		var expectedV3 = new[] {
			Verify.Diagnostic("xUnit1042").WithLocation(0).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(1).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(2).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(3).WithArguments(V3AllowedTypes),
		};

		await Verify.VerifyAnalyzerV2(source, expectedV2);
		await Verify.VerifyAnalyzerV3(source, expectedV3);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			#pragma warning disable xUnit1053

			using System.Collections.Generic;
			using System.Threading.Tasks;
			using Xunit;

			public class TestClass {
				public static MatrixTheoryData<int, string> FieldData;
				public static MatrixTheoryData<int, string> PropertyData { get; set; }
				public static MatrixTheoryData<int, string> MethodData() => null;
				public static MatrixTheoryData<int, string> MethodWithArgsData(int _) => null;

				[MemberData(nameof(FieldData))]
				[MemberData(nameof(PropertyData))]
				[MemberData(nameof(MethodData))]
				[MemberData(nameof(MethodWithArgsData), 42)]
				public void TestMethod1(int _1, string _2) { }

				public static IEnumerable<TheoryDataRow<int>> FieldEnumerableData;
				public static IAsyncEnumerable<TheoryDataRow<int>> PropertyEnumerableData { get; set; }
				public static List<TheoryDataRow<int>> MethodEnumerableData() => null;
				public static TheoryDataRow<int>[] MethodWithArgsEnumerableData(int _) => null;

				[MemberData(nameof(FieldEnumerableData))]
				[MemberData(nameof(PropertyEnumerableData))]
				[MemberData(nameof(MethodEnumerableData))]
				[MemberData(nameof(MethodWithArgsEnumerableData), 42)]
				public void TestMethod2(int _) { }

				public static Task<IEnumerable<object[]>> FieldUntypedTaskData;
				public static Task<IAsyncEnumerable<object[]>> PropertyUntypedTaskData { get; set; }
				public static Task<List<object[]>> MethodUntypedTaskData() => null;
				public static Task<object[][]> MethodWithArgsUntypedTaskData(int _) => null;

				[{|#0:MemberData(nameof(FieldUntypedTaskData))|}]
				[{|#1:MemberData(nameof(PropertyUntypedTaskData))|}]
				[{|#2:MemberData(nameof(MethodUntypedTaskData))|}]
				[{|#3:MemberData(nameof(MethodWithArgsUntypedTaskData), 42)|}]
				public void TestMethod3(int _) { }

				public static ValueTask<IEnumerable<object[]>> FieldUntypedValueTaskData;
				public static ValueTask<IAsyncEnumerable<object[]>> PropertyUntypedValueTaskData { get; set; }
				public static ValueTask<List<object[]>> MethodUntypedValueTaskData() => default;
				public static ValueTask<object[][]> MethodWithArgsUntypedValueTaskData(int _) => default;

				[{|#10:MemberData(nameof(FieldUntypedValueTaskData))|}]
				[{|#11:MemberData(nameof(PropertyUntypedValueTaskData))|}]
				[{|#12:MemberData(nameof(MethodUntypedValueTaskData))|}]
				[{|#13:MemberData(nameof(MethodWithArgsUntypedValueTaskData), 42)|}]
				public void TestMethod4(int _) { }

				public static Task<IEnumerable<ITheoryDataRow>> FieldUntypedTaskData2;
				public static Task<IAsyncEnumerable<ITheoryDataRow>> PropertyUntypedTaskData2 { get; set; }
				public static Task<List<TheoryDataRow>> MethodUntypedTaskData2() => null;
				public static Task<TheoryDataRow[]> MethodWithArgsUntypedTaskData2(int _) => null;

				[{|#20:MemberData(nameof(FieldUntypedTaskData2))|}]
				[{|#21:MemberData(nameof(PropertyUntypedTaskData2))|}]
				[{|#22:MemberData(nameof(MethodUntypedTaskData2))|}]
				[{|#23:MemberData(nameof(MethodWithArgsUntypedTaskData2), 42)|}]
				public void TestMethod5(int _) { }

				public static ValueTask<IEnumerable<ITheoryDataRow>> FieldUntypedValueTaskData2;
				public static ValueTask<IAsyncEnumerable<ITheoryDataRow>> PropertyUntypedValueTaskData2 { get; set; }
				public static ValueTask<List<TheoryDataRow>> MethodUntypedValueTaskData2() => default;
				public static ValueTask<TheoryDataRow[]> MethodWithArgsUntypedValueTaskData2(int _) => default;

				[{|#30:MemberData(nameof(FieldUntypedValueTaskData2))|}]
				[{|#31:MemberData(nameof(PropertyUntypedValueTaskData2))|}]
				[{|#32:MemberData(nameof(MethodUntypedValueTaskData2))|}]
				[{|#33:MemberData(nameof(MethodWithArgsUntypedValueTaskData2), 42)|}]
				public void TestMethod6(int _) { }

				public static TheoryData<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int> LongData;

				[MemberData(nameof(LongData))]
				public void TestMethod7(int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l, int m, int n, int o) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1042").WithLocation(0).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(1).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(2).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(3).WithArguments(V3AllowedTypes),

			Verify.Diagnostic("xUnit1042").WithLocation(10).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(11).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(12).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(13).WithArguments(V3AllowedTypes),

			Verify.Diagnostic("xUnit1042").WithLocation(20).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(21).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(22).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(23).WithArguments(V3AllowedTypes),

			Verify.Diagnostic("xUnit1042").WithLocation(30).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(31).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(32).WithArguments(V3AllowedTypes),
			Verify.Diagnostic("xUnit1042").WithLocation(33).WithArguments(V3AllowedTypes),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7_1, source, expected);
	}
}
