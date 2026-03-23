using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssignableFromAssertionIsConfusinglyNamed>;

public class X2032_AssignableFromAssertionIsConfusinglyNamedFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = "Hello world";

					[|Assert.IsAssignableFrom(typeof(object), data)|];
					[|Assert.IsAssignableFrom<object>(data)|];
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var data = "Hello world";

					Assert.IsType(typeof(object), data, exactMatch: false);
					Assert.IsType<object>(data, exactMatch: false);
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssignableFromAssertionIsConfusinglyNamedFixer.Key_UseIsType);
	}
}
