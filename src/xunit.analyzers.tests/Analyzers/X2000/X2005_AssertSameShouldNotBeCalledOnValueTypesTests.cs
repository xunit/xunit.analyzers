using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSameShouldNotBeCalledOnValueTypes>;

public class X2005_AssertSameShouldNotBeCalledOnValueTypesTests
{
	public static TheoryData<string, string> Methods_WithReplacement = new()
	{
		{ Constants.Asserts.Same, Constants.Asserts.Equal },
		{ Constants.Asserts.NotSame, Constants.Asserts.NotEqual },
	};

	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				void TwoValueParameters_Triggers() {
					int a = 0;

					{|#0:Assert.Same(0, a)|};
					{|#1:Assert.NotSame(0, a)|};
				}

				void FirstValueParameters_Triggers() {
					object a = 0;

					{|#10:Assert.Same(0, a)|};
					{|#11:Assert.NotSame(0, a)|};
				}

				void SecondValueParameters_Triggers() {
					object a = 0;

					{|#20:Assert.Same(a, 0)|};
					{|#21:Assert.NotSame(a, 0)|};
				}

				void FirstValueParametersIfSecondIsNull_Triggers() {
					{|#30:Assert.Same(0, null)|};
					{|#31:Assert.NotSame(0, null)|};
				}

				void SecondValueParametersIfFirstIsNull_Triggers() {
					{|#40:Assert.Same(null, 0)|};
					{|#41:Assert.NotSame(null, 0)|};
				}

				// https://github.com/xunit/xunit/issues/2395
				void UserDefinedImplicitConversion_DoesNotTrigger() {
					var o = new object();

					Assert.Same((MyBuggyInt)42, o);
					Assert.NotSame((MyBuggyInt)42, o);
					Assert.Same((MyBuggyInt)(int?)42, o);
					Assert.NotSame((MyBuggyInt)(int?)42, o);
					Assert.Same((MyBuggyIntBase)42, o);
					Assert.NotSame((MyBuggyIntBase)42, o);
					Assert.Same((MyBuggyIntBase)(int?)42, o);
					Assert.NotSame((MyBuggyIntBase)(int?)42, o);

					Assert.Same(o, (MyBuggyInt)42);
					Assert.NotSame(o, (MyBuggyInt)42);
					Assert.Same(o, (MyBuggyInt)(int?)42);
					Assert.NotSame(o, (MyBuggyInt)(int?)42);
					Assert.Same(o, (MyBuggyIntBase)42);
					Assert.NotSame(o, (MyBuggyIntBase)42);
					Assert.Same(o, (MyBuggyIntBase)(int?)42);
					Assert.NotSame(o, (MyBuggyIntBase)(int?)42);
				}
			}

			abstract class MyBuggyIntBase {
				public static implicit operator MyBuggyIntBase(int i) => new MyBuggyInt();
			}

			class MyBuggyInt : MyBuggyIntBase {
				public MyBuggyInt() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.Same()", "int", "Equal"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.NotSame()", "int", "NotEqual"),

			Verify.Diagnostic().WithLocation(10).WithArguments("Assert.Same()", "int", "Equal"),
			Verify.Diagnostic().WithLocation(11).WithArguments("Assert.NotSame()", "int", "NotEqual"),

			Verify.Diagnostic().WithLocation(20).WithArguments("Assert.Same()", "int", "Equal"),
			Verify.Diagnostic().WithLocation(21).WithArguments("Assert.NotSame()", "int", "NotEqual"),

			Verify.Diagnostic().WithLocation(30).WithArguments("Assert.Same()", "int", "Equal"),
			Verify.Diagnostic().WithLocation(31).WithArguments("Assert.NotSame()", "int", "NotEqual"),

			Verify.Diagnostic().WithLocation(40).WithArguments("Assert.Same()", "int", "Equal"),
			Verify.Diagnostic().WithLocation(41).WithArguments("Assert.NotSame()", "int", "NotEqual"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
