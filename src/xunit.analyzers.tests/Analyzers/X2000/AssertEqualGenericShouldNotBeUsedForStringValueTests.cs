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
	public async Task StringEqualityCheckWithoutGenericType_DoesNotTrigger(
		string expected,
		string value)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.Equal({0}, {1});
			    }}
			}}
			""", expected, value);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Data))]
	public async Task StringEqualityCheckWithGenericType_Triggers(
		string expected,
		string value)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        [|Xunit.Assert.Equal<string>({0}, {1})|];
			    }}
			}}
			""", expected, value);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Data))]
	public async Task StrictStringEqualityCheck_Triggers(
		string expected,
		string value)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        [|Xunit.Assert.StrictEqual({0}, {1})|];
			    }}
			}}
			""", expected, value);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Data))]
	public async Task StrictStringEqualityCheckWithGenericType_Triggers(
		string expected,
		string value)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        [|Xunit.Assert.StrictEqual<string>({0}, {1})|];
			    }}
			}}
			""", expected, value);

		await Verify.VerifyAnalyzer(source);
	}
}
