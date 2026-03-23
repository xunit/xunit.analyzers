using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.MemberDataShouldReferenceValidMember>;

public class X1037_MemberDataShouldReferenceValidMemberTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class DerivedTheoryData<T, U> : TheoryData<T> { }

			public class TestClass {
				public static TheoryData<int> FieldData = new TheoryData<int>();
				public static TheoryData<int> PropertyData => new TheoryData<int>();
				public static TheoryData<int> MethodData() => new TheoryData<int>();
				public static TheoryData<int> MethodDataWithArgs(int n) => new TheoryData<int>();

				[{|#0:MemberData(nameof(FieldData))|}]
				[{|#1:MemberData(nameof(PropertyData))|}]
				[{|#2:MemberData(nameof(MethodData))|}]
				[{|#3:MemberData(nameof(MethodDataWithArgs), 42)|}]
				public void TestMethod1(int n, string f) { }

				public static DerivedTheoryData<int, string> DerivedFieldData = new DerivedTheoryData<int, string>();
				public static DerivedTheoryData<int, string> DerivedPropertyData => new DerivedTheoryData<int, string>();
				public static DerivedTheoryData<int, string> DerivedMethodData() => new DerivedTheoryData<int, string>();
				public static DerivedTheoryData<int, string> DerivedMethodDataWithArgs(int n) => new DerivedTheoryData<int, string>();

				[{|#10:MemberData(nameof(DerivedFieldData))|}]
				[{|#11:MemberData(nameof(DerivedPropertyData))|}]
				[{|#12:MemberData(nameof(DerivedMethodData))|}]
				[{|#13:MemberData(nameof(DerivedMethodDataWithArgs), 42)|}]
				public void TestMethod3(int n, string f) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1037").WithLocation(0).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1037").WithLocation(1).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1037").WithLocation(2).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1037").WithLocation(3).WithArguments("Xunit.TheoryData"),

			Verify.Diagnostic("xUnit1037").WithLocation(10).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1037").WithLocation(11).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1037").WithLocation(12).WithArguments("Xunit.TheoryData"),
			Verify.Diagnostic("xUnit1037").WithLocation(13).WithArguments("Xunit.TheoryData"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {
				public static IEnumerable<TheoryDataRow<int>> NullFieldData = null;
				public static IEnumerable<TheoryDataRow<int>> NullPropertyData => null;
				public static IEnumerable<TheoryDataRow<int>> NullMethodData() => null;
				public static IEnumerable<TheoryDataRow<int>> NullMethodDataWithArgs(int n) => null;

				[{|#0:MemberData(nameof(NullFieldData))|}]
				[{|#1:MemberData(nameof(NullPropertyData))|}]
				[{|#2:MemberData(nameof(NullMethodData))|}]
				[{|#3:MemberData(nameof(NullMethodDataWithArgs), 42)|}]
				public void TestMethod2(int n, string f) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1037").WithLocation(0).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1037").WithLocation(1).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1037").WithLocation(2).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1037").WithLocation(3).WithArguments("Xunit.TheoryDataRow"),
		};

		await Verify.VerifyAnalyzerV3(source, expected);
	}
}
