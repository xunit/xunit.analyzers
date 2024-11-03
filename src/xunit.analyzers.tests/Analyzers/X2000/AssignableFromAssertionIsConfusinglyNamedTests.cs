using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssignableFromAssertionIsConfusinglyNamed>;
using Verify_v2_Pre2_9_3 = CSharpVerifier<AssignableFromAssertionIsConfusinglyNamedTests.Analyzer_v2_Pre2_9_3>;
using Verify_v3_Pre0_6_0 = CSharpVerifier<AssignableFromAssertionIsConfusinglyNamedTests.Analyzer_v3_Pre0_6_0>;

public class AssignableFromAssertionIsConfusinglyNamedTests
{
	public static TheoryData<string, string> Methods = new()
	{
		{ "IsAssignableFrom", "IsType" },
		{ "IsNotAssignableFrom", "IsNotType"},
	};

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task WhenReplacementAvailable_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Assert.{0}<object>(new object())|}};
			        {{|#1:Assert.{0}(typeof(object), new object())|}};
			    }}
			}}
			""", method);
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments(method, replacement),
			Verify.Diagnostic().WithLocation(1).WithArguments(method, replacement),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task WhenReplacementNotAvailable_DoesNotTriggers(
		string method,
		string _)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {{
			    void TestMethod() {{
			        Assert.{0}<object>(new object());
			        Assert.{0}(typeof(object), new object());
			    }}
			}}
			""", method);

		await Verify_v2_Pre2_9_3.VerifyAnalyzer(source);
		await Verify_v3_Pre0_6_0.VerifyAnalyzer(source);
	}

	internal class Analyzer_v2_Pre2_9_3 : AssignableFromAssertionIsConfusinglyNamed
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 9, 2));
	}

	internal class Analyzer_v3_Pre0_6_0 : AssignableFromAssertionIsConfusinglyNamed
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3(compilation, new Version(0, 5, 999));
	}
}
