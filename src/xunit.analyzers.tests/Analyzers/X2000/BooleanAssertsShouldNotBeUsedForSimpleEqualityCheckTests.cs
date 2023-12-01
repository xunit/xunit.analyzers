using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;

public class BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckTests
{
	[Theory]
	[InlineData(Constants.Asserts.True, "string", "==", "\"bacon\"")]
	[InlineData(Constants.Asserts.True, "char", "==", "'5'")]
	[InlineData(Constants.Asserts.True, "int", "==", "5")]
	[InlineData(Constants.Asserts.True, "long", "==", "5l")]
	[InlineData(Constants.Asserts.True, "double", "==", "5.0d")]
	[InlineData(Constants.Asserts.True, "float", "==", "5.0f")]
	[InlineData(Constants.Asserts.True, "decimal", "==", "5.0m")]
	[InlineData(Constants.Asserts.False, "string", "!=", "\"bacon\"")]
	[InlineData(Constants.Asserts.False, "char", "!=", "'5'")]
	[InlineData(Constants.Asserts.False, "int", "!=", "5")]
	[InlineData(Constants.Asserts.False, "long", "!=", "5l")]
	[InlineData(Constants.Asserts.False, "double", "!=", "5.0d")]
	[InlineData(Constants.Asserts.False, "float", "!=", "5.0f")]
	[InlineData(Constants.Asserts.False, "decimal", "!=", "5.0m")]
	public async void DoesNotFindWarning_WhenBooleanAssert_ContainsMessage(
		string method,
		string type,
		string @operator,
		string value)
	{
		var source = $@"
class TestClass {{
    {type} field = {value};

    void TestMethod() {{
        Xunit.Assert.{method}(field {@operator} {value}, ""message"");
    }}
}}";
		var expected = new DiagnosticResult[0];

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "string", "==", "\"bacon\"")]
	[InlineData(Constants.Asserts.True, "char", "==", "'5'")]
	[InlineData(Constants.Asserts.True, "int", "==", "5")]
	[InlineData(Constants.Asserts.True, "long", "==", "5l")]
	[InlineData(Constants.Asserts.True, "double", "==", "5.0d")]
	[InlineData(Constants.Asserts.True, "float", "==", "5.0f")]
	[InlineData(Constants.Asserts.True, "decimal", "==", "5.0m")]
	[InlineData(Constants.Asserts.False, "string", "!=", "\"bacon\"")]
	[InlineData(Constants.Asserts.False, "char", "!=", "'5'")]
	[InlineData(Constants.Asserts.False, "int", "!=", "5")]
	[InlineData(Constants.Asserts.False, "long", "!=", "5l")]
	[InlineData(Constants.Asserts.False, "double", "!=", "5.0d")]
	[InlineData(Constants.Asserts.False, "float", "!=", "5.0f")]
	[InlineData(Constants.Asserts.False, "decimal", "!=", "5.0m")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestSimpleEqualityAgainstLiteral(
		string method,
		string type,
		string @operator,
		string value)
	{
		var source = $@"
class TestClass {{
    {type} field = {value};

    void TestMethod() {{
        Xunit.Assert.{method}(field {@operator} {value});
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2024")
				.WithSpan(6, 9, 6, 33 + method.Length + value.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method, Constants.Asserts.Equal)
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "string", "==", "\"bacon\"")]
	[InlineData(Constants.Asserts.True, "char", "==", "'5'")]
	[InlineData(Constants.Asserts.True, "int", "==", "5")]
	[InlineData(Constants.Asserts.True, "long", "==", "5l")]
	[InlineData(Constants.Asserts.True, "double", "==", "5.0d")]
	[InlineData(Constants.Asserts.True, "float", "==", "5.0f")]
	[InlineData(Constants.Asserts.True, "decimal", "==", "5.0m")]
	[InlineData(Constants.Asserts.False, "string", "!=", "\"bacon\"")]
	[InlineData(Constants.Asserts.False, "char", "!=", "'5'")]
	[InlineData(Constants.Asserts.False, "int", "!=", "5")]
	[InlineData(Constants.Asserts.False, "long", "!=", "5l")]
	[InlineData(Constants.Asserts.False, "double", "!=", "5.0d")]
	[InlineData(Constants.Asserts.False, "float", "!=", "5.0f")]
	[InlineData(Constants.Asserts.False, "decimal", "!=", "5.0m")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestSimpleEqualityAgainstLiteralReverseArguments(
		string method,
		string type,
		string @operator,
		string value)
	{
		var source = $@"
class TestClass {{
    {type} field = {value};

    void TestMethod() {{
        Xunit.Assert.{method}({value} {@operator} field);
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2024")
				.WithSpan(6, 9, 6, 33 + method.Length + value.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method, Constants.Asserts.Equal)
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "string", "!=", "\"bacon\"")]
	[InlineData(Constants.Asserts.True, "char", "!=", "'5'")]
	[InlineData(Constants.Asserts.True, "int", "!=", "5")]
	[InlineData(Constants.Asserts.True, "long", "!=", "5l")]
	[InlineData(Constants.Asserts.True, "double", "!=", "5.0d")]
	[InlineData(Constants.Asserts.True, "float", "!=", "5.0f")]
	[InlineData(Constants.Asserts.True, "decimal", "!=", "5.0m")]
	[InlineData(Constants.Asserts.False, "string", "==", "\"bacon\"")]
	[InlineData(Constants.Asserts.False, "char", "==", "'5'")]
	[InlineData(Constants.Asserts.False, "int", "==", "5")]
	[InlineData(Constants.Asserts.False, "long", "==", "5l")]
	[InlineData(Constants.Asserts.False, "double", "==", "5.0d")]
	[InlineData(Constants.Asserts.False, "float", "==", "5.0f")]
	[InlineData(Constants.Asserts.False, "decimal", "==", "5.0m")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestSimpleInequalityAgainstLiteral(
		string method,
		string type,
		string @operator,
		string value)
	{
		var source = $@"
class TestClass {{
    {type} field = {value};

    void TestMethod() {{
        Xunit.Assert.{method}(field {@operator} {value});
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2024")
				.WithSpan(6, 9, 6, 33 + method.Length + value.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method, Constants.Asserts.NotEqual)
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "string", "!=", "\"bacon\"")]
	[InlineData(Constants.Asserts.True, "char", "!=", "'5'")]
	[InlineData(Constants.Asserts.True, "int", "!=", "5")]
	[InlineData(Constants.Asserts.True, "long", "!=", "5l")]
	[InlineData(Constants.Asserts.True, "double", "!=", "5.0d")]
	[InlineData(Constants.Asserts.True, "float", "!=", "5.0f")]
	[InlineData(Constants.Asserts.True, "decimal", "!=", "5.0m")]
	[InlineData(Constants.Asserts.False, "string", "==", "\"bacon\"")]
	[InlineData(Constants.Asserts.False, "char", "==", "'5'")]
	[InlineData(Constants.Asserts.False, "int", "==", "5")]
	[InlineData(Constants.Asserts.False, "long", "==", "5l")]
	[InlineData(Constants.Asserts.False, "double", "==", "5.0d")]
	[InlineData(Constants.Asserts.False, "float", "==", "5.0f")]
	[InlineData(Constants.Asserts.False, "decimal", "==", "5.0m")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestSimpleInequalityAgainstLiteralReverseArguments(
		string method,
		string type,
		string @operator,
		string value)
	{
		var source = $@"
class TestClass {{
    {type} field = {value};

    void TestMethod() {{
        Xunit.Assert.{method}({value} {@operator} field);
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2024")
				.WithSpan(6, 9, 6, 33 + method.Length + value.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method, Constants.Asserts.NotEqual)
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "string", "==")]
	[InlineData(Constants.Asserts.True, "int", "==")]
	[InlineData(Constants.Asserts.True, "object", "==")]
	[InlineData(Constants.Asserts.False, "string", "!=")]
	[InlineData(Constants.Asserts.False, "int", "!=")]
	[InlineData(Constants.Asserts.False, "object", "!=")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestNull(
		string method,
		string type,
		string @operator)
	{
		var source = $@"
class TestClass {{
    {type}? field = default;

    void TestMethod() {{
        Xunit.Assert.{method}(field {@operator} null);
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2024")
				.WithSpan(6, 9, 6, 37 + method.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method, Constants.Asserts.Null)
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "string", "==")]
	[InlineData(Constants.Asserts.True, "int", "==")]
	[InlineData(Constants.Asserts.True, "object", "==")]
	[InlineData(Constants.Asserts.False, "string", "!=")]
	[InlineData(Constants.Asserts.False, "int", "!=")]
	[InlineData(Constants.Asserts.False, "object", "!=")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestNullLiteralReverseArguments(
		string method,
		string type,
		string @operator)
	{
		var source = $@"
class TestClass {{
    {type}? field = default;

    void TestMethod() {{
        Xunit.Assert.{method}(null {@operator} field);
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2024")
				.WithSpan(6, 9, 6, 37 + method.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method, Constants.Asserts.Null)
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
	}


	[Theory]
	[InlineData(Constants.Asserts.True, "string", "!=")]
	[InlineData(Constants.Asserts.True, "int", "!=")]
	[InlineData(Constants.Asserts.True, "object", "!=")]
	[InlineData(Constants.Asserts.False, "string", "==")]
	[InlineData(Constants.Asserts.False, "int", "==")]
	[InlineData(Constants.Asserts.False, "object", "==")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestNotNull(
		string method,
		string type,
		string @operator)
	{
		var source = $@"
class TestClass {{
    {type}? field = default;

    void TestMethod() {{
        Xunit.Assert.{method}(field {@operator} null);
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2024")
				.WithSpan(6, 9, 6, 37 + method.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method, Constants.Asserts.NotNull)
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "string", "!=")]
	[InlineData(Constants.Asserts.True, "int", "!=")]
	[InlineData(Constants.Asserts.True, "object", "!=")]
	[InlineData(Constants.Asserts.False, "string", "==")]
	[InlineData(Constants.Asserts.False, "int", "==")]
	[InlineData(Constants.Asserts.False, "object", "==")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestNotNullLiteralReverseArguments(
		string method,
		string type,
		string @operator)
	{
		var source = $@"
class TestClass {{
    {type}? field = default;

    void TestMethod() {{
        Xunit.Assert.{method}(null {@operator} field);
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2024")
				.WithSpan(6, 9, 6, 37 + method.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method, Constants.Asserts.NotNull)
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "==")]
	[InlineData(Constants.Asserts.False, "!=")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestEqualityAgainstEnumValue(
		string method,
		string @operator)
	{
		var source = $@"
public enum MyEnum {{ None, Bacon, Veggie }}

class TestClass {{
    MyEnum field = MyEnum.None;

    void TestMethod() {{
        Xunit.Assert.{method}(field {@operator} MyEnum.Bacon);
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2024")
				.WithSpan(8, 9, 8, 45 + method.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method, Constants.Asserts.Equal)
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "==")]
	[InlineData(Constants.Asserts.False, "!=")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestEqualityAgainstEnumValueReverseArguments(
		string method,
		string @operator)
	{
		var source = $@"
public enum MyEnum {{ None, Bacon, Veggie }}

class TestClass {{
    MyEnum field = MyEnum.None;

    void TestMethod() {{
        Xunit.Assert.{method}(MyEnum.Bacon {@operator} field);
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2024")
				.WithSpan(8, 9, 8, 45 + method.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method, Constants.Asserts.Equal)
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "!=")]
	[InlineData(Constants.Asserts.False, "==")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestInqualityAgainstEnumValue(
		string method,
		string @operator)
	{
		var source = $@"
public enum MyEnum {{ None, Bacon, Veggie }}

class TestClass {{
    MyEnum field = MyEnum.None;

    void TestMethod() {{
        Xunit.Assert.{method}(field {@operator} MyEnum.Bacon);
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2024")
				.WithSpan(8, 9, 8, 45 + method.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method, Constants.Asserts.NotEqual)
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "!=")]
	[InlineData(Constants.Asserts.False, "==")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestInqualityAgainstEnumValueReverseArguments(
		string method,
		string @operator)
	{
		var source = $@"
public enum MyEnum {{ None, Bacon, Veggie }}

class TestClass {{
    MyEnum field = MyEnum.None;

    void TestMethod() {{
        Xunit.Assert.{method}(MyEnum.Bacon {@operator} field);
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2024")
				.WithSpan(8, 9, 8, 45 + method.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method, Constants.Asserts.NotEqual)
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "==", "true")]
	[InlineData(Constants.Asserts.True, "==", "false")]
	[InlineData(Constants.Asserts.True, "!=", "true")]
	[InlineData(Constants.Asserts.True, "!=", "false")]
	[InlineData(Constants.Asserts.False, "==", "true")]
	[InlineData(Constants.Asserts.False, "==", "false")]
	[InlineData(Constants.Asserts.False, "!=", "true")]
	[InlineData(Constants.Asserts.False, "!=", "false")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestBooleanEquality(
		string method,
		string @operator,
		string value)
	{
		var source = $@"
class TestClass {{
    bool field = {value};

    void TestMethod() {{
        Xunit.Assert.{method}(field {@operator} {value});
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2025")
				.WithSpan(6, 9, 6, 33 + method.Length + value.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method)
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[InlineData(Constants.Asserts.True, "==", "true")]
	[InlineData(Constants.Asserts.True, "==", "false")]
	[InlineData(Constants.Asserts.True, "!=", "true")]
	[InlineData(Constants.Asserts.True, "!=", "false")]
	[InlineData(Constants.Asserts.False, "==", "true")]
	[InlineData(Constants.Asserts.False, "==", "false")]
	[InlineData(Constants.Asserts.False, "!=", "true")]
	[InlineData(Constants.Asserts.False, "!=", "false")]
	public async void FindsWarning_WhenBooleanAssert_UsedToTestBooleanEqualityReverseArguments(
		string method,
		string @operator,
		string value)
	{
		var source = $@"
class TestClass {{
    bool field = {value};

    void TestMethod() {{
        Xunit.Assert.{method}({value} {@operator} field);
    }}
}}";
		DiagnosticResult[] expected = new[]
		{
			Verify
				.Diagnostic("xUnit2025")
				.WithSpan(6, 9, 6, 33 + method.Length + value.Length)
				.WithSeverity(DiagnosticSeverity.Info)
				.WithArguments(method)
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
