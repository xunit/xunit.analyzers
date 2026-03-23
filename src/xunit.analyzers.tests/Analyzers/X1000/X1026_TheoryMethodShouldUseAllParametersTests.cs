using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldUseAllParameters>;

public class X1026_TheoryMethodShouldUseAllParametersTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {
				[Theory]
				void ParameterRead_DoesNotTrigger(int used) {
					Console.WriteLine(used);
				}

				[Theory]
				void ParameterCapturedAsOutParameterInMockSetup_DoesNotTrigger(string used, int usedOut) {
					// mimicking mock setup use case
					// var mock = new Mock<IHaveOutParameter>();
					// mock.Setup(m => m.SomeMethod(out used));
					Action setup = () => int.TryParse(used, out usedOut);
				}

				[Theory]
				void ExpressionBodiedMethod_DoesNotTrigger(int used) => Assert.Equal(used, 2 + 2);

				[Theory]
				void ParameterNotReferenced_Triggers(int {|#0:unused|}) { }

				[Theory]
				void ParameterUnread_Triggers(int {|#1:unused|}) {
					unused = 3;
					int.TryParse("123", out unused);
				}

				[Theory]
				void MultipleUnreadParameters_Triggers(int {|#2:foo|}, int {|#3:bar|}, int {|#4:baz|}) { }

				[Theory]
				void SomeUnreadParameters_Triggers(int {|#5:foo|}, int bar, int {|#6:baz|}) {
					Console.WriteLine(bar);
					baz = 3;
				}

				[Theory]
				void ExpressionBodiedMethod_Triggers(int {|#7:unused|}) => Assert.Equal(5, 2 + 2);

				// Only a single warning, for _a; everything else is either used or matches the discard pattern
				[Theory]
				void DiscardNamedParameter_DoesNotTrigger(int used, string _, object _1, DateTime _42, double {|#8:_a|})
				{
					Assert.Equal(42, used);
				}

				[Theory]
				extern void MethodWithoutBody_DoesNotTriggerOrCrash(int foo);
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("ParameterNotReferenced_Triggers", "TestClass", "unused"),
			Verify.Diagnostic().WithLocation(1).WithArguments("ParameterUnread_Triggers", "TestClass", "unused"),
			Verify.Diagnostic().WithLocation(2).WithArguments("MultipleUnreadParameters_Triggers", "TestClass", "foo"),
			Verify.Diagnostic().WithLocation(3).WithArguments("MultipleUnreadParameters_Triggers", "TestClass", "bar"),
			Verify.Diagnostic().WithLocation(4).WithArguments("MultipleUnreadParameters_Triggers", "TestClass", "baz"),
			Verify.Diagnostic().WithLocation(5).WithArguments("SomeUnreadParameters_Triggers", "TestClass", "foo"),
			Verify.Diagnostic().WithLocation(6).WithArguments("SomeUnreadParameters_Triggers", "TestClass", "baz"),
			Verify.Diagnostic().WithLocation(7).WithArguments("ExpressionBodiedMethod_Triggers", "TestClass", "unused"),
			Verify.Diagnostic().WithLocation(8).WithArguments("DiscardNamedParameter_DoesNotTrigger", "TestClass", "_a"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {
				[CulturedTheory(new[] { "en-US" })]
				void ParameterRead_DoesNotTrigger(int used) {
					Console.WriteLine(used);
				}

				[CulturedTheory(new[] { "en-US" })]
				void ParameterCapturedAsOutParameterInMockSetup_DoesNotTrigger(string used, int usedOut) {
					// mimicking mock setup use case
					// var mock = new Mock<IHaveOutParameter>();
					// mock.Setup(m => m.SomeMethod(out used));
					Action setup = () => int.TryParse(used, out usedOut);
				}

				[CulturedTheory(new[] { "en-US" })]
				void ExpressionBodiedMethod_DoesNotTrigger(int used) => Assert.Equal(used, 2 + 2);

				[CulturedTheory(new[] { "en-US" })]
				void ParameterNotReferenced_Triggers(int {|#0:unused|}) { }

				[CulturedTheory(new[] { "en-US" })]
				void ParameterUnread_Triggers(int {|#1:unused|}) {
					unused = 3;
					int.TryParse("123", out unused);
				}

				[CulturedTheory(new[] { "en-US" })]
				void MultipleUnreadParameters_Triggers(int {|#2:foo|}, int {|#3:bar|}, int {|#4:baz|}) { }

				[CulturedTheory(new[] { "en-US" })]
				void SomeUnreadParameters_Triggers(int {|#5:foo|}, int bar, int {|#6:baz|}) {
					Console.WriteLine(bar);
					baz = 3;
				}

				[CulturedTheory(new[] { "en-US" })]
				void ExpressionBodiedMethod_Triggers(int {|#7:unused|}) => Assert.Equal(5, 2 + 2);

				// Only a single warning, for _a; everything else is either used or matches the discard pattern
				[CulturedTheory(new[] { "en-US" })]
				void DiscardNamedParameter_DoesNotTrigger(int used, string _, object _1, DateTime _42, double {|#8:_a|})
				{
					Assert.Equal(42, used);
				}

				[CulturedTheory(new[] { "en-US" })]
				extern void MethodWithoutBody_DoesNotTriggerOrCrash(int foo);
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("ParameterNotReferenced_Triggers", "TestClass", "unused"),
			Verify.Diagnostic().WithLocation(1).WithArguments("ParameterUnread_Triggers", "TestClass", "unused"),
			Verify.Diagnostic().WithLocation(2).WithArguments("MultipleUnreadParameters_Triggers", "TestClass", "foo"),
			Verify.Diagnostic().WithLocation(3).WithArguments("MultipleUnreadParameters_Triggers", "TestClass", "bar"),
			Verify.Diagnostic().WithLocation(4).WithArguments("MultipleUnreadParameters_Triggers", "TestClass", "baz"),
			Verify.Diagnostic().WithLocation(5).WithArguments("SomeUnreadParameters_Triggers", "TestClass", "foo"),
			Verify.Diagnostic().WithLocation(6).WithArguments("SomeUnreadParameters_Triggers", "TestClass", "baz"),
			Verify.Diagnostic().WithLocation(7).WithArguments("ExpressionBodiedMethod_Triggers", "TestClass", "unused"),
			Verify.Diagnostic().WithLocation(8).WithArguments("DiscardNamedParameter_DoesNotTrigger", "TestClass", "_a"),
		};

		await Verify.VerifyAnalyzerV3(source, expected);
	}
}
