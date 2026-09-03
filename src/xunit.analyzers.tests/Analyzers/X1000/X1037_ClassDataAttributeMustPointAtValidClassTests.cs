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

			public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
				public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class TestClass {
				[Theory]
				[{|#0:ClassData(typeof(DataClass))|}]
				public void TestMethod(int n, string f) { }

				[Theory]
				[{|#1:ClassData<DataClass>|}]
				public void TestMethod_Generic(int n, string f) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1037").WithLocation(0).WithArguments("Xunit.TheoryDataRow"),
			Verify.Diagnostic("xUnit1037").WithLocation(1).WithArguments("Xunit.TheoryDataRow"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp11, source, expected);
	}
}
