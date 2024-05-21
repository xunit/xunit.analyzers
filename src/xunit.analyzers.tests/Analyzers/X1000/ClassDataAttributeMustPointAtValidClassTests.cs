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
	public async Task SuccessCase()
	{
		var dataClassSource = @"
using System.Collections;
using System.Collections.Generic;

class DataClass: IEnumerable<object[]> {
    public IEnumerator<object[]> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}";

		await Verify.VerifyAnalyzer(new[] { TestMethodSource, dataClassSource });
	}

	public static TheoryData<string> SuccessCasesV3Data = new()
	{
		// IAsyncEnumerable<object[]>
		@"
using System.Collections.Generic;
using System.Threading;

public class DataClass : IAsyncEnumerable<object[]> {
    public IAsyncEnumerator<object[]> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}",
		// IEnumerable<ITheoryDataRow>
		@"
using System.Collections;
using System.Collections.Generic;
using Xunit;

class DataClass: IEnumerable<ITheoryDataRow> {
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
	};

	[Theory]
	[MemberData(nameof(SuccessCasesV3Data))]
	public async Task SuccessCases_V3(string dataClassSource)
	{
		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, new[] { TestMethodSource, dataClassSource });
	}

	public static TheoryData<string> FailureCases = new()
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
	[MemberData(nameof(FailureCases))]
	public async Task FailureCase(string dataClassSource)
	{
		var expectedV2 =
			Verify
				.Diagnostic()
				.WithSpan(6, 23, 6, 32)
				.WithArguments("DataClass", "IEnumerable<object[]>");
		var expectedV3 =
			Verify
				.Diagnostic()
				.WithSpan(6, 23, 6, 32)
				.WithArguments("DataClass", "IEnumerable<object[]>, IAsyncEnumerable<object[]>, IEnumerable<ITheoryDataRow>, or IAsyncEnumerable<ITheoryDataRow>");

		await Verify.VerifyAnalyzerV2(new[] { TestMethodSource, dataClassSource }, expectedV2);
		await Verify.VerifyAnalyzerV3(new[] { TestMethodSource, dataClassSource }, expectedV3);
	}

	[Fact]
	public async Task IAsyncEnumerableSupportedOnlyInV3()
	{
		var dataClassSource = @"
using System.Collections.Generic;
using System.Threading;

public class DataClass : IAsyncEnumerable<object[]> {
    public IAsyncEnumerator<object[]> GetAsyncEnumerator(CancellationToken cancellationToken = default) => null;
}";
		var expectedV2 =
			Verify
				.Diagnostic()
				.WithSpan(6, 23, 6, 32)
				.WithArguments("DataClass", "IEnumerable<object[]>");

		await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp8, new[] { TestMethodSource, dataClassSource }, expectedV2);
		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, new[] { TestMethodSource, dataClassSource });
	}
}
