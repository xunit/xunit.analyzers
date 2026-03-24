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

			public class DataClass1 : IAsyncEnumerable<TheoryDataRow<int, double>> {
				public IAsyncEnumerator<TheoryDataRow<int, double>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class DataClass2 : IAsyncEnumerable<TheoryDataRow<int, double[], long>> {
				public IAsyncEnumerator<TheoryDataRow<int, double[], long>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class TestClass {
				[Theory]
				[{|#0:ClassData(typeof(DataClass1))|}]
				public void TestMethod1(int n) { }

				[Theory]
				[ClassData(typeof(DataClass1))]
				[{|#1:ClassData(typeof(DataClass2))|}]
				public void TestMethod2(int n, params double[] d) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1038").WithLocation(0).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1038").WithLocation(1).WithArguments("Xunit.TheoryDataRow"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7_1, source, expected);
	}
}
