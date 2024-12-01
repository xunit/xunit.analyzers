using System.Threading.Tasks;
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
	public async Task MakesParameterNullable(
		string valueType,
		string defaultValue)
	{
		var before = string.Format(/* lang=c#-test */ """
			using Xunit;

			public enum Color {{ Red, Green, Blue }}

			public class TestClass {{
				[Theory]
				[{{|xUnit1009:InlineData|}}]
				public void TestMethod({0} p) {{ }}
			}}
			""", valueType);
		var after = string.Format(/* lang=c#-test */ """
			using Xunit;

			public enum Color {{ Red, Green, Blue }}

			public class TestClass {{
				[Theory]
				[InlineData({1})]
				public void TestMethod({0} p) {{ }}
			}}
			""", valueType, defaultValue);

		await Verify.VerifyCodeFix(before, after, InlineDataMustMatchTheoryParameters_TooFewValuesFixer.Key_AddDefaultValues);
	}
}
