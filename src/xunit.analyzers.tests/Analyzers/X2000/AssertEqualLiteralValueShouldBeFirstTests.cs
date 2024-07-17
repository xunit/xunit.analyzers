using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualLiteralValueShouldBeFirst>;

public class AssertEqualLiteralValueShouldBeFirstTests
{
	[Fact]
	public async Task WhenConstantOrLiteralUsedForBothArguments_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    void TestMethod() {
			        Xunit.Assert.Equal("TestMethod", nameof(TestMethod));
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	public static TheoryData<string, string> TypesAndValues = new()
	{
		{ "int", "0" },
		{ "int", "0.0" },
		{ "int", "sizeof(int)" },
		{ "int", "default(int)" },
		{ "string", "null" },
		{ "string", "\"\"" },
		{ "string", "nameof(TestMethod)" },
		{ "System.Type", "typeof(string)" },
		{ "System.AttributeTargets", "System.AttributeTargets.Constructor" },
	};

	[Theory]
	[MemberData(nameof(TypesAndValues))]
	public async Task ExpectedConstantOrLiteralValueAsFirstArgument_DoesNotTrigger(
		string type,
		string value)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        var v = default({0});
			        Xunit.Assert.Equal({1}, v);
			    }}
			}}
			""", type, value);

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task ConstantsUsedInStringConstructorAsFirstArgument_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
			    void TestMethod() {
			        Xunit.Assert.Equal(new string(' ', 4), "    ");
			    }
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(TypesAndValues))]
	public async Task ExpectedConstantOrLiteralValueAsSecondArgument_Triggers(
		string type,
		string value)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        var v = default({0});
			        {{|#0:Xunit.Assert.Equal(v, {1})|}};
			    }}
			}}
			""", type, value);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments(value, "Assert.Equal(expected, actual)", "TestMethod", "TestClass");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task ExpectedConstantOrLiteralValueAsNamedExpectedArgument_DoesNotTrigger(bool useAlternateForm)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        var v = default(int);
			        Xunit.Assert.Equal({0}actual: v, {0}expected: 0);
			    }}
			}}
			""", useAlternateForm ? "@" : "");

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(TypesAndValues))]
	public async Task ExpectedConstantOrLiteralValueAsNamedExpectedArgument_Triggers(
		string type,
		string value)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        var v = default({0});
			        {{|#0:Xunit.Assert.Equal(actual: {1}, expected: v)|}};
			    }}
			}}
			""", type, value);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments(value, "Assert.Equal(expected, actual)", "TestMethod", "TestClass");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData("Equal", "{|CS1739:act|}", "exp")]
	[InlineData("{|CS1501:Equal|}", "expected", "expected")]
	[InlineData("{|CS1501:Equal|}", "actual", "actual")]
	[InlineData("Equal", "{|CS1739:foo|}", "bar")]
	public async Task DoesNotFindWarningWhenArgumentsAreNotNamedCorrectly(
		string methodName,
		string firstArgumentName,
		string secondArgumentName)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        var v = default(int);
			        Xunit.Assert.{0}({1}: 1, {2}: v);
			    }}
			}}
			""", methodName, firstArgumentName, secondArgumentName);

		await Verify.VerifyAnalyzer(source);
	}
}
