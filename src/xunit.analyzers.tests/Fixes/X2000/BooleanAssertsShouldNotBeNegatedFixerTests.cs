using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeNegated>;

public class BooleanAssertsShouldNotBeNegatedFixerTests
{
	[Fact]
	public async Task AcceptanceTest()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					bool condition = true;

					// Not negated
					Assert.True(false);
					Assert.False(false);
					Assert.True(condition);
					Assert.False(condition);

					// Negated
					[|Assert.True(!false)|];
					[|Assert.False(!false)|];
					[|Assert.True(!condition)|];
					[|Assert.False(!condition)|];

					// Not negated, with message
					Assert.True(false, "test message");
					Assert.False(false, "test message");
					Assert.True(condition, "test message");
					Assert.False(condition, "test message");

					// Negated, with message
					[|Assert.True(!false, "test message")|];
					[|Assert.False(!false, "test message")|];
					[|Assert.True(!condition, "test message")|];
					[|Assert.False(!condition, "test message")|];

					// Not negated, with named parameter message
					Assert.True(false, userMessage: "test message");
					Assert.False(false, userMessage: "test message");
					Assert.True(condition, userMessage: "test message");
					Assert.False(condition, userMessage: "test message");

					// Negated, with named parameter message
					[|Assert.True(!false, userMessage: "test message")|];
					[|Assert.False(!false, userMessage: "test message")|];
					[|Assert.True(!condition, userMessage: "test message")|];
					[|Assert.False(!condition, userMessage: "test message")|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					bool condition = true;

					// Not negated
					Assert.True(false);
					Assert.False(false);
					Assert.True(condition);
					Assert.False(condition);

					// Negated
					Assert.False(false);
					Assert.True(false);
					Assert.False(condition);
					Assert.True(condition);

					// Not negated, with message
					Assert.True(false, "test message");
					Assert.False(false, "test message");
					Assert.True(condition, "test message");
					Assert.False(condition, "test message");

					// Negated, with message
					Assert.False(false, "test message");
					Assert.True(false, "test message");
					Assert.False(condition, "test message");
					Assert.True(condition, "test message");

					// Not negated, with named parameter message
					Assert.True(false, userMessage: "test message");
					Assert.False(false, userMessage: "test message");
					Assert.True(condition, userMessage: "test message");
					Assert.False(condition, userMessage: "test message");

					// Negated, with named parameter message
					Assert.False(false, userMessage: "test message");
					Assert.True(false, userMessage: "test message");
					Assert.False(condition, userMessage: "test message");
					Assert.True(condition, userMessage: "test message");
				}
			}
			""";

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}

	[Fact]
	public async Task FixAll_ReplacesAllNegatedBooleanAsserts()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					bool condition = true;

					[|Assert.True(!condition)|];
					[|Assert.False(!condition)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					bool condition = true;

					Assert.False(condition);
					Assert.True(condition);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, BooleanAssertsShouldNotBeNegatedFixer.Key_UseSuggestedAssert);
	}
}
