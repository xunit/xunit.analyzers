using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertStringEqualityCheckShouldNotUseBoolCheck>;

public class AssertStringEqualityCheckShouldNotUseBoolCheckTest
{
	public static TheoryData<string, string> Methods_WithReplacement = new()
	{
		{ Constants.Asserts.True, Constants.Asserts.Equal },
		{ Constants.Asserts.False, Constants.Asserts.NotEqual },
	};
	public static TheoryData<StringComparison> SupportedStringComparisons =
	[
		StringComparison.Ordinal,
		StringComparison.OrdinalIgnoreCase,
	];
	public static TheoryData<StringComparison> UnsupportedStringComparisons =
	[
		StringComparison.CurrentCulture,
		StringComparison.CurrentCultureIgnoreCase,
		StringComparison.InvariantCulture,
		StringComparison.InvariantCultureIgnoreCase,
	];
	public static TheoryData<StringComparison> AllStringComparisons =
	[
		StringComparison.Ordinal,
		StringComparison.OrdinalIgnoreCase,
		StringComparison.CurrentCulture,
		StringComparison.CurrentCultureIgnoreCase,
		StringComparison.InvariantCulture,
		StringComparison.InvariantCultureIgnoreCase,
	];

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async Task ForInstanceEqualsCheck_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.{0}("abc".Equals("a"))|}};
			    }}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(SupportedStringComparisons))]
	public async Task ForTrueInstanceEqualsCheck_WithSupportedStringComparison_Triggers(StringComparison comparison)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.True("abc".Equals("a", System.StringComparison.{0}))|}};
			    }}
			}}
			""", comparison);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.True()", Constants.Asserts.Equal);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(UnsupportedStringComparisons))]
	public async Task ForTrueInstanceEqualsCheck_WithUnsupportedStringComparison_DoesNotTrigger(StringComparison comparison)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.True("abc".Equals("a", System.StringComparison.{0}));
			    }}
			}}
			""", comparison);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(AllStringComparisons))]
	public async Task ForFalseInstanceEqualsCheck_WithStringComparison_DoesNotTrigger(StringComparison comparison)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.False("abc".Equals("a", System.StringComparison.{0}));
			    }}
			}}
			""", comparison);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async Task ForStaticEqualsCheck_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.{0}(System.String.Equals("abc", "a"))|}};
			    }}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(SupportedStringComparisons))]
	public async Task ForTrueStaticEqualsCheck_WithSupportedStringComparison_Triggers(StringComparison comparison)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.True(System.String.Equals("abc", "a", System.StringComparison.{0}))|}};
			    }}
			}}
			""", comparison);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("Assert.True()", Constants.Asserts.Equal);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(UnsupportedStringComparisons))]
	public async Task ForTrueStaticEqualsCheck_WithUnsupportedStringComparison_DoesNotTrigger(StringComparison comparison)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.True(System.String.Equals("abc", "a", System.StringComparison.{0}));
			    }}
			}}
			""", comparison);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(AllStringComparisons))]
	public async Task ForFalseStaticEqualsCheck_WithStringComparison_DoesNotTrigger(StringComparison comparison)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.False(System.String.Equals("abc", "a", System.StringComparison.{0}));
			    }}
			}}
			""", comparison);

		await Verify.VerifyAnalyzer(source);
	}
}
