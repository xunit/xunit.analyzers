using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

public class ClassDataAttributeMustPointAtValidClassTests
{
	const string SupportedV2 = "IEnumerable<object[]>";
	const string SupportedV3 = "IEnumerable<object[]>, IAsyncEnumerable<object[]>, IEnumerable<ITheoryDataRow>, or IAsyncEnumerable<ITheoryDataRow>";

	public class SuccessCases
	{
		[Fact]
		public async ValueTask v2_only()
		{
			var source = /* lang=c#-test */ """
				using System.Collections;
				using System.Collections.Generic;
				using Xunit;

				class DataClassObjectArray: IEnumerable<object[]> {
					public IEnumerator<object[]> GetEnumerator() => null;
					IEnumerator IEnumerable.GetEnumerator() => null;
				}

				public class TestClass {
					[Theory]
					[ClassData(typeof(DataClassObjectArray))]
					public void TestMethod(int n) { }
				}
				""";

			await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp7_1, source);
		}

		[Fact]
		public async ValueTask v3_only()
		{
			var source = /* lang=c#-test */ """
				#nullable enable

				using System.Collections;
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

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

				public class TestClass {
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

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, source);
		}
	}

	public class X1007_ClassDataAttributeMustPointAtValidClass
	{
		[Fact]
		public async ValueTask v2_only()
		{
			var source = /* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass_Async : IAsyncEnumerable<object[]> {
					public IAsyncEnumerator<object[]> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}

				public class TestClass {
					[Theory]
					[{|#0:ClassData(typeof(DataClass_Async))|}]
					public void TestMethod(int n) { }
				}
				""";
			var expected = Verify.Diagnostic("xUnit1007").WithLocation(0).WithArguments("DataClass_Async", SupportedV2);

			await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp7_1, source, expected);
		}

		[Fact]
		public async ValueTask v2_and_v3()
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

#if ROSLYN_LATEST && !NETFRAMEWORK

		[Fact]
		public async ValueTask v3_only()
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
					[{|#0:ClassData<DataClass_Enumerable_Object>|}]
					[{|#1:ClassData<DataClass_Abstract>|}]
					[{|#2:ClassData<DataClass_NoParameterlessCtor>|}]
					[{|#3:ClassData<DataClass_InternalCtor>|}]
					[{|#4:ClassData<DataClass_PrivateCtor>|}]
					public void TestMethod(int n) { }
				}
				""";
			var expectedV3 = new[] {
				Verify.Diagnostic("xUnit1007").WithLocation(0).WithArguments("DataClass_Enumerable_Object", SupportedV3),
				Verify.Diagnostic("xUnit1007").WithLocation(1).WithArguments("DataClass_Abstract", SupportedV3),
				Verify.Diagnostic("xUnit1007").WithLocation(2).WithArguments("DataClass_NoParameterlessCtor", SupportedV3),
				Verify.Diagnostic("xUnit1007").WithLocation(3).WithArguments("DataClass_InternalCtor", SupportedV3),
				Verify.Diagnostic("xUnit1007").WithLocation(4).WithArguments("DataClass_PrivateCtor", SupportedV3),
			};

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp11, source, expectedV3);
		}

#endif  // ROSLYN_LATEST && !NETFRAMEWORK
	}

	public class X1037_TheoryDataTypeArgumentsMustMatchTestMethodParameters_TooFewTypeParameters
	{
		[Fact]
		public async Task v3_only()
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
				}
				""";
			var expected = Verify.Diagnostic("xUnit1037").WithLocation(0).WithArguments("Xunit.TheoryDataRow");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7_1, source, expected);
		}
	}

	public class X1038_TheoryDataTypeArgumentsMustMatchTestMethodParameters_ExtraTypeParameters
	{
		[Fact]
		public async Task v3_only()
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

	public class X1039_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes
	{
		[Fact]
		public async Task v3_only()
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

	public class X1040_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability
	{
		[Fact]
		public async Task v3_only()
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

	public class X1050_ClassDataTheoryDataRowIsRecommendedForStronglyTypedAnalysis
	{
		[Fact]
		public async Task v3_only()
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
}
