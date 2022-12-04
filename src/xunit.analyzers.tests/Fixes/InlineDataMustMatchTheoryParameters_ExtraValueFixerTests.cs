using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;

public class InlineDataMustMatchTheoryParameters_ExtraValueFixerTests
{
	[Fact]
	public async void RemovesUnusedData()
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

		await Verify.VerifyCodeFixAsyncV2(before, after);
	}

	[Theory]
	[InlineData("21.12", "double")]
	[InlineData(@"""Hello world""", "string")]
	public async void AddsParameterWithCorrectType(
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

		await Verify.VerifyCodeFixAsyncV2(before, after, codeActionIndex: 1);
	}

	[Fact]
	public async void AddsParameterWithNonConflictingName()
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

		await Verify.VerifyCodeFixAsyncV2(before, after, codeActionIndex: 1);
	}
}
