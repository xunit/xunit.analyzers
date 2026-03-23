using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForNullCheck>;

public class X2003_AssertEqualShouldNotBeUsedForNullCheckTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using Xunit;

			class TestClass {
				readonly string StringValue = null;
				readonly object ObjectValue = null;
				readonly TestClass ClassValue = null;

				void FirstNullLiteral_StringOverload_Triggers() {
					{|#0:Assert.Equal(null, StringValue)|};
					{|#1:Assert.NotEqual(null, StringValue)|};
				}

				void FirstNullLiteral_StringOverload_WithCustomComparer_Triggers() {
					{|#10:Assert.Equal(null, StringValue, StringComparer.Ordinal)|};
					{|#11:Assert.NotEqual(null, StringValue, StringComparer.Ordinal)|};
				}

				void FirstNullLiteral_ObjectOverload_Triggers() {
					{|#20:Assert.Equal(null, ObjectValue)|};
					{|#21:Assert.StrictEqual(null, ObjectValue)|};
					{|#22:Assert.Same(null, ObjectValue)|};
					{|#23:Assert.NotEqual(null, ObjectValue)|};
					{|#24:Assert.NotStrictEqual(null, ObjectValue)|};
					{|#25:Assert.NotSame(null, ObjectValue)|};
				}

				void FirstNullLiteral_ObjectOverload_WithCustomComparer_Triggers() {
					{|#30:Assert.Equal(null, ObjectValue, EqualityComparer<object>.Default)|};
					{|#31:Assert.NotEqual(null, ObjectValue, EqualityComparer<object>.Default)|};
				}

				void FirstNullLiteral_GenericOverload_Triggers() {
					{|#40:Assert.Equal<TestClass>(null, ClassValue)|};
					{|#41:Assert.NotEqual<TestClass>(null, ClassValue)|};
				}

				void FirstNullLiteral_GenericOverload_WithCustomComparer_Triggers() {
					{|#50:Assert.Equal<TestClass>(null, ClassValue, EqualityComparer<TestClass>.Default)|};
					{|#51:Assert.NotEqual<TestClass>(null, ClassValue, EqualityComparer<TestClass>.Default)|};
				}

				void SecondNullLiteral_DoesNotTrigger() {
					Assert.Equal(StringValue, null);
					Assert.StrictEqual(StringValue, null);
					Assert.Same(StringValue, null);
					Assert.NotEqual(StringValue, null);
					Assert.NotStrictEqual(StringValue, null);
					Assert.NotSame(StringValue, null);
				}

				void NonNull_DoesNotTrigger() {
					Assert.Equal("Hello world", StringValue);
					Assert.StrictEqual("Hello world", StringValue);
					Assert.Same("Hello world", StringValue);
					Assert.NotEqual("Hello world", StringValue);
					Assert.NotStrictEqual("Hello world", StringValue);
					Assert.NotSame("Hello world", StringValue);
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.Equal()", "Null"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.NotEqual()", "NotNull"),

			Verify.Diagnostic().WithLocation(10).WithArguments("Assert.Equal()", "Null"),
			Verify.Diagnostic().WithLocation(11).WithArguments("Assert.NotEqual()", "NotNull"),

			Verify.Diagnostic().WithLocation(20).WithArguments("Assert.Equal()", "Null"),
			Verify.Diagnostic().WithLocation(21).WithArguments("Assert.StrictEqual()", "Null"),
			Verify.Diagnostic().WithLocation(22).WithArguments("Assert.Same()", "Null"),
			Verify.Diagnostic().WithLocation(23).WithArguments("Assert.NotEqual()", "NotNull"),
			Verify.Diagnostic().WithLocation(24).WithArguments("Assert.NotStrictEqual()", "NotNull"),
			Verify.Diagnostic().WithLocation(25).WithArguments("Assert.NotSame()", "NotNull"),

			Verify.Diagnostic().WithLocation(30).WithArguments("Assert.Equal()", "Null"),
			Verify.Diagnostic().WithLocation(31).WithArguments("Assert.NotEqual()", "NotNull"),

			Verify.Diagnostic().WithLocation(40).WithArguments("Assert.Equal()", "Null"),
			Verify.Diagnostic().WithLocation(41).WithArguments("Assert.NotEqual()", "NotNull"),

			Verify.Diagnostic().WithLocation(50).WithArguments("Assert.Equal()", "Null"),
			Verify.Diagnostic().WithLocation(51).WithArguments("Assert.NotEqual()", "NotNull"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
