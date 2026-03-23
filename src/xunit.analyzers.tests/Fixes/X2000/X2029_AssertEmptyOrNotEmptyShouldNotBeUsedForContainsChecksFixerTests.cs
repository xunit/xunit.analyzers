using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecks>;

public class X2029_AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var list = new[] { -1, 0, 1, 2 };

					{|xUnit2029:Assert.Empty(list.Where(f => f > 0))|};
					{|xUnit2029:Assert.Empty(list.Where(n => n == 1))|};
					{|xUnit2029:Assert.Empty(list.Where(IsEven))|};
				}

				public bool IsEven(int num) => num % 2 == 0;
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var list = new[] { -1, 0, 1, 2 };

					Assert.DoesNotContain(list, f => f > 0);
					Assert.DoesNotContain(list, n => n == 1);
					Assert.DoesNotContain(list, IsEven);
				}

				public bool IsEven(int num) => num % 2 == 0;
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixer.Key_UseDoesNotContain);
	}
}
