using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.ClassDataAttributeMustPointAtValidClass>;

public class ClassDataAttributeMustPointAtValidClassTests
{
	static readonly string TestMethodSource = @"
using Xunit;

public class TestClass {
    [Theory]
    [ClassData(typeof(DataClass))]
    public void TestMethod() { }
}";

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

		await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp7_1, [TestMethodSource, dataClassSource]);
	}

	public static TheoryData<string> SuccessCasesV3Data = new()
	{
		// IEnumerable<ITheoryDataRow<>>
		@"
using System.Collections;
using System.Collections.Generic;
using Xunit;

class DataClass: IEnumerable<TheoryDataRow<int>> {
    public IEnumerator<TheoryDataRow<int>> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}",
		// IAsyncEnumerable<TheoryDataRow<>>
		@"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<TheoryDataRow<int>> {
    public IAsyncEnumerator<TheoryDataRow<int>> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}",
		// IAsyncEnumerable<DerivedTheoryDataRow>
		@"
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class DataClass : IAsyncEnumerable<DerivedTheoryDataRow> {
    public IAsyncEnumerator<DerivedTheoryDataRow> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}

public class DerivedTheoryDataRow : TheoryDataRow<int> {
    public DerivedTheoryDataRow(int t) : base(t) { }
}",
	};

	[Theory]
	[MemberData(nameof(SuccessCasesV3Data))]
	public async Task SuccessCasesV3(string dataClassSource)
	{
		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7_1, [TestMethodSource, dataClassSource]);
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
					.WithSpan(6, 23, 6, 32)
					.WithArguments("DataClass", "IEnumerable<object[]>");
			var expectedV3 =
				Verify
					.Diagnostic("xUnit1007")
					.WithSpan(6, 23, 6, 32)
					.WithArguments("DataClass", "IEnumerable<object[]>, IAsyncEnumerable<object[]>, IEnumerable<ITheoryDataRow>, or IAsyncEnumerable<ITheoryDataRow>");

			await Verify.VerifyAnalyzerV2([TestMethodSource, dataClassSource], expectedV2);
			await Verify.VerifyAnalyzerV3([TestMethodSource, dataClassSource], expectedV3);
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
					.WithSpan(6, 23, 6, 32)
					.WithArguments("DataClass", "IEnumerable<object[]>");

			await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp7_1, [TestMethodSource, dataClassSource], expected);
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
					.WithSpan(6, 23, 6, 32);

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp7_1, [TestMethodSource, dataClassSource], expected);
		}
	}
}
