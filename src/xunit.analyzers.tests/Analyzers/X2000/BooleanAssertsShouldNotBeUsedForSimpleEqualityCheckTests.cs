using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;

public class BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckTests
{
	public class X2024_BooleanAssertionsShouldNotBeUsedForSimpleEqualityCheck
	{
		public static MatrixTheoryData<string, string> MethodOperator =
			new(
				[Constants.Asserts.True, Constants.Asserts.False],
				["==", "!="]
			);

		[Theory]
		[MemberData(nameof(MethodOperator))]
		public async Task ComparingAgainstNonLiteral_DoesNotTrigger(
			string method,
			string @operator)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public class TestClass {{
				    public void TestMethod() {{
				        var value1 = 42;
				        var value2 = 2112;
				        var value3 = new {{ innerValue = 2600 }};

				        Assert.{0}(value1 {1} value2);
				        Assert.{0}(value1 {1} value3.innerValue);
				    }}
				}}
				""", method, @operator);

			await Verify.VerifyAnalyzer(source);
		}

		public static MatrixTheoryData<string, string, string> MethodOperatorValue =
			new(
				[Constants.Asserts.True, Constants.Asserts.False],
				["==", "!="],
				["\"bacon\"", "'5'", "5", "5l", "5.0d", "5.0f", "5.0m", "MyEnum.Bacon"]
			);

		[Theory]
		[MemberData(nameof(MethodOperatorValue))]
		public async Task ComparingAgainstLiteral_WithMessage_DoesNotTrigger(
			string method,
			string @operator,
			string value)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public enum MyEnum {{ None, Bacon, Veggie }}

				public class TestClass {{
				    public void TestMethod() {{
				        var value = {2};

				        Assert.{0}(value {1} {2}, "message");
				        Assert.{0}({2} {1} value, "message");
				    }}
				}}
				""", method, @operator, value);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(MethodOperatorValue))]
		public async Task ComparingAgainstLiteral_WithoutMessage_Triggers(
			string method,
			string @operator,
			string value)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public enum MyEnum {{ None, Bacon, Veggie }}

				public class TestClass {{
				    public void TestMethod() {{
				        var value = {2};

				        {{|#0:Assert.{0}(value {1} {2})|}};
				        {{|#1:Assert.{0}({2} {1} value)|}};
				    }}
				}}
				""", method, @operator, value);
			var suggestedAssert =
				(method, @operator) switch
				{
					(Constants.Asserts.True, "==") or (Constants.Asserts.False, "!=") => Constants.Asserts.Equal,
					(_, _) => Constants.Asserts.NotEqual,
				};
			var expected = new[]
			{
				Verify.Diagnostic("xUnit2024").WithLocation(0).WithArguments(method, suggestedAssert),
				Verify.Diagnostic("xUnit2024").WithLocation(1).WithArguments(method, suggestedAssert),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}

		public static MatrixTheoryData<string, string, string> MethodOperatorType =
			new(
				[Constants.Asserts.True, Constants.Asserts.False],
				["==", "!="],
				["string", "int", "object", "MyEnum"]
			);

		[Theory]
		[MemberData(nameof(MethodOperatorType))]
		public async Task ComparingAgainstNull_WithMessage_DoesNotTrigger(
			string method,
			string @operator,
			string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public enum MyEnum {{ None, Bacon, Veggie }}

				public class TestClass {{
				    {2}? field = default;

				    public void TestMethod() {{
				        Assert.{0}(field {1} null, "Message");
				        Assert.{0}(null {1} field, "Message");
				    }}
				}}
				""", method, @operator, type);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
		}

		[Theory]
		[MemberData(nameof(MethodOperatorType))]
		public async Task ComparingAgainstNull_WithoutMessage_Triggers(
			string method,
			string @operator,
			string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public enum MyEnum {{ None, Bacon, Veggie }}

				public class TestClass {{
				    {2}? field = default;

				    public void TestMethod() {{
				        {{|#0:Assert.{0}(field {1} null)|}};
				        {{|#1:Assert.{0}(null {1} field)|}};
				    }}
				}}
				""", method, @operator, type);
			var suggestedAssert =
				(method, @operator) switch
				{
					(Constants.Asserts.True, "==") or (Constants.Asserts.False, "!=") => Constants.Asserts.Null,
					(_, _) => Constants.Asserts.NotNull,
				};
			var expected = new[]
			{
				Verify.Diagnostic("xUnit2024").WithLocation(0).WithArguments(method, suggestedAssert),
				Verify.Diagnostic("xUnit2024").WithLocation(1).WithArguments(method, suggestedAssert),
			};

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source, expected);
		}
	}

	public class X2025_BooleanAssertionCanBeSimplified
	{
		public static MatrixTheoryData<string, string, string> MethodOperatorValue =
			new(
				[Constants.Asserts.True, Constants.Asserts.False],
				["==", "!="],
				["true", "false"]
			);

		[Theory]
		[MemberData(nameof(MethodOperatorValue))]
		public async Task ComparingAgainstBooleanLiteral_Triggers(
			string method,
			string @operator,
			string value)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				public class TestClass {{
				    bool field = {2};

				    void TestMethod() {{
				        {{|#0:Assert.{0}(field {1} {2})|}};
				        {{|#1:Assert.{0}(field {1} {2}, "Message")|}};
				        {{|#2:Assert.{0}({2} {1} field)|}};
				        {{|#3:Assert.{0}({2} {1} field, "Message")|}};
				    }}
				}}
				""", method, @operator, value);
			var expected = new[]
			{
				Verify.Diagnostic("xUnit2025").WithLocation(0).WithArguments(method),
				Verify.Diagnostic("xUnit2025").WithLocation(1).WithArguments(method),
				Verify.Diagnostic("xUnit2025").WithLocation(2).WithArguments(method),
				Verify.Diagnostic("xUnit2025").WithLocation(3).WithArguments(method),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}
	}
}
