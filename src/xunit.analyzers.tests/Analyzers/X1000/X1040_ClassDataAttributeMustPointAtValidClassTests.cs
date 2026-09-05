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

			public class DataClass_TheoryDataRow : IAsyncEnumerable<TheoryDataRow<string?, int>> {
				public IAsyncEnumerator<TheoryDataRow<string?, int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class DataClass_Tuple : IAsyncEnumerable<(string?, int)> {
				public IAsyncEnumerator<(string?, int)> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class TestClass {
				[Theory]
				[ClassData(typeof(DataClass_TheoryDataRow))]
				[ClassData(typeof(DataClass_Tuple))]
				public void TestMethod({|#0:string|} s, int n) { }
			}

			public class TestClass_Generic {
				[Theory]
				[ClassData<DataClass_TheoryDataRow>]
				[ClassData<DataClass_Tuple>]
				public void TestMethod({|#10:string|} s, int n) { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "DataClass_TheoryDataRow", "s"),
			Verify.Diagnostic("xUnit1040").WithLocation(0).WithArguments("string?", "DataClass_Tuple", "s"),

			Verify.Diagnostic("xUnit1040").WithLocation(10).WithArguments("string?", "DataClass_TheoryDataRow", "s"),
			Verify.Diagnostic("xUnit1040").WithLocation(10).WithArguments("string?", "DataClass_Tuple", "s"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp11, source, expected);
	}
}
