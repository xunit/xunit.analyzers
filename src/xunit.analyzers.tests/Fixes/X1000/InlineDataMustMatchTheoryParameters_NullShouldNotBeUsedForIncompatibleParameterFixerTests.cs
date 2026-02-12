using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;

public class InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameterFixerTests
{
	[Fact]
	public async Task FixAll_MakesMultipleParametersNullable()
	{
		var before = /* lang=c#-test */ """
			#nullable enable

			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData({|xUnit1012:null|}, {|xUnit1012:null|})]
				public void TestMethod1(int a, int b) { }

				[Theory]
				[InlineData(42, {|xUnit1012:null|})]
				public void TestMethod2(int a, object b) { }
			}
			""";
		var after = /* lang=c#-test */ """
			#nullable enable

			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData(null, null)]
				public void TestMethod1(int? a, int? b) { }

				[Theory]
				[InlineData(42, null)]
				public void TestMethod2(int a, object? b) { }
			}
			""";

		await Verify.VerifyCodeFixFixAll(LanguageVersion.CSharp8, before, after, InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameterFixer.Key_MakeParameterNullable);
	}
}
