using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertSingleShouldUseTwoArgumentCall>;

public class X2031_AssertSingleShouldUseTwoArgumentCallFixerTests
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

					[|Assert.Single(list.Where(f => f > 0))|];
					[|Assert.Single(list.Where(n => n == 1))|];
					[|Assert.Single(list.Where(IsEven))|];
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

					Assert.Single(list, f => f > 0);
					Assert.Single(list, n => n == 1);
					Assert.Single(list, IsEven);
				}

				public bool IsEven(int num) => num % 2 == 0;
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertSingleShouldUseTwoArgumentCallFixer.Key_UseTwoArguments);
	}
}
