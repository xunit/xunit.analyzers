using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertCollectionContainsShouldNotUseBoolCheck>;

public class AssertCollectionContainsShouldNotUseBoolCheckTests
{
	public static TheoryData<string> Collections =
	[
		"new System.Collections.Generic.List<int>()",
		"new System.Collections.Generic.HashSet<int>()",
		"new System.Collections.ObjectModel.Collection<int>()",
	];
	public static TheoryData<string> Enumerables =
	[
		"new int[0]",
		"System.Linq.Enumerable.Empty<int>()",
	];

	[Theory]
	[MemberData(nameof(Collections))]
	public async Task TrueCollectionContainsCheck_Triggers(string collection)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.True({0}.Contains(1))|}};
			    }}
			}}
			""", collection);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.True()", Constants.Asserts.Contains);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Collections))]
	public async Task FalseCollectionContainsCheck_Triggers(string collection)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.False({0}.Contains(1))|}};
			    }}
			}}
			""", collection);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Enumerables))]
	public async Task TrueLinqContainsCheck_Triggers(string enumerable)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.True({0}.Contains(1))|}};
			    }}
			}}
			""", enumerable);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.True()", Constants.Asserts.Contains);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Enumerables))]
	public async Task TrueLinqContainsCheckWithEqualityComparer_Triggers(string enumerable)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.True({0}.Contains(1, System.Collections.Generic.EqualityComparer<int>.Default))|}};
			    }}
			}}
			""", enumerable);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.True()", Constants.Asserts.Contains);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Enumerables))]
	public async Task FalseLinqContainsCheck_Triggers(string enumerable)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.False({0}.Contains(1))|}};
			    }}
			}}
			""", enumerable);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Enumerables))]
	public async Task FalseLinqContainsCheckWithEqualityComparer_Triggers(string enumerable)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.False({0}.Contains(1, System.Collections.Generic.EqualityComparer<int>.Default))|}};
			    }}
			}}
			""", enumerable);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.False()", Constants.Asserts.DoesNotContain);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Collections))]
	public async Task TrueCollectionContainsCheckWithAssertionMessage_DoesNotTrigger(string collection)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.True({0}.Contains(1), "Custom message");
			    }}
			}}
			""", collection);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Collections))]
	public async Task FalseCollectionContainsCheckWithAssertionMessage_DoesNotTrigger(string collection)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.False({0}.Contains(1), "Custom message");
			    }}
			}}
			""", collection);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Enumerables))]
	public async Task TrueLinqContainsCheckWithAssertionMessage_DoesNotTrigger(string enumerable)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.True({0}.Contains(1), "Custom message");
			    }}
			}}
			""", enumerable);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Enumerables))]
	public async Task FalseLinqContainsCheckWithAssertionMessage_DoesNotTrigger(string enumerable)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.False({0}.Contains(1), "Custom message");
			    }}
			}}
			""", enumerable);

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task CollectionWithDifferentTypeParametersThanICollectionImplementation_ZeroParameters_Triggers()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;

			class IntList : List<int> { }

			class TestClass {
			    void TestMethod() {
			        [|Xunit.Assert.False(new IntList().Contains(1))|];
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task CollectionWithDifferentTypeParametersThanICollectionImplementation_TwoParameters_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;

			class TestClass {
			    void TestMethod() {
			        Xunit.Assert.False(new Dictionary<int, int>().ContainsKey(1));
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
