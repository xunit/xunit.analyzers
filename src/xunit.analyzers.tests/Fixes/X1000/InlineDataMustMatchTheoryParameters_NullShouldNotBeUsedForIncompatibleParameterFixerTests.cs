using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;

public class InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameterFixerTests
{
	[Fact]
	public async Task MakesParameterNullable()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Theory]
			    [InlineData(42, {|xUnit1012:null|})]
			    public void TestMethod(int a, int b) { }
			}
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
			    [Theory]
			    [InlineData(42, null)]
			    public void TestMethod(int a, int? b) { }
			}
			""";

		await Verify.VerifyCodeFix(before, after, InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameterFixer.Key_MakeParameterNullable);
	}

	[Fact]
	public async Task MakesReferenceParameterNullable()
	{
		var before = /* lang=c#-test */ """
			#nullable enable

			using Xunit;

			public class TestClass {
			    [Theory]
			    [InlineData(42, {|xUnit1012:null|})]
			    public void TestMethod(int a, object b) { }
			}
			""";
		var after = /* lang=c#-test */ """
			#nullable enable

			using Xunit;

			public class TestClass {
			    [Theory]
			    [InlineData(42, null)]
			    public void TestMethod(int a, object? b) { }
			}
			""";

		await Verify.VerifyCodeFix(LanguageVersion.CSharp8, before, after, InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameterFixer.Key_MakeParameterNullable);
	}
}
