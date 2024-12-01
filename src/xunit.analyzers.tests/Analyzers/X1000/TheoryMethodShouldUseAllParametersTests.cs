using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldUseAllParameters>;

public class TheoryMethodShouldUseAllParametersTests
{
	[Fact]
	public async Task ParameterNotReferenced_Triggers()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				[Theory]
				void TestMethod(int {|#0:unused|}) { }
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass", "unused");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task ParameterUnread_Triggers()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {
				[Theory]
				void TestMethod(int {|#0:unused|}) {
					unused = 3;
					int.TryParse("123", out unused);
				}
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass", "unused");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task MultipleUnreadParameters_Triggers()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				[Theory]
				void TestMethod(int {|#0:foo|}, int {|#1:bar|}, int {|#2:baz|}) { }
			}
			""";
		var expected = new[]
		{
			Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass", "foo"),
			Verify.Diagnostic().WithLocation(1).WithArguments("TestMethod", "TestClass", "bar"),
			Verify.Diagnostic().WithLocation(2).WithArguments("TestMethod", "TestClass", "baz"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task SomeUnreadParameters_Triggers()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {
				[Theory]
				void TestMethod(int {|#0:foo|}, int bar, int {|#1:baz|}) {
					Console.WriteLine(bar);
					baz = 3;
				}
			}
			""";
		var expected = new[]
		{
			Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass", "foo"),
			Verify.Diagnostic().WithLocation(1).WithArguments("TestMethod", "TestClass", "baz"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task ExpressionBodiedMethod_Triggers()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				[Theory]
				void TestMethod(int {|#0:unused|}) => Assert.Equal(5, 2 + 2);
			}
			""";
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass", "unused");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task ParameterRead_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {
				[Theory]
				void TestMethod(int used) {
					Console.WriteLine(used);
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task ParameterCapturedAsOutParameterInMockSetup_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {
				[Theory]
				void TestMethod(string used, int usedOut) {
					// mimicking mock setup use case
					// var mock = new Mock<IHaveOutParameter>();
					// mock.Setup(m => m.SomeMethod(out used));
					Action setup = () => int.TryParse(used, out usedOut);
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task ExpressionBodiedMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				[Theory]
				void TestMethod(int used) => Assert.Equal(used, 2 + 2);
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task WhenParameterIsDiscardNamed_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {
				[Theory]
				void TestMethod(int used, string _, object _1, DateTime _42, double {|#0:_a|})
				{
					Assert.Equal(42, used);
				}
			}
			""";
		// Only a single warning, for _a; everything else is either used or matches the discard pattern
		var expected = Verify.Diagnostic().WithLocation(0).WithArguments("TestMethod", "TestClass", "_a");

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task DoesNotCrash_MethodWithoutBody()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			class TestClass {
				[Theory]
				extern void TestMethod(int foo);
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
