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

			public class DataClass1 : IAsyncEnumerable<TheoryDataRow<int, string>> {
				public IAsyncEnumerator<TheoryDataRow<int, string>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class DataClass2 : IAsyncEnumerable<TheoryDataRow<int, string, int>> {
				public IAsyncEnumerator<TheoryDataRow<int, string, int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class TestClass {
				[Theory]
				[ClassData(typeof(DataClass1))]
				public void TestMethod1(int n, {|#0:double|} d) { }

				[Theory]
				[ClassData(typeof(DataClass2))]
				public void TestMethod2(int n, params {|#1:string[]|} s) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1039").WithLocation(0).WithArguments("string", "DataClass1", "d"),
			Verify.Diagnostic("xUnit1039").WithLocation(1).WithArguments("int", "DataClass2", "s"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7_1, source, expected);
	}
}
