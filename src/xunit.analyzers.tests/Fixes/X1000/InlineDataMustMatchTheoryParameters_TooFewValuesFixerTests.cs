using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;

public class InlineDataMustMatchTheoryParameters_TooFewValuesFixerTests
{
	[Theory]
	[InlineData("bool", "false")]
	[InlineData("char", "'\\0'")]
	[InlineData("double", "0D")]
	[InlineData("float", "0F")]
	[InlineData("int", "0")]
	[InlineData("string", "\"\"")]
	[InlineData("object", "null")]
	[InlineData("Color", "default(Color)")]
	public async void MakesParameterNullable(
		string valueType,
		string defaultValue)
	{
		var before = $@"
using Xunit;

public enum Color {{ Red, Green, Blue }}

public class TestClass {{
    [Theory]
    [{{|xUnit1009:InlineData|}}]
    public void TestMethod({valueType} p) {{ }}
}}";

		var after = $@"
using Xunit;

public enum Color {{ Red, Green, Blue }}

public class TestClass {{
    [Theory]
    [InlineData({defaultValue})]
    public void TestMethod({valueType} p) {{ }}
}}";

		await Verify.VerifyCodeFixAsyncV2(before, after, InlineDataMustMatchTheoryParameters_TooFewValuesFixer.Key_AddDefaultValues);
	}
}
