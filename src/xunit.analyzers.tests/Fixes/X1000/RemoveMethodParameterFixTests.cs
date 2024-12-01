using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Analyzers;
using Xunit.Analyzers.Fixes;
using Verify_X1022 = CSharpVerifier<RemoveMethodParameterFixTests.Analyzer_X1022>;
using Verify_X1026 = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldUseAllParameters>;

public class RemoveMethodParameterFixTests
{
	[Fact]
	public async Task X1022_RemoveParamsArray()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1, 2, 3)]
				public void TestMethod([|params int[] values|]) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1, 2, 3)]
				public void TestMethod() { }
			}
			""";

		await Verify_X1022.VerifyCodeFix(before, after, RemoveMethodParameterFix.Key_RemoveParameter);
	}

	[Fact]
	public async Task X1026_RemovesUnusedParameter()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1)]
				public void TestMethod(int [|arg|]) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1)]
				public void TestMethod() { }
			}
			""";

		await Verify_X1026.VerifyCodeFix(before, after, RemoveMethodParameterFix.Key_RemoveParameter);
	}

	[Fact]
	public async Task X1026_DoesNotCrashWhenParameterDeclarationIsMissing()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1, 1)]
				public void Test1(int x, {|CS1001:{|CS1031:{|xUnit1026:|})|}|}
				{
					var x1 = x;
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1, 1)]
				public void Test1(int x, {|CS1001:{|CS1031:{|xUnit1026:|})|}|}
				{
					var x1 = x;
				}
			}
			""";

		await Verify_X1026.VerifyCodeFix(before, after, RemoveMethodParameterFix.Key_RemoveParameter);
	}

	internal class Analyzer_X1022 : TheoryMethodCannotHaveParamsArray
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 1, 999));
	}
}
