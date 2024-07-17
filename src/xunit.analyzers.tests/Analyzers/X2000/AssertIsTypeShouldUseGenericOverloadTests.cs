using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldUseGenericOverloadType>;

public class AssertIsTypeShouldUseGenericOverloadTests
{
	public static TheoryData<string> Methods =
	[
		"IsType",
		"IsNotType",
		"IsAssignableFrom",
	];

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForNonGenericCall_Triggers(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Xunit.Assert.{0}(typeof(int), 1)|}};
			    }}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("int");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task ForGenericCall_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
			    void TestMethod() {{
			        Xunit.Assert.{0}<int>(1);
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}
}
