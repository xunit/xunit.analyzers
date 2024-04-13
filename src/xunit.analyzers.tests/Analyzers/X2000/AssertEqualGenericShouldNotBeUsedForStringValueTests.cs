using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualGenericShouldNotBeUsedForStringValue>;

public class AssertEqualGenericShouldNotBeUsedForStringValueTests
{
	public static TheoryData<string, string> Data = new()
	{
		{ "true.ToString()", "\"True\"" },
		{ "1.ToString()", "\"1\"" },
		{ "\"\"", "null" },
		{ "null", "\"\"" },
		{ "\"\"", "\"\"" },
		{ "\"abc\"", "\"abc\"" },
		{ "\"TestMethod\"", "nameof(TestMethod)" },
	};

	[Theory]
	[MemberData(nameof(Data))]
	public async Task DoesNotFindWarningForStringEqualityCheckWithoutGenericType(
		string expected,
		string value)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.Equal({expected}, {value});
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Data))]
	public async Task FindsWarningForStringEqualityCheckWithGenericType(
		string expected,
		string value)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        [|Xunit.Assert.Equal<string>({expected}, {value})|];
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Data))]
	public async Task FindsWarningForStrictStringEqualityCheck(
		string expected,
		string value)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        [|Xunit.Assert.StrictEqual({expected}, {value})|];
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Data))]
	public async Task FindsWarningForStrictStringEqualityCheckWithGenericType(
		string expected,
		string value)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        [|Xunit.Assert.StrictEqual<string>({expected}, {value})|];
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}
}
