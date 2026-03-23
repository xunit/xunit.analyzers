using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualLiteralValueShouldBeFirst>;

public class X2000_AssertEqualLiteralValueShouldBeFirstTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {
				int IntValue = 42;
				string StringValue = "Hello, world";
				Type TypeValue = typeof(TestClass);
				AttributeTargets EnumValue = AttributeTargets.Constructor;

				void WhenConstantOrLiteralUsedForBothArguments_DoesNotTrigger() {
					Assert.Equal("TestClass", nameof(TestClass));
					Assert.Equal(nameof(TestClass), "TestClass");
				}

				void ExpectedConstantOrLiteralValueAsFirstArgument_DoesNotTrigger() {
					Assert.Equal(0, IntValue);
					Assert.Equal(0.0, IntValue);
					Assert.Equal(sizeof(int), IntValue);
					Assert.Equal(default(int), IntValue);

					Assert.Equal(null, StringValue);
					Assert.Equal("Hello world", StringValue);
					Assert.Equal(nameof(TestClass), StringValue);
					Assert.Equal(new string(' ', 4), StringValue);

					Assert.Equal(typeof(string), TypeValue);

					Assert.Equal(AttributeTargets.Method, EnumValue);
				}

				void ReversedExplicitArguments_DoesNotTrigger() {
					Assert.Equal(actual: IntValue, expected: 0);
					Assert.Equal(actual: IntValue, expected: 0.0);
					Assert.Equal(actual: IntValue, expected: sizeof(int));
					Assert.Equal(actual: IntValue, expected: default(int));
					Assert.Equal(actual: StringValue, expected: null);
					Assert.Equal(actual: StringValue, expected: "Hello world");
					Assert.Equal(actual: StringValue, expected: nameof(TestClass));
					Assert.Equal(actual: StringValue, expected: new string(' ', 4));
					Assert.Equal(actual: TypeValue, expected: typeof(string));
					Assert.Equal(actual: EnumValue, expected: AttributeTargets.Method);
				}

				void ExpectedConstantOrLiteralValueAsSecondArgument_Triggers() {
					{|#0:Assert.Equal(IntValue, 0)|};
					{|#1:Assert.Equal(IntValue, 0.0)|};
					{|#2:Assert.Equal(IntValue, sizeof(int))|};
					{|#3:Assert.Equal(IntValue, default(int))|};

					{|#4:Assert.Equal(StringValue, null)|};
					{|#5:Assert.Equal(StringValue, "Hello world")|};
					{|#6:Assert.Equal(StringValue, nameof(TestClass))|};
					{|#7:Assert.Equal(StringValue, new string(' ', 4))|};

					{|#8:Assert.Equal(TypeValue, typeof(string))|};

					{|#9:Assert.Equal(EnumValue, AttributeTargets.Method)|};
				}

				void InvalidArguments_DoesNotTrigger() {
					Assert.Equal({|CS1739:exp|}: IntValue, actual: 0);
					Assert.Equal(expected: IntValue, {|CS1739:act|}: 0);
					Assert.{|CS1501:Equal|}(expected: IntValue, expected: 0);
					Assert.{|CS1501:Equal|}(actual: IntValue, actual: 0);
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("0", "Assert.Equal(expected, actual)", "ExpectedConstantOrLiteralValueAsSecondArgument_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(1).WithArguments("0.0", "Assert.Equal(expected, actual)", "ExpectedConstantOrLiteralValueAsSecondArgument_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(2).WithArguments("sizeof(int)", "Assert.Equal(expected, actual)", "ExpectedConstantOrLiteralValueAsSecondArgument_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(3).WithArguments("default(int)", "Assert.Equal(expected, actual)", "ExpectedConstantOrLiteralValueAsSecondArgument_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(4).WithArguments("null", "Assert.Equal(expected, actual)", "ExpectedConstantOrLiteralValueAsSecondArgument_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(5).WithArguments(@"""Hello world""", "Assert.Equal(expected, actual)", "ExpectedConstantOrLiteralValueAsSecondArgument_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(6).WithArguments("nameof(TestClass)", "Assert.Equal(expected, actual)", "ExpectedConstantOrLiteralValueAsSecondArgument_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(7).WithArguments("new string(' ', 4)", "Assert.Equal(expected, actual)", "ExpectedConstantOrLiteralValueAsSecondArgument_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(8).WithArguments("typeof(string)", "Assert.Equal(expected, actual)", "ExpectedConstantOrLiteralValueAsSecondArgument_Triggers", "TestClass"),
			Verify.Diagnostic().WithLocation(9).WithArguments("AttributeTargets.Method", "Assert.Equal(expected, actual)", "ExpectedConstantOrLiteralValueAsSecondArgument_Triggers", "TestClass"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
