using Microsoft.CodeAnalysis;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataShouldBeUniqueWithinTheory>;

public abstract class InlineDataShouldBeUniqueWithinTheoryTests
{
	public class ForNonRelatedToInlineDataMethod : InlineDataShouldBeUniqueWithinTheoryTests
	{
		[Fact]
		public async void DoesNotFindError_WhenNoDataAttributes()
		{
			var source = @"
public class TestClass {
    [Xunit.Fact]
    public void TestMethod() { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("MemberData(\"\")")]
		[InlineData("ClassData(typeof(string))")]
		public async void DoesNotFindError_WhenDataAttributesOtherThanInline(
			string dataAttribute)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.{dataAttribute}]
    public void TestMethod() {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class ForUniqueInlineDataMethod : InlineDataShouldBeUniqueWithinTheoryTests
	{
		[Fact]
		public async void DoesNotFindError_WhenNonTheorySingleInlineData()
		{
			var source = @"
public class TestClass {
    [Xunit.Fact]
    [Xunit.InlineData]
    public void TestMethod(int x) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void DoesNotFindError_WhenNonTheoryDoubledInlineData()
		{
			var source = @"
public class TestClass {
    [Xunit.Fact]
    [Xunit.InlineData]
    [Xunit.InlineData]
    public void TestMethod(int x) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void DoesNotFindError_WhenSingleInlineDataContainingValue()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(10)]
    public void TestMethod(int x) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void DoesNotFindError_WhenInlineDataAttributesHaveDifferentParameterValues()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(10)]
    [Xunit.InlineData(20)]
    public void TestMethod(int x) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void DoesNotFindError_WhenInlineDataAttributesDifferAtLastParameterValue()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(10, ""foo"")]
    [Xunit.InlineData(10, ""bar"")]
    public void TestMethod(int x, string y) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("new object[] { 1, 3 }")]
		[InlineData("data: new object[] { 1, 3 }")]
		[InlineData("new object[] { }")]
		[InlineData("data: new object[] { 1 }")]
		public async void DoesNotFindError_WhenUniquenessProvidedWithParamsInitializerValues(string data)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(1, 2)]
    [Xunit.InlineData({data})]
    public void TestMethod(params int[] args) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void DoesNotFindError_WhenUniquenessProvidedWithOverridingDefaultValues()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(1)]
    [Xunit.InlineData(1, ""non-default-val"")]
    public void TestMethod(int x, string a = ""default-val"") { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void DoesNotFindError_WhenNullAndEmptyInlineDataAttributes()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(null)]
    [Xunit.InlineData]
    public void TestMethod(string s) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void DoesNotFindError_WhenNewArrayAndNullDataAttributes()
		{
			var source = @"
public class TestClass{
    [Xunit.Theory]
    [Xunit.InlineData(new[] { 0 })]
    [Xunit.InlineData(null)]
    public void TestMethod(int[] arr) { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void DoesNotFindError_WhenFirstArrayIsEqualAndEmptyArraysAreUsed()
		{
			// Specially crafted InlineData values that will cause the InlineDataUniquenessComparer
			// to return same hashcodes, because GetFlattenedArgumentPrimitives ignores empty arrays.
			// This will trigger the actual bug, where the first parameter object array being equal
			// would cause the other parameters to not be evaluated for equality at all.
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new int[] { 1 }, new int[0], new int[] { 1 })]
    [Xunit.InlineData(new int[] { 1 }, new int[] { 1 }, new int[0])]
    public static void Test(int[] x, int[] y, int[] z) { }
}";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class ForDuplicatedInlineDataMethod : InlineDataShouldBeUniqueWithinTheoryTests
	{
		[Fact]
		public async void FindsError_WhenEmptyInlineDataRepeatedTwice()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData]
    [Xunit.InlineData]
    public void TestMethod(int x) { }
}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 6, 5, 22)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void FindsError_WhenNullInlineDataRepeatedTwice()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(null)]
    [Xunit.InlineData(null)]
    public void TestMethod(string x) { }
}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 6, 5, 28)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void FindsError_WhenInlineDataAttributesHaveExactlySameDeclarations()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(10)]
    [Xunit.InlineData(10)]
    public void TestMethod(int x) { }
}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 6, 5, 26)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void FindsError_WhenInlineDataAttributesHaveSameCompilationTimeEvaluation()
		{
			var source = @"
public class TestClass {
    private const int X = 10;
    [Xunit.Theory]
    [Xunit.InlineData(10)]
    [Xunit.InlineData(X)]
    public void TestMethod(int x) { }
}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(6, 6, 6, 25)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[InlineData("new object[] { 10, 20 }")]
		[InlineData("data: new object[] { 10, 20 }")]
		public async void FindsError_WhenInlineDataHaveSameParameterValuesButDeclaredArrayCollectionOfArguments(string data)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(10, 20)]
    [Xunit.InlineData({data})]
    public void TestMethod(int x, int y) {{ }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 6, 5, 24 + data.Length)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[InlineData("new object[] { 10, 20 }")]
		[InlineData("data: new object[] { 10, 20 }")]
		public async void FindsError_WhenTestMethodIsDefinedWithParamsArrayOfArguments(string data)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData(10, 20)]
    [Xunit.InlineData({data})]
    public void TestMethod(params int[] args) {{ }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 6, 5, 24 + data.Length)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void FindsError_WhenBothInlineDataHaveObjectArrayCollectionOfArguments()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new object[] { 10, 20 })]
    [Xunit.InlineData(new object[] { 10, 20 })]
    public void TestMethod(int x, int y) { }
}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 6, 5, 47)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void FindsError_WhenArgumentsAreArrayOfValues()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}})]
    [Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}})]
    public void TestMethod(object x, object y) { }
}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 6, 5, 80)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void FindsError_WhenArgumentsAreArrayOfValuesAndTestMethodOffersDefaultParameterValues()
		{
			var source = @"
public class TestClass {
   [Xunit.Theory]
   [Xunit.InlineData(10, new object[] { new object[] {20}, 30}, 40)]
   [Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}})]
   [Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}, 50})]
   [Xunit.InlineData(new object[] {10, new object[] { new object[] {90}, 30}, 40})]
   public void TestMethod(object x, object y, int z = 40) { }
}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 5, 5, 79)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		// The value 1 doesn't seem to trigger bugs related to comparing boxed values, but 2 does
		public static TheoryData<int> DefaultValueData = new() { 1, 2 };

		[Theory]
		[MemberData(nameof(DefaultValueData))]
		public async void FindsError_WhenFirstDuplicatedByDefaultValueOfParameter_DefaultInlineDataFirst(int defaultValue)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData]
    [Xunit.InlineData({defaultValue})]
    public void TestMethod(int y = {defaultValue}) {{ }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 6, 5, 25)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(DefaultValueData))]
		public async void FindsError_WhenSecondDuplicatedByDefaultValueOfParameter(int defaultValue)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({defaultValue})]
    [Xunit.InlineData]
    public void TestMethod(int y = {defaultValue}) {{ }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 6, 5, 22)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(DefaultValueData))]
		public async void FindsError_WhenTwoDuplicatedByDefaultValueOfParameter(int defaultValue)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData]
    [Xunit.InlineData]
    public void TestMethod(int y = {defaultValue}) {{ }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 6, 5, 22)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[InlineData("null", "null")]
		[InlineData("null", "")]
		[InlineData("", "null")]
		[InlineData("", "")]
		public async void FindsError_WhenBothNullEntirelyOrBySingleDefaultParameterNullValue(
			string firstArg,
			string secondArg)
		{
			var source = $@"
public class TestClass {{
    [Xunit.Theory]
    [Xunit.InlineData({firstArg})]
    [Xunit.InlineData({secondArg})]
    public void TestMethod(string x = null) {{ }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithLocation(5, 6)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void FindsError_WhenDuplicateContainsNulls()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(1, null)]
    [Xunit.InlineData(new object[] {1, null})]
    public void TestMethod(object x, object y) { }
}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 6, 5, 46)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[InlineData("", "")]
		[InlineData("", ", null")]
		[InlineData(", null", "")]
		[InlineData(", null", ", null")]
		public async void FindsError_WhenDuplicateContainsDefaultOfStruct(
			string firstDefaultOverride,
			string secondDefaultOverride)
		{
			var source = $@"
public class TestClass {{
   [Xunit.Theory]
   [Xunit.InlineData(1 {firstDefaultOverride})]
   [Xunit.InlineData(1 {secondDefaultOverride})]
   public void TestMethod(int x, System.DateTime date = default(System.DateTime)) {{ }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 5, 5, 25 + secondDefaultOverride.Length)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[InlineData("", "")]
		[InlineData("", ", null")]
		[InlineData(", null", "")]
		[InlineData(", null", ", null")]
		public async void FindsError_WhenDuplicateContainsDefaultOfString(
			string firstDefaultOverride,
			string secondDefaultOverride)
		{
			var source = $@"
public class TestClass {{
   [Xunit.Theory]
   [Xunit.InlineData(1 {firstDefaultOverride})]
   [Xunit.InlineData(1 {secondDefaultOverride})]
   public void TestMethod(int x, string y = null) {{ }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 5, 5, 25 + secondDefaultOverride.Length)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void FindsError_WhenInlineDataDuplicateAndOriginalAreItemsOfDistinctAttributesLists()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(10, 20)]
    [Xunit.InlineData(30, 40)]
    [Xunit.InlineData(50, 60)]
    [Xunit.InlineData(10, 20)]
    public void TestMethod(int x, int y) { }
}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(7, 6, 7, 30)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void FindsErrorsTwiceOnCorrectLinesReferringToInitialOccurence_WhenThreeInlineDataAttributesConstituteDuplication()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(10)]
    [Xunit.InlineData(10)]
    [Xunit.InlineData(10)]
    public void TestMethod(int x) { }
}";
			var expected = new[]
			{
				Verify
					.Diagnostic()
					.WithSpan(5, 6, 5, 26)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass"),
				Verify
					.Diagnostic()
					.WithSpan(6, 6, 6, 26)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass"),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void FindsErrorOnCorrectLineReferringToInitialOccurence_WhenDuplicateIsSeparatedByOtherNonDuplicateData()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(10)]
    [Xunit.InlineData(50)]
    [Xunit.InlineData(10)]
    public void TestMethod(int x) { }
}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(6, 6, 6, 26)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void FindsErrorOnCorrectLineReferringToInitialOccurence_WhenTwoDuplicationEquivalenceSetsExistWithinTheory()
		{
			var source = @"
public class TestClass {
    [Xunit.Theory]
    [Xunit.InlineData(10)]
    [Xunit.InlineData(20)]
    [Xunit.InlineData(10)]
    [Xunit.InlineData(20)]
    public void TestMethod(int x) { }
}";
			var expected = new[]
			{
				Verify
					.Diagnostic()
					.WithSpan(6, 6, 6, 26)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass"),
				Verify
					.Diagnostic()
					.WithSpan(7, 6, 7, 26)
					.WithSeverity(DiagnosticSeverity.Warning)
					.WithArguments("TestMethod", "TestClass"),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}
	}
}
