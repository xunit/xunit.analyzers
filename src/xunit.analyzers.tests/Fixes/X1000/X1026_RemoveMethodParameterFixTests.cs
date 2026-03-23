using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryMethodShouldUseAllParameters>;

public class X1026_RemoveMethodParameterFixTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1)]
				public void RemovesUnusedParameter(int [|arg|]) { }

				[Theory]
				[InlineData(1, 1)]
				public void DoesNotCrashWhenParameterDeclarationIsMissing(int x, {|CS1001:{|CS1031:[||])|}|}
				{
					var x1 = x;
				}
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(1)]
				public void RemovesUnusedParameter() { }

				[Theory]
				[InlineData(1, 1)]
				public void DoesNotCrashWhenParameterDeclarationIsMissing(int x, {|CS1001:{|CS1031:{|#0:|})|}|}
				{
					var x1 = x;
				}
			}
			""";
		var expected = Verify.Diagnostic("xUnit1026").WithLocation(0).WithArguments("DoesNotCrashWhenParameterDeclarationIsMissing", "TestClass", "");

		await Verify.VerifyCodeFix(before, after, RemoveMethodParameterFix.Key_RemoveParameter, expected);
	}
}
