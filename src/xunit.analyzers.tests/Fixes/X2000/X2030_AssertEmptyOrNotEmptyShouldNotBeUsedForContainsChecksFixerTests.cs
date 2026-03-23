using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecks>;

public class X2030_AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixerTests
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

					{|xUnit2030:Assert.NotEmpty(list.Where(f => f > 0))|};
					{|xUnit2030:Assert.NotEmpty(list.Where(n => n == 1))|};
					{|xUnit2030:Assert.NotEmpty(list.Where(IsEven))|};
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

					Assert.Contains(list, f => f > 0);
					Assert.Contains(list, n => n == 1);
					Assert.Contains(list, IsEven);
				}

				public bool IsEven(int num) => num % 2 == 0;
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecksFixer.Key_UseContains);
	}
}
