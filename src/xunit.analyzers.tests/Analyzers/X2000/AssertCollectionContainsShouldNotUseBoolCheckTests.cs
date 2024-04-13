using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertCollectionContainsShouldNotUseBoolCheck>;

public class AssertCollectionContainsShouldNotUseBoolCheckTests
{
	public static TheoryData<string> Collections = new()
	{
		"new System.Collections.Generic.List<int>()",
		"new System.Collections.Generic.HashSet<int>()",
		"new System.Collections.ObjectModel.Collection<int>()",
	};
	public static TheoryData<string> Enumerables = new()
	{
		"new int[0]",
		"System.Linq.Enumerable.Empty<int>()",
	};

	[Theory]
	[MemberData(nameof(Collections))]
	public async Task FindsWarningForTrueCollectionContainsCheck(string collection)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.True({collection}.Contains(1));
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 9, 4, 40 + collection.Length)
				.WithArguments("Assert.True()", Constants.Asserts.Contains);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Collections))]
	public async Task FindsWarningForFalseCollectionContainsCheck(string collection)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.False({collection}.Contains(1));
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 9, 4, 41 + collection.Length)
				.WithArguments("Assert.False()", Constants.Asserts.DoesNotContain);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Enumerables))]
	public async Task FindsWarningForTrueLinqContainsCheck(string enumerable)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.True({enumerable}.Contains(1));
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(6, 9, 6, 40 + enumerable.Length)
				.WithArguments("Assert.True()", Constants.Asserts.Contains);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Enumerables))]
	public async Task FindsWarningForTrueLinqContainsCheckWithEqualityComparer(string enumerable)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.True({enumerable}.Contains(1, System.Collections.Generic.EqualityComparer<int>.Default));
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(6, 9, 6, 98 + enumerable.Length)
				.WithArguments("Assert.True()", Constants.Asserts.Contains);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Enumerables))]
	public async Task FindsWarningForFalseLinqContainsCheck(string enumerable)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.False({enumerable}.Contains(1));
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(6, 9, 6, 41 + enumerable.Length)
				.WithArguments("Assert.False()", Constants.Asserts.DoesNotContain);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Enumerables))]
	public async Task FindsWarningForFalseLinqContainsCheckWithEqualityComparer(string enumerable)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.False({enumerable}.Contains(1, System.Collections.Generic.EqualityComparer<int>.Default));
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(6, 9, 6, 99 + enumerable.Length)
				.WithArguments("Assert.False()", Constants.Asserts.DoesNotContain);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Collections))]
	public async Task DoesNotFindWarningForTrueCollectionContainsCheckWithAssertionMessage(string collection)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.True({collection}.Contains(1), ""Custom message"");
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Collections))]
	public async Task DoesNotFindWarningForFalseCollectionContainsCheckWithAssertionMessage(string collection)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.False({collection}.Contains(1), ""Custom message"");
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Enumerables))]
	public async Task DoesNotFindWarningForTrueLinqContainsCheckWithAssertionMessage(string enumerable)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.True({enumerable}.Contains(1), ""Custom message"");
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Enumerables))]
	public async Task DoesNotFindWarningForFalseLinqContainsCheckWithAssertionMessage(string enumerable)
	{
		var source = $@"
using System.Linq;

class TestClass {{
    void TestMethod() {{
        Xunit.Assert.False({enumerable}.Contains(1), ""Custom message"");
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task DoesNotCrashForCollectionWithDifferentTypeParametersThanICollectionImplementation_ZeroParameters()
	{
		var source = @"
using System.Collections.Generic;

class IntList : List<int> { }

class TestClass {
    void TestMethod() {
        [|Xunit.Assert.False(new IntList().Contains(1))|];
    }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task DoesNotCrashForCollectionWithDifferentTypeParametersThanICollectionImplementation_TwoParameters()
	{
		var source = @"
using System.Collections.Generic;

class TestClass {
    void TestMethod() {
        Xunit.Assert.False(new Dictionary<int, int>().ContainsKey(1));
    }
}";

		await Verify.VerifyAnalyzer(source);
	}
}
