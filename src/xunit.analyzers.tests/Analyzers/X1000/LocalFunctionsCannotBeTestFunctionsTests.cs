using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.LocalFunctionsCannotBeTestFunctions>;

public class LocalFunctionsCannotBeTestFunctionsTests
{
	[Fact]
	public async Task NoTestAttribute_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			public class TestClass {
				public void Method() {
					void LocalFunction() { }
				}
			}
			""";

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source);
	}

	[Theory]
	[InlineData("Fact")]
	[InlineData("Theory")]
	[InlineData("InlineData(42)")]
	[InlineData("MemberData(nameof(MyData))")]
	[InlineData("ClassData(typeof(TestClass))")]
	public async Task TestAttribute_Triggers(string attribute)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			public class TestClass {{
				public void Method() {{
					[{{|#0:{0}|}}]
					void LocalFunction() {{ }}
				}}

				public static IEnumerable<object[]> MyData;
			}}
			""", attribute);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"[{attribute}]");

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp9, source, expected);
	}
}
