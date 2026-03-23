using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1039_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static TheoryData<int, string[]> FieldData = new TheoryData<int, string[]>();
				public static TheoryData<int, string[]> PropertyData => new TheoryData<int, string[]>();
				public static TheoryData<int, string[]> MethodData() => new TheoryData<int, string[]>();
				public static TheoryData<int, string[]> MethodWithArgsData(int _) => new TheoryData<int, string[]>();

				// Exact match
				[MemberData(nameof(FieldData))]
				[MemberData(nameof(PropertyData))]
				[MemberData(nameof(MethodData))]
				[MemberData(nameof(MethodWithArgsData), 42)]
				public void TestMethod1(int _1, params string[] _2) { }

				public static TheoryData<int, string, string> FieldDataCollapse = new TheoryData<int, string, string>();
				public static TheoryData<int, string, string> PropertyDataCollapse => new TheoryData<int, string, string>();
				public static TheoryData<int, string, string> MethodDataCollapse() => new TheoryData<int, string, string>();
				public static TheoryData<int, string, string> MethodWithArgsDataCollapse(int _) => new TheoryData<int, string, string>();

				// Multiple values can be collapsed into the params array
				[MemberData(nameof(FieldDataCollapse))]
				[MemberData(nameof(PropertyDataCollapse))]
				[MemberData(nameof(MethodDataCollapse))]
				[MemberData(nameof(MethodWithArgsDataCollapse), 42)]
				public void TestMethod2(int _1, params string[] _2) { }

				public static TheoryData<(int, int)> FieldNamelessTupleData = new TheoryData<(int, int)>();
				public static TheoryData<(int, int)> PropertyNamelessTupleData => new TheoryData<(int, int)>();
				public static TheoryData<(int, int)> MethodNamelessTupleData() => new TheoryData<(int, int)>();
				public static TheoryData<(int, int)> MethodWithArgsNamelessTupleData(int _) => new TheoryData<(int, int)>();

				// Nameless anonymous tuples
				[MemberData(nameof(FieldNamelessTupleData))]
				[MemberData(nameof(PropertyNamelessTupleData))]
				[MemberData(nameof(MethodNamelessTupleData))]
				[MemberData(nameof(MethodWithArgsNamelessTupleData), 42)]
				public void TestMethod3((int a, int b) _) { }

				public static TheoryData<(int x, int y)> FieldNamedTupleData = new TheoryData<(int x, int y)>();
				public static TheoryData<(int x, int y)> PropertyNamedTupleData => new TheoryData<(int x, int y)>();
				public static TheoryData<(int x, int y)> MethodNamedTupleData() => new TheoryData<(int x, int y)>();
				public static TheoryData<(int x, int y)> MethodWithArgsNamedTupleData(int _) => new TheoryData<(int x, int y)>();

				// Named anonymous tuples (names don't need to match, just the shape)
				[MemberData(nameof(FieldNamedTupleData))]
				[MemberData(nameof(PropertyNamedTupleData))]
				[MemberData(nameof(MethodNamedTupleData))]
				[MemberData(nameof(MethodWithArgsNamedTupleData), 42)]
				public void TestMethod4((int a, int b) _) { }

				public static TheoryData<object[]> FieldArrayData = new TheoryData<object[]>();
				public static TheoryData<object[]> PropertyArrayData => new TheoryData<object[]>();
				public static TheoryData<object[]> MethodArrayData() => new TheoryData<object[]>();
				public static TheoryData<object[]> MethodWithArgsArrayData(int _) => new TheoryData<object[]>();

				// https://github.com/xunit/xunit/issues/3007
				[MemberData(nameof(FieldArrayData))]
				[MemberData(nameof(PropertyArrayData))]
				[MemberData(nameof(MethodArrayData))]
				[MemberData(nameof(MethodWithArgsArrayData), 42)]
				public void TestMethod5a<T>(T[] _1) {{ }}

				[MemberData(nameof(FieldArrayData))]
				[MemberData(nameof(PropertyArrayData))]
				[MemberData(nameof(MethodArrayData))]
				[MemberData(nameof(MethodWithArgsArrayData), 42)]
				public void TestMethod5b<T>(IEnumerable<T> _1) {{ }}

				public static TheoryData<int, string, int> FieldWithExtraArgData = new TheoryData<int, string, int>();
				public static TheoryData<int, string, int> PropertyWithExtraArgData => new TheoryData<int, string, int>();
				public static TheoryData<int, string, int> MethodWithExtraArgData() => new TheoryData<int, string, int>();
				public static TheoryData<int, string, int> MethodWithArgsWithExtraArgData(int _) => new TheoryData<int, string, int>();

				// Extra argument does not match params array type
				[MemberData(nameof(FieldWithExtraArgData))]
				[MemberData(nameof(PropertyWithExtraArgData))]
				[MemberData(nameof(MethodWithExtraArgData))]
				[MemberData(nameof(MethodWithArgsWithExtraArgData))]
				public void TestMethod6(int _1, params {|#0:string[]|} _2) { }

				public static TheoryData<int> FieldIncompatibleData = new TheoryData<int>();
				public static TheoryData<int> PropertyIncompatibleData => new TheoryData<int>();
				public static TheoryData<int> MethodIncompatibleData() => new TheoryData<int>();
				public static TheoryData<int> MethodWithArgsIncompatibleData(int _) => new TheoryData<int>();

				// Incompatible data type
				[MemberData(nameof(FieldIncompatibleData))]
				[MemberData(nameof(PropertyIncompatibleData))]
				[MemberData(nameof(MethodIncompatibleData))]
				[MemberData(nameof(MethodWithArgsIncompatibleData))]
				public void TestMethod7({|#1:string|} _) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.FieldWithExtraArgData", "_2"),
			Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.PropertyWithExtraArgData", "_2"),
			Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.MethodWithExtraArgData", "_2"),
			Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.MethodWithArgsWithExtraArgData", "_2"),

			Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.FieldIncompatibleData", "_"),
			Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.PropertyIncompatibleData", "_"),
			Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.MethodIncompatibleData", "_"),
			Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.MethodWithArgsIncompatibleData", "_"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, source, expected);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static IEnumerable<TheoryDataRow<int, string[]>> FieldData = new List<TheoryDataRow<int, string[]>>();
				public static IEnumerable<TheoryDataRow<int, string[]>> PropertyData => new List<TheoryDataRow<int, string[]>>();
				public static IEnumerable<TheoryDataRow<int, string[]>> MethodData() => new List<TheoryDataRow<int, string[]>>();
				public static IEnumerable<TheoryDataRow<int, string[]>> MethodWithArgsData(int _) => new List<TheoryDataRow<int, string[]>>();

				// Exact match
				[MemberData(nameof(FieldData))]
				[MemberData(nameof(PropertyData))]
				[MemberData(nameof(MethodData))]
				[MemberData(nameof(MethodWithArgsData), 42)]
				public void TestMethod1(int _1, params string[] _2) { }

				public static IEnumerable<TheoryDataRow<int, string, string>> FieldDataCollapse = new List<TheoryDataRow<int, string, string>>();
				public static IEnumerable<TheoryDataRow<int, string, string>> PropertyDataCollapse => new List<TheoryDataRow<int, string, string>>();
				public static IEnumerable<TheoryDataRow<int, string, string>> MethodDataCollapse() => new List<TheoryDataRow<int, string, string>>();
				public static IEnumerable<TheoryDataRow<int, string, string>> MethodWithArgsDataCollapse(int _) => new List<TheoryDataRow<int, string, string>>();

				// Multiple values can be collapsed into the params array
				[MemberData(nameof(FieldDataCollapse))]
				[MemberData(nameof(PropertyDataCollapse))]
				[MemberData(nameof(MethodDataCollapse))]
				[MemberData(nameof(MethodWithArgsDataCollapse), 42)]
				public void TestMethod2(int _1, params string[] _2) { }

				public static IEnumerable<TheoryDataRow<(int, int)>> FieldNamelessTupleData = new List<TheoryDataRow<(int, int)>>();
				public static IEnumerable<TheoryDataRow<(int, int)>> PropertyNamelessTupleData => new List<TheoryDataRow<(int, int)>>();
				public static IEnumerable<TheoryDataRow<(int, int)>> MethodNamelessTupleData() => new List<TheoryDataRow<(int, int)>>();
				public static IEnumerable<TheoryDataRow<(int, int)>> MethodWithArgsNamelessTupleData(int _) => new List<TheoryDataRow<(int, int)>>();

				// Nameless anonymous tuples
				[MemberData(nameof(FieldNamelessTupleData))]
				[MemberData(nameof(PropertyNamelessTupleData))]
				[MemberData(nameof(MethodNamelessTupleData))]
				[MemberData(nameof(MethodWithArgsNamelessTupleData), 42)]
				public void TestMethod3((int a, int b) _) { }

				public static IEnumerable<TheoryDataRow<(int x, int y)>> FieldNamedTupleData = new List<TheoryDataRow<(int x, int y)>>();
				public static IEnumerable<TheoryDataRow<(int x, int y)>> PropertyNamedTupleData => new List<TheoryDataRow<(int x, int y)>>();
				public static IEnumerable<TheoryDataRow<(int x, int y)>> MethodNamedTupleData() => new List<TheoryDataRow<(int x, int y)>>();
				public static IEnumerable<TheoryDataRow<(int x, int y)>> MethodWithArgsNamedTupleData(int _) => new List<TheoryDataRow<(int x, int y)>>();

				// Named anonymous tuples (names don't need to match, just the shape)
				[MemberData(nameof(FieldNamedTupleData))]
				[MemberData(nameof(PropertyNamedTupleData))]
				[MemberData(nameof(MethodNamedTupleData))]
				[MemberData(nameof(MethodWithArgsNamedTupleData), 42)]
				public void TestMethod4((int a, int b) _) { }

				public static IEnumerable<TheoryDataRow<object[]>> FieldArrayData = new List<TheoryDataRow<object[]>>();
				public static IEnumerable<TheoryDataRow<object[]>> PropertyArrayData => new List<TheoryDataRow<object[]>>();
				public static IEnumerable<TheoryDataRow<object[]>> MethodArrayData() => new List<TheoryDataRow<object[]>>();
				public static IEnumerable<TheoryDataRow<object[]>> MethodWithArgsArrayData(int _) => new List<TheoryDataRow<object[]>>();

				// https://github.com/xunit/xunit/issues/3007
				[MemberData(nameof(FieldArrayData))]
				[MemberData(nameof(PropertyArrayData))]
				[MemberData(nameof(MethodArrayData))]
				[MemberData(nameof(MethodWithArgsArrayData), 42)]
				public void TestMethod5a<T>(T[] _1) {{ }}

				[MemberData(nameof(FieldArrayData))]
				[MemberData(nameof(PropertyArrayData))]
				[MemberData(nameof(MethodArrayData))]
				[MemberData(nameof(MethodWithArgsArrayData), 42)]
				public void TestMethod5b<T>(IEnumerable<T> _1) {{ }}

				public static IEnumerable<TheoryDataRow<int, string, int>> FieldWithExtraArgData = new List<TheoryDataRow<int, string, int>>();
				public static IEnumerable<TheoryDataRow<int, string, int>> PropertyWithExtraArgData => new List<TheoryDataRow<int, string, int>>();
				public static IEnumerable<TheoryDataRow<int, string, int>> MethodWithExtraArgData() => new List<TheoryDataRow<int, string, int>>();
				public static IEnumerable<TheoryDataRow<int, string, int>> MethodWithArgsWithExtraArgData(int _) => new List<TheoryDataRow<int, string, int>>();

				// Extra argument does not match params array type
				[MemberData(nameof(FieldWithExtraArgData))]
				[MemberData(nameof(PropertyWithExtraArgData))]
				[MemberData(nameof(MethodWithExtraArgData))]
				[MemberData(nameof(MethodWithArgsWithExtraArgData))]
				public void TestMethod6(int _1, params {|#0:string[]|} _2) { }

				public static IEnumerable<TheoryDataRow<int>> FieldIncompatibleData = new List<TheoryDataRow<int>>();
				public static IEnumerable<TheoryDataRow<int>> PropertyIncompatibleData => new List<TheoryDataRow<int>>();
				public static IEnumerable<TheoryDataRow<int>> MethodIncompatibleData() => new List<TheoryDataRow<int>>();
				public static IEnumerable<TheoryDataRow<int>> MethodWithArgsIncompatibleData(int _) => new List<TheoryDataRow<int>>();

				// Incompatible data type
				[MemberData(nameof(FieldIncompatibleData))]
				[MemberData(nameof(PropertyIncompatibleData))]
				[MemberData(nameof(MethodIncompatibleData))]
				[MemberData(nameof(MethodWithArgsIncompatibleData))]
				public void TestMethod7({|#1:string|} _) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.FieldWithExtraArgData", "_2"),
			Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.PropertyWithExtraArgData", "_2"),
			Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.MethodWithExtraArgData", "_2"),
			Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("int", "TestClass.MethodWithArgsWithExtraArgData", "_2"),

			Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.FieldIncompatibleData", "_"),
			Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.PropertyIncompatibleData", "_"),
			Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.MethodIncompatibleData", "_"),
			Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "TestClass.MethodWithArgsIncompatibleData", "_"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7, source, expected);
	}
}
