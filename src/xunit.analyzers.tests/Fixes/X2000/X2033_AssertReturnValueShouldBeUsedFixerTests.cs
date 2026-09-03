using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertReturnValueShouldBeUsed>;

public class X2033_AssertReturnValueShouldBeUsedFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Linq;
			using Xunit;

			public class TestClass {
				[Fact]
				public void UsesSingleReturnValue() {
					var xs = new List<int> { 42 };
					[|Assert.Single(xs)|];
					var value = xs.Single();
				}

				[Fact]
				public void UsesTypeAssertReturnValue() {
					object value = "Hello world";
					[|Assert.IsType<string>(value)|];
					var text = (string)value;
				}

				[Fact]
				public void PicksSafeVariableNameOnCollision() {
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
				public void UsesSingleReturnValue() {
					var xs = new List<int> { 42 };
					var item = Assert.Single(xs);
					var value = item;
				}

				[Fact]
				public void UsesTypeAssertReturnValue() {
					object value = "Hello world";
					var typed = Assert.IsType<string>(value);
					var text = typed;
				}

				[Fact]
				public void PicksSafeVariableNameOnCollision() {
					var item = 42;
					var xs = new List<int> { 42 };
					var item_2 = Assert.Single(xs);
					var value = item_2;
				}
			}
			""";

		await Verify.VerifyCodeFixFixAll(before, after, AssertReturnValueShouldBeUsedFixer.Key_UseReturnValue);
	}
}
