using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

public class ClassDataAttributeMustPointAtValidClassTests
{
	static string TestMethodSource(string testMethodParams = "(int n)") => @$"
#nullable enable

using Xunit;

public class TestClass {{
    [Theory]
    [ClassData(typeof(DataClass))]
    public void TestMethod{testMethodParams} {{ }}
}}";

	public class SuccessCases
	{
		[Fact]
		public async Task SuccessCaseV2()
		{
			var dataClassSource = @"
using System.Collections;
using System.Collections.Generic;

class DataClass: IEnumerable<object[]> {
    public IEnumerator<object[]> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}";

			await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp9, [TestMethodSource(), dataClassSource]);
		}

		public static TheoryData<string, string> SuccessCasesV3Data = new()
		{
			// IEnumerable<ITheoryDataRow<int>> maps to int
			{ "(int n)", @"
using System.Collections;
using System.Collections.Generic;
using Xunit;

class DataClass: IEnumerable<TheoryDataRow<int>> {
    public IEnumerator<TheoryDataRow<int>> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}" },
			// IAsyncEnumerable<TheoryDataRow<int>> maps to int
			{ "(int n)", @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
    public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}" },
			// IAsyncEnumerable<TheoryDataRow<int>> with optional parameter
			{ "(int n, int p = 0)", @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
    public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}" },
			// IAsyncEnumerable<TheoryDataRow<int>> with params array (no values)
			{ "(int n, params int[] a)", @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
    public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}" },
			// IAsyncEnumerable<TheoryDataRow<int, string>> with params array (one value)
			{ "(int n, params string[] a)", @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int, string>> {
    public IAsyncEnumerator<TheoryDataRow<int, string>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}" },
			// IAsyncEnumerable<TheoryDataRow<int, string, string>> with params array (multiple values)
			{ "(int n, params string[] a)", @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int, string, string>> {
    public IAsyncEnumerator<TheoryDataRow<int, string, string>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}" },
			// IAsyncEnumerable<TheoryDataRow<int, string[]>> with params array (array for params array)
			{ "(int n, params string[] a)", @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int, string[]>> {
    public IAsyncEnumerator<TheoryDataRow<int, string[]>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}" },
			// IAsyncEnumerable<TheoryDataRow<int>> maps to generic T
			{ "<T>(T t)", @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
    public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}" },
			// IAsyncEnumerable<TheoryDataRow<int>> maps to generic T?
			{ "<T>(T? t)", @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
    public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}" },
			// IAsyncEnumerable<TheoryDataRow<(int, int)>> maps unnamed tuple to named tuple
			{ "((int x, int y) point)", @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<(int, int)>> {
    public IAsyncEnumerator<TheoryDataRow<(int, int)>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}" },
			// IAsyncEnumerable<TheoryDataRow<(int, int)>> maps tuples with mismatched names
			{ "((int x, int y) point)", @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<(int x1, int y1)>> {
    public IAsyncEnumerator<TheoryDataRow<(int x1, int y1)>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}" },
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
		public static TheoryData<string> FailureCasesData = new()
		{
			// Incorrect enumeration type (object instead of object[])
			@"
using System.Collections;
using System.Collections.Generic;

class DataClass: IEnumerable<object> {
    public IEnumerator<object> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}",
			// Abstract class
			@"
using System.Collections;
using System.Collections.Generic;

abstract class DataClass: IEnumerable<object[]> {
    public IEnumerator<object[]> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}",
			// Missing parameterless constructor
			@"
using System.Collections;
using System.Collections.Generic;

class DataClass: IEnumerable<object[]> {
    public DataClass(string parameter) { }
    public IEnumerator<object[]> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}",
			// Parameterless constructor is internal
			@"
using System.Collections;
using System.Collections.Generic;

class DataClass: IEnumerable<object[]> {
    internal DataClass() { }
    public IEnumerator<object[]> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}",
			// Parameterless constructor is private
			@"
using System.Collections;
using System.Collections.Generic;

class DataClass: IEnumerable<object[]> {
    private DataClass() { }
    public IEnumerator<object[]> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}",
		};

		[Theory]
		[MemberData(nameof(FailureCasesData))]
		public async Task FailureCases(string dataClassSource)
		{
			var expectedV2 =
				Verify
					.Diagnostic("xUnit1007")
					.WithSpan(8, 6, 8, 34)
					.WithArguments("DataClass", "IEnumerable<object[]>");
			var expectedV3 =
				Verify
					.Diagnostic("xUnit1007")
					.WithSpan(8, 6, 8, 34)
					.WithArguments("DataClass", "IEnumerable<object[]>, IAsyncEnumerable<object[]>, IEnumerable<ITheoryDataRow>, or IAsyncEnumerable<ITheoryDataRow>");

			await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp9, [TestMethodSource(), dataClassSource], expectedV2);
			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource(), dataClassSource], expectedV3);
		}

		[Fact]
		public async Task IAsyncEnumerableNotSupportedInV2()
		{
			var dataClassSource = @"
using System.Collections.Generic;
using System.Threading;

public class DataClass : IAsyncEnumerable<object[]> {
    public IAsyncEnumerator<object[]> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}";
			var expected =
				Verify
					.Diagnostic("xUnit1007")
					.WithSpan(8, 6, 8, 34)
					.WithArguments("DataClass", "IEnumerable<object[]>");

			await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp9, [TestMethodSource(), dataClassSource], expected);
		}
	}

	public class X1037_TheoryDataTypeArgumentsMustMatchTestMethodParameters_TooFewTypeParameters
	{
		[Fact]
		public async Task NotEnoughTypeParameters_Triggers()
		{
			var source = @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
    public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}";

			var expected =
				Verify
					.Diagnostic("xUnit1037")
					.WithSpan(8, 6, 8, 34)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Xunit.TheoryDataRow");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource("(int n, string f)"), source], expected);
		}
	}

	public class X1038_TheoryDataTypeArgumentsMustMatchTestMethodParameters_ExtraTypeParameters
	{
		[Fact]
		public async Task TooManyTypeParameters_Triggers()
		{
			var source = @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int, double>> {
    public IAsyncEnumerator<TheoryDataRow<int, double>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}";

			var expected =
				Verify
					.Diagnostic("xUnit1038")
					.WithSpan(8, 6, 8, 34)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Xunit.TheoryDataRow");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource("(int n)"), source], expected);
		}

		[Fact]
		public async Task ExtraDataPastParamsArray_Triggers()
		{
			var source = $@"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int, double[], long>> {{
    public IAsyncEnumerator<TheoryDataRow<int, double[], long>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}}";

			var expected =
				Verify
					.Diagnostic("xUnit1038")
					.WithSpan(8, 6, 8, 34)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("Xunit.TheoryDataRow");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource("(int n, params double[] d)"), source], expected);
		}
	}

	public class X1039_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes
	{
		[Fact]
		public async Task WithIncompatibleType_Triggers()
		{
			var source = @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int, string>> {
    public IAsyncEnumerator<TheoryDataRow<int, string>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}";

			var expected =
				Verify
					.Diagnostic("xUnit1039")
					.WithSpan(9, 35, 9, 41)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("string", "DataClass", "d");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource("(int n, double d)"), source], expected);
		}

		[Fact]
		public async Task WithExtraValueNotCompatibleWithParamsArray_Triggers()
		{
			var source = @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int, string, int>> {
    public IAsyncEnumerator<TheoryDataRow<int, string, int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}";

			var expected =
				Verify
					.Diagnostic("xUnit1039")
					.WithSpan(9, 42, 9, 50)
					.WithSeverity(DiagnosticSeverity.Error)
					.WithArguments("int", "DataClass", "s");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource("(int n, params string[] s)"), source], expected);
		}
	}

	public class X1040_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability
	{
		[Fact]
		public async Task ValidTheoryDataRowMemberWithMismatchedNullability_Triggers()
		{
			var source = @"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<string?>> {
    public IAsyncEnumerator<TheoryDataRow<string?>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}";

			var expected =
				Verify
					.Diagnostic("xUnit1040")
					.WithSpan(9, 28, 9, 34)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("string?", "DataClass", "s");

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource("(string s)"), source], expected);
		}
	}

	public class X1050_ClassDataTheoryDataRowIsRecommendedForStronglyTypedAnalysis
	{
		public static TheoryData<string> FailureCasesData = new()
		{
			// IEnumerable<object[]>
			@"
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class DataClass : IEnumerable<object[]> {
    public IEnumerator<object[]> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}",
			// IAsyncEnumerable<object[]>
			@"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<object[]> {
    public IAsyncEnumerator<object[]> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}",
			// IEnumerable<ITheoryDataRow>
			@"
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class DataClass : IEnumerable<ITheoryDataRow> {
    public IEnumerator<ITheoryDataRow> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}",
			// IAsyncEnumerable<ITheoryDataRow>
			@"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<ITheoryDataRow> {
    public IAsyncEnumerator<ITheoryDataRow> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}",
			// IEnumerable<TheoryDataRow>
			@"
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class DataClass : IEnumerable<TheoryDataRow> {
    public IEnumerator<TheoryDataRow> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}",
			// IAsyncEnumerable<TheoryDataRow>
			@"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow> {
    public IAsyncEnumerator<TheoryDataRow> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}",
		};

		[Theory]
		[MemberData(nameof(FailureCasesData))]
		public async Task FailureCases(string dataClassSource)
		{
			var expected =
				Verify
					.Diagnostic("xUnit1050")
					.WithSpan(8, 6, 8, 34);

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp9, [TestMethodSource(), dataClassSource], expected);
		}
	}
}
