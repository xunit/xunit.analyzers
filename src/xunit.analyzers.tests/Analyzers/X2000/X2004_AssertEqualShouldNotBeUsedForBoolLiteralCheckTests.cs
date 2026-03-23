using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForBoolLiteralCheck>;

public class X2004_AssertEqualShouldNotBeUsedForBoolLiteralCheckTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			class TestClass {
			    readonly bool TrueValue = true;
				readonly object ObjectValue = false;

				void BooleanToBoolean_Triggers() {
					{|#0:Assert.Equal(true, TrueValue)|};
					{|#1:Assert.Equal(false, TrueValue)|};
					{|#2:Assert.NotEqual(true, TrueValue)|};
					{|#3:Assert.NotEqual(false, TrueValue)|};
				}

				void BooleanToBoolean_WithComparer_Triggers() {
					{|#10:Assert.Equal(true, TrueValue, EqualityComparer<bool>.Default)|};
					{|#11:Assert.Equal(false, TrueValue, EqualityComparer<bool>.Default)|};
					{|#12:Assert.NotEqual(true, TrueValue, EqualityComparer<bool>.Default)|};
					{|#13:Assert.NotEqual(false, TrueValue, EqualityComparer<bool>.Default)|};
				}

				void BooleanToNonBoolean_DoesNotTrigger() {
					Assert.Equal(true, ObjectValue);
					Assert.Equal(false, ObjectValue);
					Assert.NotEqual(true, ObjectValue);
					Assert.NotEqual(false, ObjectValue);
				}

				void NonBooleanToBoolean_DoesNotTrigger() {
					Assert.Equal(ObjectValue, true);
					Assert.Equal(ObjectValue, false);
					Assert.NotEqual(ObjectValue, true);
					Assert.NotEqual(ObjectValue, false);
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.Equal()", "True"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.Equal()", "False"),
			Verify.Diagnostic().WithLocation(2).WithArguments("Assert.NotEqual()", "False"),
			Verify.Diagnostic().WithLocation(3).WithArguments("Assert.NotEqual()", "True"),

			Verify.Diagnostic().WithLocation(10).WithArguments("Assert.Equal()", "True"),
			Verify.Diagnostic().WithLocation(11).WithArguments("Assert.Equal()", "False"),
			Verify.Diagnostic().WithLocation(12).WithArguments("Assert.NotEqual()", "False"),
			Verify.Diagnostic().WithLocation(13).WithArguments("Assert.NotEqual()", "True"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask NonAOT()
	{
		// StrictEqual and NotStrictEqual are generic in reflection mode, so we can do the
		// boolean-to-boolean verification. In AOT mode, it's object, so we fall back to the
		// boolean-to-object behavior.
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;
			using Xunit;

			class TestClass {
			    readonly bool TrueValue = true;
				readonly object ObjectValue = false;

				void BooleanToBoolean_Triggers() {
					{|#0:Assert.StrictEqual(true, TrueValue)|};
					{|#1:Assert.StrictEqual(false, TrueValue)|};
					{|#2:Assert.NotStrictEqual(true, TrueValue)|};
					{|#3:Assert.NotStrictEqual(false, TrueValue)|};
				}

				void BooleanToNonBoolean_DoesNotTrigger() {
					Assert.StrictEqual(true, ObjectValue);
					Assert.StrictEqual(false, ObjectValue);
					Assert.NotStrictEqual(true, ObjectValue);
					Assert.NotStrictEqual(false, ObjectValue);
				}

				void NonBooleanToBoolean_DoesNotTrigger() {
					Assert.StrictEqual(ObjectValue, true);
					Assert.StrictEqual(ObjectValue, false);
					Assert.NotStrictEqual(ObjectValue, true);
					Assert.NotStrictEqual(ObjectValue, false);
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.StrictEqual()", "True"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.StrictEqual()", "False"),
			Verify.Diagnostic().WithLocation(2).WithArguments("Assert.NotStrictEqual()", "False"),
			Verify.Diagnostic().WithLocation(3).WithArguments("Assert.NotStrictEqual()", "True"),
		};

		await Verify.VerifyAnalyzerNonAot(source, expected);
	}
}
