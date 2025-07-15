using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck>;
using Verify_v3_Pre_301 = CSharpVerifier<BooleanAssertsShouldNotBeUsedForSimpleEqualityCheckTests.Analyzer_v3_Pre301>;

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

		[Fact]
		public async Task ComparingAgainstNullPointer_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					public unsafe void TestMethod() {
						var value = 42;
						var ptr = &value;

						Assert.True(ptr == null);
						Assert.True(null == ptr);
						Assert.True(ptr != null);
						Assert.True(null != ptr);

						Assert.False(ptr == null);
						Assert.False(null == ptr);
						Assert.False(ptr != null);
						Assert.False(null != ptr);
					}
				}
				""";

			await Verify.VerifyAnalyzerV2(source);
			await Verify_v3_Pre_301.VerifyAnalyzerV3(source);
		}

		[Fact]
		public async Task ComparingAgainstNullPointer_v3_301_Triggers()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					public unsafe void TestMethod() {
						var value = 42;
						var ptr = &value;

						{|#0:Assert.True(ptr == null)|};
						{|#1:Assert.True(null == ptr)|};
						{|#2:Assert.True(ptr != null)|};
						{|#3:Assert.True(null != ptr)|};

						{|#10:Assert.False(ptr == null)|};
						{|#11:Assert.False(null == ptr)|};
						{|#12:Assert.False(ptr != null)|};
						{|#13:Assert.False(null != ptr)|};
					}
				}
				""";
			var expected = new[]
			{
				Verify.Diagnostic("xUnit2024").WithLocation(0).WithArguments("True", "Null"),
				Verify.Diagnostic("xUnit2024").WithLocation(1).WithArguments("True", "Null"),
				Verify.Diagnostic("xUnit2024").WithLocation(2).WithArguments("True", "NotNull"),
				Verify.Diagnostic("xUnit2024").WithLocation(3).WithArguments("True", "NotNull"),

				Verify.Diagnostic("xUnit2024").WithLocation(10).WithArguments("False", "NotNull"),
				Verify.Diagnostic("xUnit2024").WithLocation(11).WithArguments("False", "NotNull"),
				Verify.Diagnostic("xUnit2024").WithLocation(12).WithArguments("False", "Null"),
				Verify.Diagnostic("xUnit2024").WithLocation(13).WithArguments("False", "Null"),
			};

			await Verify.VerifyAnalyzerV3(source, expected);
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

	public class Analyzer_v3_Pre301 : BooleanAssertsShouldNotBeUsedForSimpleEqualityCheck
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3(compilation, new(3, 0, 0));
	}
}
