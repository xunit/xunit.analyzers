using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1038_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using Xunit;

			public class DerivedTheoryData : TheoryData<int> { }
			public class DerivedTheoryData<T> : TheoryData<T> { }
			public class DerivedTheoryData<T, U> : TheoryData<T> { }

			public class DerivedTheoryData2 : TheoryData<int, int> { }
			public class DerivedTheoryData2<T1, T2> : TheoryData<T1, T2> { }

			public class DerivedTheoryData3 : TheoryData<int, string[], string> { }
			public class DerivedTheoryData3<T1, T2, T3> : TheoryData<T1, T2, T3> { }

			public class TestClass {
				// ===== Direct TheoryData<> usage =====

				public static TheoryData<int> FieldTheoryData = new TheoryData<int>();
				public static TheoryData<int> PropertyTheoryData => new TheoryData<int>();
				public static TheoryData<int> MethodTheoryData() => new TheoryData<int>();
				public static TheoryData<int> MethodWithArgsTheoryData(int _) => new TheoryData<int>();

				// Exact match
				[MemberData(nameof(FieldTheoryData))]
				[MemberData(nameof(PropertyTheoryData))]
				[MemberData(nameof(MethodTheoryData))]
				[MemberData(nameof(MethodWithArgsTheoryData), 42)]
				public void TestMethod1a(int _) { }

				// Optional paramter, no argument from data source
				[MemberData(nameof(FieldTheoryData))]
				[MemberData(nameof(PropertyTheoryData))]
				[MemberData(nameof(MethodTheoryData))]
				[MemberData(nameof(MethodWithArgsTheoryData), 42)]
				public void TestMethod1b(int _1, int _2 = 0) { }

				// Params array, no argument from data source
				[MemberData(nameof(FieldTheoryData))]
				[MemberData(nameof(PropertyTheoryData))]
				[MemberData(nameof(MethodTheoryData))]
				[MemberData(nameof(MethodWithArgsTheoryData), 42)]
				public void TestMethod1c(int _1, params int[] _2) { }

				// Generic match
				[MemberData(nameof(FieldTheoryData))]
				[MemberData(nameof(PropertyTheoryData))]
				[MemberData(nameof(MethodTheoryData))]
				[MemberData(nameof(MethodWithArgsTheoryData), 42)]
				public void TestMethod1d<T>(T _) { }

				// Generic nullable match
				[MemberData(nameof(FieldTheoryData))]
				[MemberData(nameof(PropertyTheoryData))]
				[MemberData(nameof(MethodTheoryData))]
				[MemberData(nameof(MethodWithArgsTheoryData), 42)]
				public void TestMethod1e<T>(T? _) { }

				public static TheoryData<int, int> FieldTheoryData2 = new TheoryData<int, int>();
				public static TheoryData<int, int> PropertyTheoryData2 => new TheoryData<int, int>();
				public static TheoryData<int, int> MethodTheoryData2() => new TheoryData<int, int>();
				public static TheoryData<int, int> MethodWithArgsTheoryData2(int _) => new TheoryData<int, int>();

				// Params array, single non-array argument from data source
				[MemberData(nameof(FieldTheoryData2))]
				[MemberData(nameof(PropertyTheoryData2))]
				[MemberData(nameof(MethodTheoryData2))]
				[MemberData(nameof(MethodWithArgsTheoryData2), 42)]
				public void TestMethod1f(int _1, params int[] _2) { }

				// Too many arguments
				[{|#0:MemberData(nameof(FieldTheoryData2))|}]
				[{|#1:MemberData(nameof(PropertyTheoryData2))|}]
				[{|#2:MemberData(nameof(MethodTheoryData2))|}]
				[{|#3:MemberData(nameof(MethodWithArgsTheoryData2), 42)|}]
				public void TestMethod1g(int _) { }

				public static TheoryData<int, string[], string> FieldTheoryData3 = new TheoryData<int, string[], string>();
				public static TheoryData<int, string[], string> PropertyTheoryData3 => new TheoryData<int, string[], string>();
				public static TheoryData<int, string[], string> MethodTheoryData3() => new TheoryData<int, string[], string>();
				public static TheoryData<int, string[], string> MethodWithArgsTheoryData3(int _) => new TheoryData<int, string[], string>();

				// Extra parameter type on data source
				[{|#4:MemberData(nameof(FieldTheoryData3))|}]
				[{|#5:MemberData(nameof(PropertyTheoryData3))|}]
				[{|#6:MemberData(nameof(MethodTheoryData3))|}]
				[{|#7:MemberData(nameof(MethodWithArgsTheoryData3), 42)|}]
				public void TestMethod1h(int _1, params string[] _2) { }

				// ===== Indirect TheoryData<> without generics =====

				public static DerivedTheoryData FieldDerivedTheoryData = new DerivedTheoryData();
				public static DerivedTheoryData PropertyDerivedTheoryData => new DerivedTheoryData();
				public static DerivedTheoryData MethodDerivedTheoryData() => new DerivedTheoryData();
				public static DerivedTheoryData MethodWithArgsDerivedTheoryData(int _) => new DerivedTheoryData();

				// Exact match
				[MemberData(nameof(FieldDerivedTheoryData))]
				[MemberData(nameof(PropertyDerivedTheoryData))]
				[MemberData(nameof(MethodDerivedTheoryData))]
				[MemberData(nameof(MethodWithArgsDerivedTheoryData), 42)]
				public void TestMethod2a(int _) { }

				// Optional paramter, no argument from data source
				[MemberData(nameof(FieldDerivedTheoryData))]
				[MemberData(nameof(PropertyDerivedTheoryData))]
				[MemberData(nameof(MethodDerivedTheoryData))]
				[MemberData(nameof(MethodWithArgsDerivedTheoryData), 42)]
				public void TestMethod2b(int _1, int _2 = 0) { }

				// Params array, no argument from data source
				[MemberData(nameof(FieldDerivedTheoryData))]
				[MemberData(nameof(PropertyDerivedTheoryData))]
				[MemberData(nameof(MethodDerivedTheoryData))]
				[MemberData(nameof(MethodWithArgsDerivedTheoryData), 42)]
				public void TestMethod2c(int _1, params int[] _2) { }

				// Generic match
				[MemberData(nameof(FieldDerivedTheoryData))]
				[MemberData(nameof(PropertyDerivedTheoryData))]
				[MemberData(nameof(MethodDerivedTheoryData))]
				[MemberData(nameof(MethodWithArgsDerivedTheoryData), 42)]
				public void TestMethod2d<T>(T _) { }

				// Generic nullable match
				[MemberData(nameof(FieldDerivedTheoryData))]
				[MemberData(nameof(PropertyDerivedTheoryData))]
				[MemberData(nameof(MethodDerivedTheoryData))]
				[MemberData(nameof(MethodWithArgsDerivedTheoryData), 42)]
				public void TestMethod2e<T>(T? _) { }

				public static DerivedTheoryData2 FieldDerivedTheoryData2 = new DerivedTheoryData2();
				public static DerivedTheoryData2 PropertyDerivedTheoryData2 => new DerivedTheoryData2();
				public static DerivedTheoryData2 MethodDerivedTheoryData2() => new DerivedTheoryData2();
				public static DerivedTheoryData2 MethodWithArgsDerivedTheoryData2(int _) => new DerivedTheoryData2();

				// Params array, single non-array argument from data source
				[MemberData(nameof(FieldDerivedTheoryData2))]
				[MemberData(nameof(PropertyDerivedTheoryData2))]
				[MemberData(nameof(MethodDerivedTheoryData2))]
				[MemberData(nameof(MethodWithArgsDerivedTheoryData2), 42)]
				public void TestMethod2f(int _1, params int[] _2) { }

				// Too many arguments
				[{|#10:MemberData(nameof(FieldDerivedTheoryData2))|}]
				[{|#11:MemberData(nameof(PropertyDerivedTheoryData2))|}]
				[{|#12:MemberData(nameof(MethodDerivedTheoryData2))|}]
				[{|#13:MemberData(nameof(MethodWithArgsDerivedTheoryData2), 42)|}]
				public void TestMethod2g(int _) { }

				public static DerivedTheoryData3 FieldDerivedTheoryData3 = new DerivedTheoryData3();
				public static DerivedTheoryData3 PropertyDerivedTheoryData3 => new DerivedTheoryData3();
				public static DerivedTheoryData3 MethodDerivedTheoryData3() => new DerivedTheoryData3();
				public static DerivedTheoryData3 MethodWithArgsDerivedTheoryData3(int _) => new DerivedTheoryData3();

				// Extra parameter type on data source
				[{|#14:MemberData(nameof(FieldDerivedTheoryData3))|}]
				[{|#15:MemberData(nameof(PropertyDerivedTheoryData3))|}]
				[{|#16:MemberData(nameof(MethodDerivedTheoryData3))|}]
				[{|#17:MemberData(nameof(MethodWithArgsDerivedTheoryData3), 42)|}]
				public void TestMethod2h(int _1, params string[] _2) { }

				// ===== Indirect TheoryData<> with generics =====

				public static DerivedTheoryData<int> FieldDerivedGenericTheoryData = new DerivedTheoryData<int>();
				public static DerivedTheoryData<int> PropertyDerivedGenericTheoryData => new DerivedTheoryData<int>();
				public static DerivedTheoryData<int> MethodDerivedGenericTheoryData() => new DerivedTheoryData<int>();
				public static DerivedTheoryData<int> MethodWithArgsDerivedGenericTheoryData(int _) => new DerivedTheoryData<int>();

				// Exact match
				[MemberData(nameof(FieldDerivedGenericTheoryData))]
				[MemberData(nameof(PropertyDerivedGenericTheoryData))]
				[MemberData(nameof(MethodDerivedGenericTheoryData))]
				[MemberData(nameof(MethodWithArgsDerivedGenericTheoryData), 42)]
				public void TestMethod3a(int _) { }

				// Optional paramter, no argument from data source
				[MemberData(nameof(FieldDerivedGenericTheoryData))]
				[MemberData(nameof(PropertyDerivedGenericTheoryData))]
				[MemberData(nameof(MethodDerivedGenericTheoryData))]
				[MemberData(nameof(MethodWithArgsDerivedGenericTheoryData), 42)]
				public void TestMethod3b(int _1, int _2 = 0) { }

				// Params array, no argument from data source
				[MemberData(nameof(FieldDerivedGenericTheoryData))]
				[MemberData(nameof(PropertyDerivedGenericTheoryData))]
				[MemberData(nameof(MethodDerivedGenericTheoryData))]
				[MemberData(nameof(MethodWithArgsDerivedGenericTheoryData), 42)]
				public void TestMethod3c(int _1, params int[] _2) { }

				// Generic match
				[MemberData(nameof(FieldDerivedGenericTheoryData))]
				[MemberData(nameof(PropertyDerivedGenericTheoryData))]
				[MemberData(nameof(MethodDerivedGenericTheoryData))]
				[MemberData(nameof(MethodWithArgsDerivedGenericTheoryData), 42)]
				public void TestMethod3d<T>(T _) { }

				// Generic nullable match
				[MemberData(nameof(FieldDerivedGenericTheoryData))]
				[MemberData(nameof(PropertyDerivedGenericTheoryData))]
				[MemberData(nameof(MethodDerivedGenericTheoryData))]
				[MemberData(nameof(MethodWithArgsDerivedGenericTheoryData), 42)]
				public void TestMethod3e<T>(T? _) { }

				public static DerivedTheoryData2<int, int> FieldDerivedGenericTheoryData2 = new DerivedTheoryData2<int, int>();
				public static DerivedTheoryData2<int, int> PropertyDerivedGenericTheoryData2 => new DerivedTheoryData2<int, int>();
				public static DerivedTheoryData2<int, int> MethodDerivedGenericTheoryData2() => new DerivedTheoryData2<int, int>();
				public static DerivedTheoryData2<int, int> MethodWithArgsDerivedGenericTheoryData2(int _) => new DerivedTheoryData2<int, int>();

				// Params array, single non-array argument from data source
				[MemberData(nameof(FieldDerivedGenericTheoryData2))]
				[MemberData(nameof(PropertyDerivedGenericTheoryData2))]
				[MemberData(nameof(MethodDerivedGenericTheoryData2))]
				[MemberData(nameof(MethodWithArgsDerivedGenericTheoryData2), 42)]
				public void TestMethod3f(int _1, params int[] _2) { }

				// Too many arguments
				[{|#20:MemberData(nameof(FieldDerivedGenericTheoryData2))|}]
				[{|#21:MemberData(nameof(PropertyDerivedGenericTheoryData2))|}]
				[{|#22:MemberData(nameof(MethodDerivedGenericTheoryData2))|}]
				[{|#23:MemberData(nameof(MethodWithArgsDerivedGenericTheoryData2), 42)|}]
				public void TestMethod3g(int _) { }

				public static DerivedTheoryData3<int, string[], string> FieldDerivedGenericTheoryData3 = new DerivedTheoryData3<int, string[], string>();
				public static DerivedTheoryData3<int, string[], string> PropertyDerivedGenericTheoryData3 => new DerivedTheoryData3<int, string[], string>();
				public static DerivedTheoryData3<int, string[], string> MethodDerivedGenericTheoryData3() => new DerivedTheoryData3<int, string[], string>();
				public static DerivedTheoryData3<int, string[], string> MethodWithArgsDerivedGenericTheoryData3(int _) => new DerivedTheoryData3<int, string[], string>();

				// Extra parameter type on data source
				[{|#24:MemberData(nameof(FieldDerivedGenericTheoryData3))|}]
				[{|#25:MemberData(nameof(PropertyDerivedGenericTheoryData3))|}]
				[{|#26:MemberData(nameof(MethodDerivedGenericTheoryData3))|}]
				[{|#27:MemberData(nameof(MethodWithArgsDerivedGenericTheoryData3), 42)|}]
				public void TestMethod3h(int _1, params string[] _2) { }

				// ===== Indirect TheoryData<> with generic type reduction =====

				public static DerivedTheoryData<int, string> FieldTheoryDataTypeReduced = new DerivedTheoryData<int, string>();
				public static DerivedTheoryData<int, string> PropertyTheoryDataTypeReduced => new DerivedTheoryData<int, string>();
				public static DerivedTheoryData<int, string> MethodTheoryDataTypeReduced() => new DerivedTheoryData<int, string>();
				public static DerivedTheoryData<int, string> MethodWithArgsTheoryDataTypeReduced(int _) => new DerivedTheoryData<int, string>();

				// Exact match
				[MemberData(nameof(FieldTheoryDataTypeReduced))]
				[MemberData(nameof(PropertyTheoryDataTypeReduced))]
				[MemberData(nameof(MethodTheoryDataTypeReduced))]
				[MemberData(nameof(MethodWithArgsTheoryDataTypeReduced), 42)]
				public void TestMethod4a(int _) { }

				// Generic match
				[MemberData(nameof(FieldTheoryDataTypeReduced))]
				[MemberData(nameof(PropertyTheoryDataTypeReduced))]
				[MemberData(nameof(MethodTheoryDataTypeReduced))]
				[MemberData(nameof(MethodWithArgsTheoryDataTypeReduced), 42)]
				public void TestMethod4d<T>(T _) { }

				// Generic nullable match
				[MemberData(nameof(FieldTheoryDataTypeReduced))]
				[MemberData(nameof(PropertyTheoryDataTypeReduced))]
				[MemberData(nameof(MethodTheoryDataTypeReduced))]
				[MemberData(nameof(MethodWithArgsTheoryDataTypeReduced), 42)]
				public void TestMethod4e<T>(T? _) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1038").WithLocation(0).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(1).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(2).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(3).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(4).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(5).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(6).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(7).WithArguments("Xunit.TheoryData"),

			Verify.Diagnostic("xUnit1038").WithLocation(10).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(11).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(12).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(13).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(14).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(15).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(16).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(17).WithArguments("Xunit.TheoryData"),

			Verify.Diagnostic("xUnit1038").WithLocation(20).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(21).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(22).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(23).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(24).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(25).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(26).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1038").WithLocation(27).WithArguments("Xunit.TheoryData"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source, expected);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static TheoryDataRow<int>[] FieldData = new TheoryDataRow<int>[0];
				public static TheoryDataRow<int>[] PropertyData => new TheoryDataRow<int>[0];
				public static TheoryDataRow<int>[] MethodData() => new TheoryDataRow<int>[0];
				public static TheoryDataRow<int>[] MethodWithArgsData(int _) => new TheoryDataRow<int>[0];

				// Exact match
				[MemberData(nameof(FieldData))]
				[MemberData(nameof(PropertyData))]
				[MemberData(nameof(MethodData))]
				[MemberData(nameof(MethodWithArgsData), 42)]
				public void TestMethod1a(int _) { }

				// Optional paramter, no argument from data source
				[MemberData(nameof(FieldData))]
				[MemberData(nameof(PropertyData))]
				[MemberData(nameof(MethodData))]
				[MemberData(nameof(MethodWithArgsData), 42)]
				public void TestMethod1b(int _1, int _2 = 0) { }

				// Params array, no argument from data source
				[MemberData(nameof(FieldData))]
				[MemberData(nameof(PropertyData))]
				[MemberData(nameof(MethodData))]
				[MemberData(nameof(MethodWithArgsData), 42)]
				public void TestMethod1c(int _1, params int[] _2) { }

				// Generic match
				[MemberData(nameof(FieldData))]
				[MemberData(nameof(PropertyData))]
				[MemberData(nameof(MethodData))]
				[MemberData(nameof(MethodWithArgsData), 42)]
				public void TestMethod1d<T>(T _) { }

				// Generic nullable match
				[MemberData(nameof(FieldData))]
				[MemberData(nameof(PropertyData))]
				[MemberData(nameof(MethodData))]
				[MemberData(nameof(MethodWithArgsData), 42)]
				public void TestMethod1e<T>(T? _) { }

				public static TheoryDataRow<int, int>[] FieldData2 = new TheoryDataRow<int, int>[0];
				public static TheoryDataRow<int, int>[] PropertyData2 => new TheoryDataRow<int, int>[0];
				public static TheoryDataRow<int, int>[] MethodData2() => new TheoryDataRow<int, int>[0];
				public static TheoryDataRow<int, int>[] MethodWithArgsData2(int _) => new TheoryDataRow<int, int>[0];

				// Params array, single non-array argument from data source
				[MemberData(nameof(FieldData2))]
				[MemberData(nameof(PropertyData2))]
				[MemberData(nameof(MethodData2))]
				[MemberData(nameof(MethodWithArgsData2), 42)]
				public void TestMethod1f(int _1, params int[] _2) { }

				// Too many arguments
				[{|#0:MemberData(nameof(FieldData2))|}]
				[{|#1:MemberData(nameof(PropertyData2))|}]
				[{|#2:MemberData(nameof(MethodData2))|}]
				[{|#3:MemberData(nameof(MethodWithArgsData2), 42)|}]
				public void TestMethod1g(int _) { }

				public static TheoryDataRow<int, string[], string>[] FieldData3 = new TheoryDataRow<int, string[], string>[0];
				public static TheoryDataRow<int, string[], string>[] PropertyData3 => new TheoryDataRow<int, string[], string>[0];
				public static TheoryDataRow<int, string[], string>[] MethodData3() => new TheoryDataRow<int, string[], string>[0];
				public static TheoryDataRow<int, string[], string>[] MethodWithArgsData3(int _) => new TheoryDataRow<int, string[], string>[0];

				// Extra parameter type on data source
				[{|#4:MemberData(nameof(FieldData3))|}]
				[{|#5:MemberData(nameof(PropertyData3))|}]
				[{|#6:MemberData(nameof(MethodData3))|}]
				[{|#7:MemberData(nameof(MethodWithArgsData3), 42)|}]
				public void TestMethod1h(int _1, params string[] _2) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1038").WithLocation(0).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1038").WithLocation(1).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1038").WithLocation(2).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1038").WithLocation(3).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1038").WithLocation(4).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1038").WithLocation(5).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1038").WithLocation(6).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1038").WithLocation(7).WithArguments("Xunit.TheoryDataRow"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source, expected);
	}
}
