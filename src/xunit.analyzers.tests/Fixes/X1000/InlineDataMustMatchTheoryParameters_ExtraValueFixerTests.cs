using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;

public class InlineDataMustMatchTheoryParameters_ExtraValueFixerTests
{
	[Fact]
	public async Task RemovesUnusedData()
	{
		var before = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(42, {|xUnit1011:21.12|})]
    public void TestMethod(int a) { }
}";

		var after = @"
using Xunit;

public class TestClass {
    [Theory]
    [InlineData(42)]
    public void TestMethod(int a) { }
}";

		await Verify.VerifyCodeFix(before, after, InlineDataMustMatchTheoryParameters_ExtraValueFixer.Key_RemoveExtraDataValue);
	}

	[Theory]
	[InlineData("21.12", "double")]
	[InlineData(@"""Hello world""", "string")]
	public async Task AddsParameterWithCorrectType(
		string value,
		string valueType)
	{
		var before = $@"
using Xunit;

public class TestClass {{
    [Theory]
    [InlineData(42, {{|xUnit1011:{value}|}})]
    public void TestMethod(int a) {{ }}
}}";

		var after = $@"
using Xunit;

public class TestClass {{
    [Theory]
    [InlineData(42, {value})]
    public void TestMethod(int a, {valueType} p) {{ }}
}}";

		await Verify.VerifyCodeFix(before, after, InlineDataMustMatchTheoryParameters_ExtraValueFixer.Key_AddTheoryParameter);
	}

	[Fact]
	public async Task AddsParameterWithNonConflictingName()
	{
		var before = $@"
using Xunit;

public class TestClass {{
    [Theory]
    [InlineData(42, {{|xUnit1011:21.12|}})]
    public void TestMethod(int p) {{ }}
}}";

		var after = $@"
using Xunit;

public class TestClass {{
    [Theory]
    [InlineData(42, 21.12)]
    public void TestMethod(int p, double p_2) {{ }}
}}";

		await Verify.VerifyCodeFix(before, after, InlineDataMustMatchTheoryParameters_ExtraValueFixer.Key_AddTheoryParameter);
	}
}
