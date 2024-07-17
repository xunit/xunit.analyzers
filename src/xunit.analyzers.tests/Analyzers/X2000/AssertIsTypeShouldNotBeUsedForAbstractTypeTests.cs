using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldNotBeUsedForAbstractType>;

public class AssertIsTypeShouldNotBeUsedForAbstractTypeTests
{
	public static TheoryData<string, string> Methods = new()
	{
		{ "IsType", "IsAssignableFrom" },
		{ "IsNotType", "IsNotAssignableFrom" },
	};

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task Interface_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Assert.{0}<IDisposable>(new object())|}};
			    }}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("interface", "System.IDisposable", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task AbstractClass_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.IO;
			using Xunit;

			class TestClass {{
			    void TestMethod() {{
			        {{|#0:Assert.{0}<Stream>(new object())|}};
			    }}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("abstract class", "System.IO.Stream", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task UsingStatic_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System;
			using static Xunit.Assert;

			class TestClass {{
			    void TestMethod() {{
			        {{|#0:{0}<IDisposable>(new object())|}};
			    }}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("interface", "System.IDisposable", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task NonAbstractClass_DoesNotTrigger(
		string method,
		string _)
	{
		var source = string.Format(/* lang=c#-test */ """
			using Xunit;

			class TestClass {{
			    void TestMethod() {{
			        Assert.{0}<string>(new object());
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}
}
