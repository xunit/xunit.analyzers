using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

public class X1039_ClassDataAttributeMustPointAtValidClassTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Threading;
			using Xunit;

			public class DataClass1_TheoryDataRow : IAsyncEnumerable<TheoryDataRow<int, string>> {
				public IAsyncEnumerator<TheoryDataRow<int, string>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class DataClass1_Tuple : IAsyncEnumerable<(int, string)> {
				public IAsyncEnumerator<(int, string)> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class DataClass2_TheoryDataRow : IAsyncEnumerable<TheoryDataRow<int, string, int>> {
				public IAsyncEnumerator<TheoryDataRow<int, string, int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class DataClass2_Tuple : IAsyncEnumerable<(int, string, int)> {
				public IAsyncEnumerator<(int, string, int)> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class TestClass {
				[Theory]
				[ClassData(typeof(DataClass1_TheoryDataRow))]
				[ClassData(typeof(DataClass1_Tuple))]
				public void TestMethod1(int n, {|#0:double|} d) { }

				[Theory]
				[ClassData(typeof(DataClass2_TheoryDataRow))]
				[ClassData(typeof(DataClass2_Tuple))]
				public void TestMethod2(int n, params {|#1:string[]|} s) { }
			}

			public class TestClass_Generic {
				[Theory]
				[ClassData<DataClass1_TheoryDataRow>]
				[ClassData<DataClass1_Tuple>]
				public void TestMethod1(int n, {|#10:double|} d) { }

				[Theory]
				[ClassData<DataClass2_TheoryDataRow>]
				[ClassData<DataClass2_Tuple>]
				public void TestMethod2(int n, params {|#11:string[]|} s) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("string", "DataClass1_TheoryDataRow", "d"),
			Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("string", "DataClass1_Tuple", "d"),
			Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "DataClass2_TheoryDataRow", "s"),
			Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "DataClass2_Tuple", "s"),

			Verify.Diagnostic("xUnit1039").WithLocation(10).WithArguments("string", "DataClass1_TheoryDataRow", "d"),
			Verify.Diagnostic("xUnit1039").WithLocation(10).WithArguments("string", "DataClass1_Tuple", "d"),
			Verify.Diagnostic("xUnit1039").WithLocation(11).WithArguments("int", "DataClass2_TheoryDataRow", "s"),
			Verify.Diagnostic("xUnit1039").WithLocation(11).WithArguments("int", "DataClass2_Tuple", "s"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp11, source, expected);
	}
}
