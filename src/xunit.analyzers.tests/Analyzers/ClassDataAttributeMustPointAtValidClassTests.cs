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
	public async void SuccessCase()
	{
		var dataClassSource = @"
using System.Collections;
using System.Collections.Generic;

class DataClass: IEnumerable<object[]> {
    public IEnumerator<object[]> GetEnumerator() => null;
    IEnumerator IEnumerable.GetEnumerator() => null;
}";

		await Verify.VerifyAnalyzerAsync(new[] { TestMethodSource, dataClassSource });
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
	public async void FailureCase(string dataClassSource)
	{
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(6, 23, 6, 32)
				.WithArguments("DataClass");

		await Verify.VerifyAnalyzerAsync(new[] { TestMethodSource, dataClassSource }, expected);
	}
}
