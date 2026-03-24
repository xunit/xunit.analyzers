using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

public class X1040_ClassDataAttributeMustPointAtValidClassTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using System.Collections.Generic;
			using System.Threading;
			using Xunit;

			public class DataClass : IAsyncEnumerable<TheoryDataRow<string?>> {
				public IAsyncEnumerator<TheoryDataRow<string?>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class TestClass {
				[Theory]
				[ClassData(typeof(DataClass))]
				public void TestMethod({|#0:string|} s) { }
			}
			""";
		var expected = Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "DataClass", "s");

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source, expected);
	}
}
