using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

public class X1038_ClassDataAttributeMustPointAtValidClassTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Threading;
			using Xunit;

			public class DataClass1_TheoryDataRow : IAsyncEnumerable<TheoryDataRow<int, double>> {
				public IAsyncEnumerator<TheoryDataRow<int, double>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class DataClass1_Tuple : IAsyncEnumerable<(int, double)> {
				public IAsyncEnumerator<(int, double)> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class DataClass2_TheoryDataRow : IAsyncEnumerable<TheoryDataRow<int, double[], long>> {
				public IAsyncEnumerator<TheoryDataRow<int, double[], long>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class DataClass2_Tuple : IAsyncEnumerable<(int, double[], long)> {
				public IAsyncEnumerator<(int, double[], long)> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class TestClass {
				[Theory]
				[{|#0:ClassData(typeof(DataClass1_TheoryDataRow))|}]
				[{|#1:ClassData(typeof(DataClass1_Tuple))|}]
				public void TestMethod1(int n) { }

				[Theory]
				[ClassData(typeof(DataClass1_TheoryDataRow))]
				[ClassData(typeof(DataClass1_Tuple))]
				[{|#2:ClassData(typeof(DataClass2_TheoryDataRow))|}]
				[{|#3:ClassData(typeof(DataClass2_Tuple))|}]
				public void TestMethod2(int n, params double[] d) { }
			}

			public class TestClass_Generic {
				[Theory]
				[{|#10:ClassData<DataClass1_TheoryDataRow>|}]
				[{|#11:ClassData<DataClass1_Tuple>|}]
				public void TestMethod1(int n) { }

				[Theory]
				[ClassData<DataClass1_TheoryDataRow>]
				[ClassData<DataClass1_Tuple>]
				[{|#12:ClassData<DataClass2_TheoryDataRow>|}]
				[{|#13:ClassData<DataClass2_Tuple>|}]
				public void TestMethod2(int n, params double[] d) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1038").WithLocation(0).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1038").WithLocation(1).WithArguments("(int, double)"),
			Verify.Diagnostic("xUnit1038").WithLocation(2).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1038").WithLocation(3).WithArguments("(int, double[], long)"),

			Verify.Diagnostic("xUnit1038").WithLocation(10).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1038").WithLocation(11).WithArguments("(int, double)"),
			Verify.Diagnostic("xUnit1038").WithLocation(12).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1038").WithLocation(13).WithArguments("(int, double[], long)"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp11, source, expected);
	}
}
