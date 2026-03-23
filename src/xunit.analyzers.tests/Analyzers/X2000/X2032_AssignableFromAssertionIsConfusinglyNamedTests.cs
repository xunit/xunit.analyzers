using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssignableFromAssertionIsConfusinglyNamed>;
using Verify_v2_Pre2_9_3 = CSharpVerifier<X2032_AssignableFromAssertionIsConfusinglyNamedTests.Analyzer_v2_Pre2_9_3>;
using Verify_v3_Pre0_6_0 = CSharpVerifier<X2032_AssignableFromAssertionIsConfusinglyNamedTests.Analyzer_v3_Pre0_6_0>;

public class X2032_AssignableFromAssertionIsConfusinglyNamedTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {
				void TestMethod() {
					{|#0:Assert.IsAssignableFrom(typeof(object), new object())|};
					{|#1:Assert.IsAssignableFrom<object>(new object())|};

					{|#2:Assert.IsNotAssignableFrom(typeof(object), new object())|};
					{|#3:Assert.IsNotAssignableFrom<object>(new object())|};
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("IsAssignableFrom", "IsType"),
			Verify.Diagnostic().WithLocation(1).WithArguments("IsAssignableFrom", "IsType"),
			Verify.Diagnostic().WithLocation(2).WithArguments("IsNotAssignableFrom", "IsNotType"),
			Verify.Diagnostic().WithLocation(3).WithArguments("IsNotAssignableFrom", "IsNotType"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask V2_and_V3_PreInexactMatchSupport()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {
				void TestMethod() {
					Assert.IsAssignableFrom(typeof(object), new object());
					Assert.IsAssignableFrom<object>(new object());

					Assert.IsNotAssignableFrom(typeof(object), new object());
					Assert.IsNotAssignableFrom<object>(new object());
				}
			}
			""";

		await Verify_v2_Pre2_9_3.VerifyAnalyzer(source);
		await Verify_v3_Pre0_6_0.VerifyAnalyzer(source);
	}

	internal class Analyzer_v2_Pre2_9_3 : AssignableFromAssertionIsConfusinglyNamed
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 9, 2));
	}

	internal class Analyzer_v3_Pre0_6_0 : AssignableFromAssertionIsConfusinglyNamed
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3(compilation, new Version(0, 5, 999));
	}
}
