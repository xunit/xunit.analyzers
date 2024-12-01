using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

public class ClassDataAttributeMustPointAtValidClassTests
{
	static string TestMethodSource(string testMethodParams = "(int n)") => string.Format(/* lang=c#-test */ """
		#nullable enable

		using Xunit;

		public class TestClass {{
			[Theory]
			[ClassData(typeof(DataClass))]
			public void TestMethod{0} {{ }}
		}}
		""", testMethodParams);

	public class SuccessCases
	{
		[Fact]
		public async Task SuccessCaseV2()
		{
			var dataClassSource = /* lang=c#-test */ """
				using System.Collections;
				using System.Collections.Generic;

				class DataClass: IEnumerable<object[]> {
					public IEnumerator<object[]> GetEnumerator() => null;
					IEnumerator IEnumerable.GetEnumerator() => null;
				}
				""";

			await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp9, [TestMethodSource(), dataClassSource]);
		}

		public static TheoryData<string, string> SuccessCasesV3Data = new()
		{
			// IEnumerable<ITheoryDataRow<int>> maps to int
			{
				/* lang=c#-test */ "(int n)",
				/* lang=c#-test */ """
				using System.Collections;
				using System.Collections.Generic;
				using Xunit;

				class DataClass: IEnumerable<TheoryDataRow<int>> {
					public IEnumerator<TheoryDataRow<int>> GetEnumerator() => null;
					IEnumerator IEnumerable.GetEnumerator() => null;
				}
				"""
			},
			// IAsyncEnumerable<TheoryDataRow<int>> maps to int
			{
				/* lang=c#-test */ "(int n)",
				/* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
					public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				"""
			},
			// IAsyncEnumerable<TheoryDataRow<int>> with optional parameter
			{
				/* lang=c#-test */ "(int n, int p = 0)",
				/* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
					public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				"""
			},
			// IAsyncEnumerable<TheoryDataRow<int>> with params array (no values)
			{
				/* lang=c#-test */ "(int n, params int[] a)",
				/* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
					public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				"""
			},
			// IAsyncEnumerable<TheoryDataRow<int, string>> with params array (one value)
			{
				/* lang=c#-test */ "(int n, params string[] a)",
				/* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<int, string>> {
					public IAsyncEnumerator<TheoryDataRow<int, string>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				"""
			},
			// IAsyncEnumerable<TheoryDataRow<int, string, string>> with params array (multiple values)
			{
				/* lang=c#-test */ "(int n, params string[] a)",
				/* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<int, string, string>> {
					public IAsyncEnumerator<TheoryDataRow<int, string, string>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				"""
			},
			// IAsyncEnumerable<TheoryDataRow<int, string[]>> with params array (array for params array)
			{
				/* lang=c#-test */ "(int n, params string[] a)",
				/* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<int, string[]>> {
					public IAsyncEnumerator<TheoryDataRow<int, string[]>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				"""
			},
			// IAsyncEnumerable<TheoryDataRow<int>> maps to generic T
			{
				/* lang=c#-test */ "<T>(T t)",
				/* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
					public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				"""
			},
			// IAsyncEnumerable<TheoryDataRow<int>> maps to generic T?
			{
				/* lang=c#-test */ "<T>(T? t)",
				/* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
					public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				"""
			},
			// IAsyncEnumerable<TheoryDataRow<(int, int)>> maps unnamed tuple to named tuple
			{
				/* lang=c#-test */ "((int x, int y) point)",
				/* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<(int, int)>> {
					public IAsyncEnumerator<TheoryDataRow<(int, int)>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				"""
			},
			// IAsyncEnumerable<TheoryDataRow<(int, int)>> maps tuples with mismatched names
			{
				/* lang=c#-test */ "((int x, int y) point)",
				/* lang=c#-test */ """

				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<(int x1, int y1)>> {
					public IAsyncEnumerator<TheoryDataRow<(int x1, int y1)>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				"""
			},
		};

		[Theory]
		[MemberData(nameof(SuccessCasesV3Data))]
		public async Task SuccessCasesV3(
			string methodParams,
			string dataClassSource)
		{
			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource(methodParams), dataClassSource]);
		}
	}

	public class X1007_ClassDataAttributeMustPointAtValidClass
	{
		public static TheoryData<string> FailureCasesData =
		[
			// Incorrect enumeration type (object instead of object[])
			/* lang=c#-test */
			"""
			using System.Collections;
			using System.Collections.Generic;

			class DataClass: IEnumerable<object> {
				public IEnumerator<object> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}
			""",

			// Abstract class
			/* lang=c#-test */
			"""
			using System.Collections;
			using System.Collections.Generic;

			abstract class DataClass: IEnumerable<object[]> {
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}
			""",

			// Missing parameterless constructor
			/* lang=c#-test */
			"""
			using System.Collections;
			using System.Collections.Generic;

			class DataClass: IEnumerable<object[]> {
				public DataClass(string parameter) { }
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}
			""",

			// Parameterless constructor is internal
			/* lang=c#-test */
			"""
			using System.Collections;
			using System.Collections.Generic;

			class DataClass: IEnumerable<object[]> {
				internal DataClass() { }
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}
			""",

			// Parameterless constructor is private
			/* lang=c#-test */
			"""
			using System.Collections;
			using System.Collections.Generic;

			class DataClass: IEnumerable<object[]> {
				private DataClass() { }
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}
			""",
		];

		[Theory]
		[MemberData(nameof(FailureCasesData))]
		public async Task FailureCases(string dataClassSource)
		{
			var expectedV2 = Verify.Diagnostic("xUnit1007").WithSpan(7, 3, 7, 31).WithArguments("DataClass", "IEnumerable<object[]>");
			var expectedV3 = Verify.Diagnostic("xUnit1007").WithSpan(7, 3, 7, 31).WithArguments("DataClass", "IEnumerable<object[]>, IAsyncEnumerable<object[]>, IEnumerable<ITheoryDataRow>, or IAsyncEnumerable<ITheoryDataRow>");

			await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp9, [TestMethodSource(), dataClassSource], expectedV2);
			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource(), dataClassSource], expectedV3);
		}

		[Fact]
		public async Task IAsyncEnumerableNotSupportedInV2()
		{
			var dataClassSource = /* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;

				public class DataClass : IAsyncEnumerable<object[]> {
					public IAsyncEnumerator<object[]> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				""";
			var expected = Verify.Diagnostic("xUnit1007").WithSpan(7, 3, 7, 31).WithArguments("DataClass", "IEnumerable<object[]>");

			await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp9, [TestMethodSource(), dataClassSource], expected);
		}
	}

	public class X1037_TheoryDataTypeArgumentsMustMatchTestMethodParameters_TooFewTypeParameters
	{
		[Fact]
		public async Task NotEnoughTypeParameters_Triggers()
		{
			var dataClassSource = /* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
					public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				""";
			var expected = Verify.Diagnostic("xUnit1037").WithSpan(7, 3, 7, 31).WithArguments("Xunit.TheoryDataRow");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource("(int n, string f)"), dataClassSource], expected);
		}
	}

	public class X1038_TheoryDataTypeArgumentsMustMatchTestMethodParameters_ExtraTypeParameters
	{
		[Fact]
		public async Task TooManyTypeParameters_Triggers()
		{
			var dataClassSource = /* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<int, double>> {
					public IAsyncEnumerator<TheoryDataRow<int, double>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				""";
			var expected = Verify.Diagnostic("xUnit1038").WithSpan(7, 3, 7, 31).WithArguments("Xunit.TheoryDataRow");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource("(int n)"), dataClassSource], expected);
		}

		[Fact]
		public async Task ExtraDataPastParamsArray_Triggers()
		{
			var dataClassSource = /* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<int, double[], long>> {
					public IAsyncEnumerator<TheoryDataRow<int, double[], long>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				""";
			var expected = Verify.Diagnostic("xUnit1038").WithSpan(7, 3, 7, 31).WithArguments("Xunit.TheoryDataRow");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource("(int n, params double[] d)"), dataClassSource], expected);
		}
	}

	public class X1039_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes
	{
		[Fact]
		public async Task WithIncompatibleType_Triggers()
		{
			var dataClassSource = /* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<int, string>> {
					public IAsyncEnumerator<TheoryDataRow<int, string>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				""";
			var expected = Verify.Diagnostic("xUnit1039").WithSpan(8, 32, 8, 38).WithArguments("string", "DataClass", "d");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource("(int n, double d)"), dataClassSource], expected);
		}

		[Fact]
		public async Task WithExtraValueNotCompatibleWithParamsArray_Triggers()
		{
			var dataClassSource = /* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<int, string, int>> {
					public IAsyncEnumerator<TheoryDataRow<int, string, int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				""";
			var expected = Verify.Diagnostic("xUnit1039").WithSpan(8, 39, 8, 47).WithArguments("int", "DataClass", "s");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource("(int n, params string[] s)"), dataClassSource], expected);
		}
	}

	public class X1040_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability
	{
		[Fact]
		public async Task ValidTheoryDataRowMemberWithMismatchedNullability_Triggers()
		{
			var dataClassSource = /* lang=c#-test */ """
				using System.Collections.Generic;
				using System.Threading;
				using Xunit;

				public class DataClass : IAsyncEnumerable<TheoryDataRow<string?>> {
					public IAsyncEnumerator<TheoryDataRow<string?>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
				}
				""";
			var expected = Verify.Diagnostic("xUnit1040").WithSpan(8, 25, 8, 31).WithArguments("string?", "DataClass", "s");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource("(string s)"), dataClassSource], expected);
		}
	}

	public class X1050_ClassDataTheoryDataRowIsRecommendedForStronglyTypedAnalysis
	{
		public static TheoryData<string> FailureCasesData =
		[
			// IEnumerable<object[]>
			/* lang=c#-test */ """
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			public class DataClass : IEnumerable<object[]> {
				public IEnumerator<object[]> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}
			""",

			// IAsyncEnumerable<object[]>
			/* lang=c#-test */
			"""
			using System.Collections.Generic;
			using System.Threading;
			using Xunit;

			public class DataClass : IAsyncEnumerable<object[]> {
				public IAsyncEnumerator<object[]> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}
			""",

			// IEnumerable<ITheoryDataRow>
			/* lang=c#-test */
			"""
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			public class DataClass : IEnumerable<ITheoryDataRow> {
				public IEnumerator<ITheoryDataRow> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}
			""",

			// IAsyncEnumerable<ITheoryDataRow>
			/* lang=c#-test */
			"""
			using System.Collections.Generic;
			using System.Threading;
			using Xunit;

			public class DataClass : IAsyncEnumerable<ITheoryDataRow> {
				public IAsyncEnumerator<ITheoryDataRow> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}
			""",

			// IEnumerable<TheoryDataRow>
			/* lang=c#-test */
			"""
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			public class DataClass : IEnumerable<TheoryDataRow> {
				public IEnumerator<TheoryDataRow> GetEnumerator() => null;
				IEnumerator IEnumerable.GetEnumerator() => null;
			}
			""",

			// IAsyncEnumerable<TheoryDataRow>
			/* lang=c#-test */
			"""
			using System.Collections.Generic;
			using System.Threading;
			using Xunit;

			public class DataClass : IAsyncEnumerable<TheoryDataRow> {
				public IAsyncEnumerator<TheoryDataRow> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
			}
			""",
		];

		[Theory]
		[MemberData(nameof(FailureCasesData))]
		public async Task FailureCases(string dataClassSource)
		{
			var expected = Verify.Diagnostic("xUnit1050").WithSpan(7, 3, 7, 31);

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource(), dataClassSource], expected);
		}
	}
}
