using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

public class X1007_ClassDataAttributeMustPointAtValidClassTests
{
	const string SupportedV2 = "IEnumerable<object[]>";
	const string SupportedV3 = "IEnumerable<object[]>, IAsyncEnumerable<object[]>, IEnumerable<ITheoryDataRow>, or IAsyncEnumerable<ITheoryDataRow>";

	[Fact]
	public async ValueTask v2_only()
	{
		var source = /* lang=c#-test */ """
			using System.Collections;
			using System.Collections.Generic;
			using System.Threading;
			using Xunit;

			class DataClass: IEnumerable<object[]> {
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			class DataClass_Async : IAsyncEnumerable<object[]> {
				public IAsyncEnumerator<object[]> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class TestClass {
				[Theory]
				[ClassData(typeof(DataClass))]
				[{|#0:ClassData(typeof(DataClass_Async))|}]
				public void IAsyncEnumerable_Triggers(int n) { }
			}
			""";
		var expected = Verify.Diagnostic("xUnit1007").WithLocation(0).WithArguments("DataClass_Async", SupportedV2);

		await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp7_1, source, expected);
	}

	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			class DataClass_Enumerable_Object: IEnumerable<object> {
				public IEnumerator<object> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			abstract class DataClass_Abstract: IEnumerable<object[]> {
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			class DataClass_NoParameterlessCtor: IEnumerable<object[]> {
				public DataClass_NoParameterlessCtor(string parameter) { }
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			class DataClass_InternalCtor: IEnumerable<object[]> {
				internal DataClass_InternalCtor() { }
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			class DataClass_PrivateCtor: IEnumerable<object[]> {
				internal DataClass_PrivateCtor() { }
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			public class TestClass {
				[Theory]
				[{|#0:ClassData(typeof(DataClass_Enumerable_Object))|}]
				[{|#1:ClassData(typeof(DataClass_Abstract))|}]
				[{|#2:ClassData(typeof(DataClass_NoParameterlessCtor))|}]
				[{|#3:ClassData(typeof(DataClass_InternalCtor))|}]
				[{|#4:ClassData(typeof(DataClass_PrivateCtor))|}]
				public void TestMethod(int n) { }
			}
			""";
		var expectedV2 = new[] {
			Verify.Diagnostic("xUnit1007").WithLocation(0).WithArguments("DataClass_Enumerable_Object", SupportedV2),
			Verify.Diagnostic("xUnit1007").WithLocation(1).WithArguments("DataClass_Abstract", SupportedV2),
			Verify.Diagnostic("xUnit1007").WithLocation(2).WithArguments("DataClass_NoParameterlessCtor", SupportedV2),
			Verify.Diagnostic("xUnit1007").WithLocation(3).WithArguments("DataClass_InternalCtor", SupportedV2),
			Verify.Diagnostic("xUnit1007").WithLocation(4).WithArguments("DataClass_PrivateCtor", SupportedV2),
		};
		var expectedV3 = new[] {
			Verify.Diagnostic("xUnit1007").WithLocation(0).WithArguments("DataClass_Enumerable_Object", SupportedV3),
			Verify.Diagnostic("xUnit1007").WithLocation(1).WithArguments("DataClass_Abstract", SupportedV3),
			Verify.Diagnostic("xUnit1007").WithLocation(2).WithArguments("DataClass_NoParameterlessCtor", SupportedV3),
			Verify.Diagnostic("xUnit1007").WithLocation(3).WithArguments("DataClass_InternalCtor", SupportedV3),
			Verify.Diagnostic("xUnit1007").WithLocation(4).WithArguments("DataClass_PrivateCtor", SupportedV3),
		};

		await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp7_1, source, expectedV2);
		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7_1, source, expectedV3);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System.Collections;
			using System.Collections.Generic;
			using System.Threading;
			using Xunit;

			class DataClass_Enumerable_Object: IEnumerable<object> {
				public IEnumerator<object> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			abstract class DataClass_Abstract: IEnumerable<object[]> {
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			class DataClass_NoParameterlessCtor: IEnumerable<object[]> {
				public DataClass_NoParameterlessCtor(string parameter) { }
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			class DataClass_InternalCtor: IEnumerable<object[]> {
				internal DataClass_InternalCtor() { }
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			class DataClass_PrivateCtor: IEnumerable<object[]> {
				internal DataClass_PrivateCtor() { }
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			public class TestClass {
				[Theory]
				[{|#0:ClassData<DataClass_Enumerable_Object>|}]
				[{|#1:ClassData<DataClass_Abstract>|}]
				[{|#2:ClassData<DataClass_NoParameterlessCtor>|}]
				[{|#3:ClassData<DataClass_InternalCtor>|}]
				[{|#4:ClassData<DataClass_PrivateCtor>|}]
				public void TestMethod(int n) { }
			}

			#nullable enable

			class DataClass_TheoryDataRow_1: IEnumerable<TheoryDataRow<int>> {
				public IEnumerator<TheoryDataRow<int>> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			class DataClass_TheoryDataRow_1_Async : IAsyncEnumerable<TheoryDataRow<int>> {
				public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			class DataClass_TheoryDataRow_2: IEnumerable<TheoryDataRow<int, string>> {
				public IEnumerator<TheoryDataRow<int, string>> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			class DataClass_TheoryDataRow_2_Async : IAsyncEnumerable<TheoryDataRow<int, string>> {
				public IAsyncEnumerator<TheoryDataRow<int, string>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			class DataClass_TheoryDataRow_2A: IEnumerable<TheoryDataRow<int, string[]>> {
				public IEnumerator<TheoryDataRow<int, string[]>> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			class DataClass_TheoryDataRow_2A_Async : IAsyncEnumerable<TheoryDataRow<int, string[]>> {
				public IAsyncEnumerator<TheoryDataRow<int, string[]>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			class DataClass_TheoryDataRow_3: IEnumerable<TheoryDataRow<int, string, string>> {
				public IEnumerator<TheoryDataRow<int, string, string>> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}

			class DataClass_TheoryDataRow_3_Async : IAsyncEnumerable<TheoryDataRow<int, string, string>> {
				public IAsyncEnumerator<TheoryDataRow<int, string, string>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			class DataClass_Tuple_Named : IAsyncEnumerable<TheoryDataRow<(int x, int y)>> {
				public IAsyncEnumerator<TheoryDataRow<(int x, int y)>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			class DataClass_Tuple_Misnamed : IAsyncEnumerable<TheoryDataRow<(int a, int b)>> {
				public IAsyncEnumerator<TheoryDataRow<(int a, int b)>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			class DataClass_Tuple_Unnamed : IAsyncEnumerable<TheoryDataRow<(int, int)>> {
				public IAsyncEnumerator<TheoryDataRow<(int, int)>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}

			public class NullableTestClass {
				[Theory]
				[ClassData(typeof(DataClass_TheoryDataRow_1))]
				[ClassData(typeof(DataClass_TheoryDataRow_1_Async))]
				public void TestMethod1(int n) { }

				[Theory]
				[ClassData(typeof(DataClass_TheoryDataRow_1))]
				[ClassData(typeof(DataClass_TheoryDataRow_1_Async))]
				public void TestMethod1Generic1<T>(T t) { }

				[Theory]
				[ClassData(typeof(DataClass_TheoryDataRow_1))]
				[ClassData(typeof(DataClass_TheoryDataRow_1_Async))]
				public void TestMethod1Generic2<T>(T? t) { }

				[Theory]
				[ClassData(typeof(DataClass_TheoryDataRow_1))]
				[ClassData(typeof(DataClass_TheoryDataRow_1_Async))]
				[ClassData(typeof(DataClass_TheoryDataRow_2))]
				[ClassData(typeof(DataClass_TheoryDataRow_2_Async))]
				public void TestMethod2(int n, string s = "") { }

				[Theory]
				[ClassData(typeof(DataClass_TheoryDataRow_1))]
				[ClassData(typeof(DataClass_TheoryDataRow_1_Async))]
				[ClassData(typeof(DataClass_TheoryDataRow_2))]
				[ClassData(typeof(DataClass_TheoryDataRow_2_Async))]
				[ClassData(typeof(DataClass_TheoryDataRow_2A))]
				[ClassData(typeof(DataClass_TheoryDataRow_2A_Async))]
				[ClassData(typeof(DataClass_TheoryDataRow_3))]
				[ClassData(typeof(DataClass_TheoryDataRow_3_Async))]
				public void TestMethod3(int n, params string[] a) { }

				[Theory]
				[ClassData(typeof(DataClass_Tuple_Named))]
				[ClassData(typeof(DataClass_Tuple_Misnamed))]
				[ClassData(typeof(DataClass_Tuple_Unnamed))]
				public void TestMethod4((int x, int y) point) { }
			}
			""";
#if ROSLYN_LATEST && !NETFRAMEWORK  // This is here because otherwise `dotnet format` destroys the multi-line source string
		var expectedV3 = new[] {
			Verify.Diagnostic("xUnit1007").WithLocation(0).WithArguments("DataClass_Enumerable_Object", SupportedV3),
			Verify.Diagnostic("xUnit1007").WithLocation(1).WithArguments("DataClass_Abstract", SupportedV3),
			Verify.Diagnostic("xUnit1007").WithLocation(2).WithArguments("DataClass_NoParameterlessCtor", SupportedV3),
			Verify.Diagnostic("xUnit1007").WithLocation(3).WithArguments("DataClass_InternalCtor", SupportedV3),
			Verify.Diagnostic("xUnit1007").WithLocation(4).WithArguments("DataClass_PrivateCtor", SupportedV3),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp11, source, expectedV3);
#else
		Assert.NotNull(source);
		await Task.Yield();
#endif
	}
}
