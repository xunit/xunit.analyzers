using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

public class X1037_ClassDataAttributeMustPointAtValidClassTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Threading;
			using Xunit;

			public class DataClass_TheoryDataRow : IAsyncEnumerable<TheoryDataRow<int>> {
				public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class DataClass_Tuple : IAsyncEnumerable<(int, string)> {
				public IAsyncEnumerator<(int, string)> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class TestClass {
				[Theory]
				[{|#0:ClassData(typeof(DataClass_TheoryDataRow))|}]
				[{|#1:ClassData(typeof(DataClass_Tuple))|}]
				public void TestMethod(int n, string f, double d) { }

				[Theory]
				[{|#10:ClassData<DataClass_TheoryDataRow>|}]
				[{|#11:ClassData<DataClass_Tuple>|}]
				public void TestMethod_Generic(int n, string f, double d) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1037").WithLocation(0).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1037").WithLocation(1).WithArguments("(int, string)"),

			Verify.Diagnostic("xUnit1037").WithLocation(10).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1037").WithLocation(11).WithArguments("(int, string)"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp11, source, expected);
	}
}
