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
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckBooleanFixer.Key_UseSuggestedAssert);
	}
}
