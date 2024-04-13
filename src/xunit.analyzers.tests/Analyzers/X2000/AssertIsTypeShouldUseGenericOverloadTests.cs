using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldUseGenericOverloadType>;

public class AssertIsTypeShouldUseGenericOverloadTests
{
	public static TheoryData<string> Methods = new()
	{
		"IsType",
		"IsNotType",
		"IsAssignableFrom",
	};

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task FindsWarning_ForNonGenericCall(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}(typeof(int), 1);
    }}
}}";
		var expected =
			Verify
				.Diagnostic()
				.WithSpan(4, 9, 4, 38 + method.Length)
				.WithSeverity(DiagnosticSeverity.Warning)
				.WithArguments("int");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task DoesNotFindWarning_ForGenericCall(string method)
	{
		var source = $@"
class TestClass {{
    void TestMethod() {{
        Xunit.Assert.{method}<int>(1);
    }}
}}";

		await Verify.VerifyAnalyzer(source);
	}
}
