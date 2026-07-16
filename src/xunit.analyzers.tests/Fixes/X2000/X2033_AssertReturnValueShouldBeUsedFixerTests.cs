using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertReturnValueShouldBeUsed>;

public class X2033_AssertReturnValueShouldBeUsedFixerTests
{
	[Fact]
	public async ValueTask UsesSingleReturnValue()
	{
		var before = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var xs = new List<int> { 42 };
					[|Assert.Single(xs)|];
					var value = xs.Single();
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var xs = new List<int> { 42 };
					var item = Assert.Single(xs);
					var value = item;
				}
			}
			""";
		await Verify.VerifyCodeFix(before, after, AssertReturnValueShouldBeUsedFixer.Key_UseReturnValue);
	}

	[Fact]
	public async ValueTask UsesTypeAssertReturnValue()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					object value = "Hello world";
					[|Assert.IsType<string>(value)|];
					var text = (string)value;
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					object value = "Hello world";
					var typed = Assert.IsType<string>(value);
					var text = typed;
				}
			}
			""";
		await Verify.VerifyCodeFix(before, after, AssertReturnValueShouldBeUsedFixer.Key_UseReturnValue);
	}

	[Fact]
	public async ValueTask PicksSafeVariableNameOnCollision()
	{
		var before = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var item = 42;
					var xs = new List<int> { 42 };
					[|Assert.Single(xs)|];
					var value = xs.Single();
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void TestMethod() {
					var item = 42;
					var xs = new List<int> { 42 };
					var item_2 = Assert.Single(xs);
					var value = item_2;
				}
			}
			""";
		await Verify.VerifyCodeFix(before, after, AssertReturnValueShouldBeUsedFixer.Key_UseReturnValue);
	}
}
