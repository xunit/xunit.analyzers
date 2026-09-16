using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;

public class X2025_BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					bool condition = true;

					{|xUnit2025:Assert.True(condition == true)|};
					{|xUnit2025:Assert.True(condition != true)|};
					{|xUnit2025:Assert.True(true == condition)|};
					{|xUnit2025:Assert.True(true != condition)|};

					{|xUnit2025:Assert.True(condition == false)|};
					{|xUnit2025:Assert.True(condition != false)|};
					{|xUnit2025:Assert.True(false == condition)|};
					{|xUnit2025:Assert.True(false != condition)|};

					{|xUnit2025:Assert.False(condition == true)|};
					{|xUnit2025:Assert.False(condition != true)|};
					{|xUnit2025:Assert.False(true == condition)|};
					{|xUnit2025:Assert.False(true != condition)|};

					{|xUnit2025:Assert.False(condition == false)|};
					{|xUnit2025:Assert.False(condition != false)|};
					{|xUnit2025:Assert.False(false == condition)|};
					{|xUnit2025:Assert.False(false != condition)|};

					{|xUnit2025:Assert.True(condition == true, "message")|};
					{|xUnit2025:Assert.False(condition == true, "message")|};

					bool? nullableCondition = true;

					{|xUnit2025:Assert.True(nullableCondition == true)|};
					{|xUnit2025:Assert.True(false == nullableCondition)|};
					{|xUnit2025:Assert.False(nullableCondition != true)|};
					{|xUnit2025:Assert.False(false != nullableCondition, "message")|};

					Assert.True(nullableCondition != true);
					Assert.False(nullableCondition == false);
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					bool condition = true;

					Assert.True(condition);
					Assert.False(condition);
					Assert.True(condition);
					Assert.False(condition);

					Assert.False(condition);
					Assert.True(condition);
					Assert.False(condition);
					Assert.True(condition);

					Assert.False(condition);
					Assert.True(condition);
					Assert.False(condition);
					Assert.True(condition);

					Assert.True(condition);
					Assert.False(condition);
					Assert.True(condition);
					Assert.False(condition);

					Assert.True(condition, "message");
					Assert.False(condition, "message");

					bool? nullableCondition = true;

					Assert.True(nullableCondition);
					Assert.False(nullableCondition);
					Assert.True(nullableCondition);
					Assert.False(nullableCondition, "message");

					Assert.True(nullableCondition != true);
					Assert.False(nullableCondition == false);
				}
			}
			""";

		await Verify.VerifyCodeFix(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixer.Key_UseSuggestedAssert);
	}
}
