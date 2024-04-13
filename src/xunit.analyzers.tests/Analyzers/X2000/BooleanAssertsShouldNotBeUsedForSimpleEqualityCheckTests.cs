using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;

public class BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckTests
{
	public class X2024_BooleanAssertionsShouldNotBeUsedForSimpleEqualityCheck
	{
		public static MatrixTheoryData<string, string> MethodOperator =
			new(
				new[] { Constants.Asserts.True, Constants.Asserts.False },
				new[] { "==", "!=" }
			);

		[Theory]
		[MemberData(nameof(MethodOperator))]
		public async Task ComparingAgainstNonLiteral_DoesNotTrigger(
			string method,
			string @operator)
		{
			var source = $@"
using Xunit;

public class TestClass {{
    public void TestMethod() {{
        var value1 = 42;
        var value2 = 2112;
        var value3 = new {{ innerValue = 2600 }};

        Assert.{method}(value1 {@operator} value2);
        Assert.{method}(value1 {@operator} value3.innerValue);
    }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		public static MatrixTheoryData<string, string, string> MethodOperatorValue =
			new(
				new[] { Constants.Asserts.True, Constants.Asserts.False },
				new[] { "==", "!=" },
				new[] { "\"bacon\"", "'5'", "5", "5l", "5.0d", "5.0f", "5.0m", "MyEnum.Bacon" }
			);

		[Theory]
		[MemberData(nameof(MethodOperatorValue))]
		public async Task ComparingAgainstLiteral_WithMessage_DoesNotTrigger(
			string method,
			string @operator,
			string value)
		{
			var source = $@"
using Xunit;

public enum MyEnum {{ None, Bacon, Veggie }}

public class TestClass {{
    public void TestMethod() {{
        var value = {value};

        Assert.{method}(value {@operator} {value}, ""message"");
        Assert.{method}({value} {@operator} value, ""message"");
    }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MethodOperatorValue))]
		public async Task ComparingAgainstLiteral_WithoutMessage_Triggers(
			string method,
			string @operator,
			string value)
		{
			var source = $@"
using Xunit;

public enum MyEnum {{ None, Bacon, Veggie }}

public class TestClass {{
    public void TestMethod() {{
        var value = {value};

        Assert.{method}(value {@operator} {value});
        Assert.{method}({value} {@operator} value);
    }}
}}";
			var suggestedAssert =
				(method, @operator) switch
				{
					(Constants.Asserts.True, "==") or (Constants.Asserts.False, "!=") => Constants.Asserts.Equal,
					(_, _) => Constants.Asserts.NotEqual,
				};
			DiagnosticResult[] expected =
			{
				Verify
					.Diagnostic("xUnit2024")
					.WithSpan(10, 9, 10, 27 + method.Length + value.Length)
					.WithSeverity(DiagnosticSeverity.Info)
					.WithArguments(method, suggestedAssert),
				Verify
					.Diagnostic("xUnit2024")
					.WithSpan(11, 9, 11, 27 + method.Length + value.Length)
					.WithSeverity(DiagnosticSeverity.Info)
					.WithArguments(method, suggestedAssert),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}

		public static MatrixTheoryData<string, string, string> MethodOperatorType =
			new(
				new[] { Constants.Asserts.True, Constants.Asserts.False },
				new[] { "==", "!=" },
				new[] { "string", "int", "object", "MyEnum" }
			);

		[Theory]
		[MemberData(nameof(MethodOperatorType))]
		public async Task ComparingAgainstNull_WithMessage_DoesNotTrigger(
			string method,
			string @operator,
			string type)
		{
			var source = $@"
using Xunit;

public enum MyEnum {{ None, Bacon, Veggie }}

public class TestClass {{
    {type}? field = default;

    public void TestMethod() {{
        Assert.{method}(field {@operator} null, ""Message"");
        Assert.{method}(null {@operator} field, ""Message"");
    }}
}}";

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Theory]
		[MemberData(nameof(MethodOperatorType))]
		public async Task ComparingAgainstNull_WithoutMessage_Triggers(
			string method,
			string @operator,
			string type)
		{
			var source = $@"
using Xunit;

public enum MyEnum {{ None, Bacon, Veggie }}

public class TestClass {{
    {type}? field = default;

    public void TestMethod() {{
        Assert.{method}(field {@operator} null);
        Assert.{method}(null {@operator} field);
    }}
}}";
			var suggestedAssert =
				(method, @operator) switch
				{
					(Constants.Asserts.True, "==") or (Constants.Asserts.False, "!=") => Constants.Asserts.Null,
					(_, _) => Constants.Asserts.NotNull,
				};
			DiagnosticResult[] expected = new[]
			{
				Verify
					.Diagnostic("xUnit2024")
					.WithSpan(10, 9, 10, 31 + method.Length)
					.WithSeverity(DiagnosticSeverity.Info)
					.WithArguments(method, suggestedAssert),
				Verify
					.Diagnostic("xUnit2024")
					.WithSpan(11, 9, 11, 31 + method.Length)
					.WithSeverity(DiagnosticSeverity.Info)
					.WithArguments(method, suggestedAssert),
			};

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}
	}

	public class X2025_BooleanAssertionCanBeSimplified
	{
		public static MatrixTheoryData<string, string, string> MethodOperatorValue =
			new(
				new[] { Constants.Asserts.True, Constants.Asserts.False },
				new[] { "==", "!=" },
				new[] { "true", "false" }
			);

		[Theory]
		[MemberData(nameof(MethodOperatorValue))]
		public async Task ComparingAgainstBooleanLiteral_Triggers(
			string method,
			string @operator,
			string value)
		{
			var source = $@"
using Xunit;

public class TestClass {{
    bool field = {value};

    void TestMethod() {{
        Assert.{method}(field {@operator} {value});
        Assert.{method}(field {@operator} {value}, ""Message"");
        Assert.{method}({value} {@operator} field);
        Assert.{method}({value} {@operator} field, ""Message"");
    }}
}}";
			DiagnosticResult[] expected = new[]
			{
				Verify
					.Diagnostic("xUnit2025")
					.WithSpan(8, 9, 8, 27 + method.Length + value.Length)
					.WithSeverity(DiagnosticSeverity.Info)
					.WithArguments(method),
				Verify
					.Diagnostic("xUnit2025")
					.WithSpan(9, 9, 9, 38 + method.Length + value.Length)
					.WithSeverity(DiagnosticSeverity.Info)
					.WithArguments(method),
				Verify
					.Diagnostic("xUnit2025")
					.WithSpan(10, 9, 10, 27 + method.Length + value.Length)
					.WithSeverity(DiagnosticSeverity.Info)
					.WithArguments(method),
				Verify
					.Diagnostic("xUnit2025")
					.WithSpan(11, 9, 11, 38 + method.Length + value.Length)
					.WithSeverity(DiagnosticSeverity.Info)
					.WithArguments(method),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}
	}
}
