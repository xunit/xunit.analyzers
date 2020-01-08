using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.AssertEqualLiteralValueShouldBeFirst>;

namespace Xunit.Analyzers
{
	public class AssertEqualLiteralValueShouldBeFirstTests
	{
		[Fact]
		public async void DoesNotFindWarningWhenConstantOrLiteralUsedForBothArguments()
		{
			var source =
@"class TestClass { void TestMethod() {
    Xunit.Assert.Equal(""TestMethod"", nameof(TestMethod));
} }";

			await Verify.VerifyAnalyzerAsync(source);
		}

		public static TheoryData<string, string> TypesAndValues { get; } = new TheoryData<string, string>
			{
				{"int", "0"},
				{"int", "0.0"},
				{"int", "sizeof(int)"},
				{"int", "default(int)"},
				{"string", "null"},
				{"string", "\"\""},
				{"string", "nameof(TestMethod)"},
				{"System.Type", "typeof(string)"},
				{"System.AttributeTargets", "System.AttributeTargets.Constructor"},
			};

		[Theory]
		[MemberData(nameof(TypesAndValues))]
		public async void DoesNotFindWarningForExpectedConstantOrLiteralValueAsFirstArgument(string type, string value)
		{
			var source =
@"class TestClass { void TestMethod() {
    var v = default(" + type + @");
    Xunit.Assert.Equal(" + value + @", v);
} }";
			await Verify.VerifyAnalyzerAsync(source);
		}

		[Theory]
		[MemberData(nameof(TypesAndValues))]
		public async void FindsWarningForExpectedConstantOrLiteralValueAsSecondArgument(string type, string value)
		{
			var source =
@"class TestClass { void TestMethod() {
    var v = default(" + type + @");
    Xunit.Assert.Equal(v, " + value + @");
} }";

			var expected = Verify.Diagnostic().WithLocation(3, 5).WithArguments(value, "Assert.Equal(expected, actual)", "TestMethod", "TestClass");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async void DoesNotFindWarningForExpectedConstantOrLiteralValueAsNamedExpectedArgument(bool useAlternateForm)
		{
			var s = useAlternateForm ? "@" : "";
			var source =
@"class TestClass { void TestMethod() {
    var v = default(int);
    Xunit.Assert.Equal(" + s + "actual: v, " + s + @"expected: 0);
} }";

			await Verify.VerifyAnalyzerAsync(source);
		}

		[Theory]
		[MemberData(nameof(TypesAndValues))]
		public async void FindsWarningForExpectedConstantOrLiteralValueAsNamedExpectedArgument(string type, string value)
		{
			var source =
@"class TestClass { void TestMethod() {
    var v = default(" + type + @");
    Xunit.Assert.Equal(actual: " + value + @",expected: v);
} }";

			var expected = Verify.Diagnostic().WithLocation(3, 5).WithArguments(value, "Assert.Equal(expected, actual)", "TestMethod", "TestClass");
			await Verify.VerifyAnalyzerAsync(source, expected);
		}

		[Theory]
		[InlineData("{|CS1739:act|}", "exp")]
		[InlineData("expected", "{|CS1740:expected|}")]
		[InlineData("actual", "{|CS1740:actual|}")]
		[InlineData("{|CS1739:foo|}", "bar")]
		public async void DoesNotFindWarningWhenArgumentsAreNotNamedCorrectly(string firstArgumentName, string secondArgumentName)
		{
			var source =
@"class TestClass { void TestMethod() {
    var v = default(int);
    Xunit.Assert.Equal(" + firstArgumentName + @": 1, " + secondArgumentName + @": v);
} }";

			await Verify.VerifyAnalyzerAsync(source);
		}
	}
}
