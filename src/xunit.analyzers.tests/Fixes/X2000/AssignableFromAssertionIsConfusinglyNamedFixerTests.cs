using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssignableFromAssertionIsConfusinglyNamed>;

public class AssignableFromAssertionIsConfusinglyNamedFixerTests
{
	[Fact]
	public async Task FixAll_ReplacesAllAssignableFromAssertions()
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
