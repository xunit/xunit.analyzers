using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertIsTypeShouldNotBeUsedForAbstractType>;
using Verify_v2_Pre2_9_3 = CSharpVerifier<AssertIsTypeShouldNotBeUsedForAbstractTypeTests.Analyzer_v2_Pre2_9_3>;
using Verify_v3_Pre0_6_0 = CSharpVerifier<AssertIsTypeShouldNotBeUsedForAbstractTypeTests.Analyzer_v3_Pre0_6_0>;

public class AssertIsTypeShouldNotBeUsedForAbstractTypeTests
{
	public static TheoryData<string> Methods = ["IsType", "IsNotType"];
	public static TheoryData<string, string, bool> MethodsWithReplacements = new()
	{
		{ "IsType", "Assert.IsAssignableFrom", false },
		{ "IsType", "exactMatch: false", true },
		{ "IsNotType", "Assert.IsNotAssignableFrom", false },
		{ "IsNotType", "exactMatch: false", true },
	};

	[Theory]
	[MemberData(nameof(MethodsWithReplacements))]
	public async Task Interface_Triggers(
		string method,
		string replacement,
		bool supportsInexactTypeAssertions)
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

		if (supportsInexactTypeAssertions)
			await Verify.VerifyAnalyzer(source, expected);
		else
		{
			await Verify_v2_Pre2_9_3.VerifyAnalyzer(source, expected);
			await Verify_v3_Pre0_6_0.VerifyAnalyzer(source, expected);
		}
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task Interface_WithExactMatchFlag_TriggersForTrue(string method)
	{
		// We can only trigger when we know the literal true is being used; anything else,
		// we let the runtime figure it out.
		var source = string.Format(/* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {{
			    void TestMethod() {{
			        var flag = true;

			        {{|#0:Assert.{0}<IDisposable>(new object(), true)|}};
			        {{|#1:Assert.{0}<IDisposable>(new object(), exactMatch: true)|}};
			        Assert.{0}<IDisposable>(new object(), flag);
			    }}
			}}
			""", method);
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("interface", "System.IDisposable", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(1).WithArguments("interface", "System.IDisposable", "exactMatch: false"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(MethodsWithReplacements))]
	public async Task AbstractClass_Triggers(
		string method,
		string replacement,
		bool supportsInexactTypeAssertions)
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

		if (supportsInexactTypeAssertions)
			await Verify.VerifyAnalyzer(source, expected);
		else
		{
			await Verify_v2_Pre2_9_3.VerifyAnalyzer(source, expected);
			await Verify_v3_Pre0_6_0.VerifyAnalyzer(source, expected);
		}
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task AbstractClass_WithExactMatchFlag_TriggersForTrue(string method)
	{
		// We can only trigger when we know the literal true is being used; anything else,
		// we let the runtime figure it out.
		var source = string.Format(/* lang=c#-test */ """
			using System.IO;
			using Xunit;

			class TestClass {{
			    void TestMethod() {{
			        var flag = true;

			        {{|#0:Assert.{0}<Stream>(new object(), true)|}};
			        {{|#1:Assert.{0}<Stream>(new object(), exactMatch: true)|}};
			        Assert.{0}<Stream>(new object(), flag);
			    }}
			}}
			""", method);
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("abstract class", "System.IO.Stream", "exactMatch: false"),
			Verify.Diagnostic().WithLocation(1).WithArguments("abstract class", "System.IO.Stream", "exactMatch: false"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(MethodsWithReplacements))]
	public async Task UsingStatic_Triggers(
		string method,
		string replacement,
		bool supportsInexactTypeAssertions)
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

		if (supportsInexactTypeAssertions)
			await Verify.VerifyAnalyzer(source, expected);
		else
		{
			await Verify_v2_Pre2_9_3.VerifyAnalyzer(source, expected);
			await Verify_v3_Pre0_6_0.VerifyAnalyzer(source, expected);
		}
	}

	[Theory]
	[MemberData(nameof(Methods))]
	public async Task NonAbstractClass_DoesNotTrigger(string method)
	{
		var source = string.Format(/* lang=c#-test */ """
			using Xunit;

			class TestClass {{
			    void TestMethod() {{
			        var flag = true;

			        Assert.{0}<string>(new object());
			        Assert.{0}<string>(new object(), flag);
			        Assert.{0}<string>(new object(), exactMatch: flag);
			        Assert.{0}<string>(new object(), true);
			        Assert.{0}<string>(new object(), exactMatch: true);
			        Assert.{0}<string>(new object(), false);
			        Assert.{0}<string>(new object(), exactMatch: false);
			    }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	internal class Analyzer_v2_Pre2_9_3 : AssertIsTypeShouldNotBeUsedForAbstractType
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 9, 2));
	}

	internal class Analyzer_v3_Pre0_6_0 : AssertIsTypeShouldNotBeUsedForAbstractType
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3(compilation, new Version(0, 5, 999));
	}
}
