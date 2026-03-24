using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

public class X1050_ClassDataAttributeMustPointAtValidClassTests
{
	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System.Collections;
			using System.Collections.Generic;
			using System.Threading;
			using Xunit;

			public class DataClass_ObjectArray : IEnumerable<object[]> {
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			public class DataClass_ObjectArray_Async : IAsyncEnumerable<object[]> {
				public IAsyncEnumerator<object[]> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class DataClass_ITheoryDataRow : IEnumerable<ITheoryDataRow> {
				public IEnumerator<ITheoryDataRow> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			public class DataClass_ITheoryDataRow_Async : IAsyncEnumerable<ITheoryDataRow> {
				public IAsyncEnumerator<ITheoryDataRow> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class DataClass_TheoryDataRow : IEnumerable<TheoryDataRow> {
				public IEnumerator<TheoryDataRow> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			public class DataClass_TheoryDataRow_Async : IAsyncEnumerable<TheoryDataRow> {
				public IAsyncEnumerator<TheoryDataRow> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class TestClass {
				[Theory]
				[{|xUnit1050:ClassData(typeof(DataClass_ObjectArray))|}]
				[{|xUnit1050:ClassData(typeof(DataClass_ObjectArray_Async))|}]
				[{|xUnit1050:ClassData(typeof(DataClass_ITheoryDataRow))|}]
				[{|xUnit1050:ClassData(typeof(DataClass_ITheoryDataRow_Async))|}]
				[{|xUnit1050:ClassData(typeof(DataClass_TheoryDataRow))|}]
				[{|xUnit1050:ClassData(typeof(DataClass_TheoryDataRow_Async))|}]
				public void TestMethod(int n) { }
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7_1, source);
	}
}
