using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSameShouldNotBeCalledOnValueTypes>;

public class AssertSameShouldNotBeCalledOnValueTypesTests
{
	public static TheoryData<string, string> Methods_WithReplacement = new()
	{
		{ Constants.Asserts.Same, Constants.Asserts.Equal },
		{ Constants.Asserts.NotSame, Constants.Asserts.NotEqual },
	};

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async Task TwoValueParameters_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					int a = 0;
					{{|#0:Xunit.Assert.{0}(0, a)|}};
				}}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", "int", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async Task FirstValueParameters_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					object a = 0;
					{{|#0:Xunit.Assert.{0}(0, a)|}};
				}}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", "int", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async Task SecondValueParameters_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					object a = 0;
					{{|#0:Xunit.Assert.{0}(a, 0)|}};
				}}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", "int", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	// https://github.com/xunit/xunit/issues/2395
	public async Task UserDefinedImplicitConversion_DoesNotTrigger(
		string method,
		string _)
	{
		var source = string.Format(/* lang=c#-test */ """
			public class TestClass {{
				public void TestMethod() {{
					var o = new object();

					Xunit.Assert.{0}((MyBuggyInt)42, o);
					Xunit.Assert.{0}((MyBuggyInt)(int?)42, o);
					Xunit.Assert.{0}((MyBuggyIntBase)42, o);
					Xunit.Assert.{0}((MyBuggyIntBase)(int?)42, o);

					Xunit.Assert.{0}(o, (MyBuggyInt)42);
					Xunit.Assert.{0}(o, (MyBuggyInt)(int?)42);
					Xunit.Assert.{0}(o, (MyBuggyIntBase)42);
					Xunit.Assert.{0}(o, (MyBuggyIntBase)(int?)42);
				}}
			}}

			public abstract class MyBuggyIntBase {{
				public static implicit operator MyBuggyIntBase(int i) => new MyBuggyInt();
			}}

			public class MyBuggyInt : MyBuggyIntBase {{
				public MyBuggyInt() {{ }}
			}}
			""", method);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async Task FirstValueParametersIfSecondIsNull_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					{{|#0:Xunit.Assert.{0}(0, null)|}};
				}}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", "int", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(Methods_WithReplacement))]
	public async Task SecondValueParametersIfFirstIsNull_Triggers(
		string method,
		string replacement)
	{
		var source = string.Format(/* lang=c#-test */ """
			class TestClass {{
				void TestMethod() {{
					{{|#0:Xunit.Assert.{0}(null, 0)|}};
				}}
			}}
			""", method);
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments($"Assert.{method}()", "int", replacement);

		await Verify.VerifyAnalyzer(source, expected);
	}
}
