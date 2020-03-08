using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.InlineDataShouldBeUniqueWithinTheory>;

namespace Xunit.Analyzers
{
	public abstract class InlineDataShouldBeUniqueWithinTheoryTests
	{
		public class ForNonRelatedToInlineDataMethod : InlineDataShouldBeUniqueWithinTheoryTests
		{
			[Fact]
			public async void DoesNotFindError_WhenNoDataAttributes()
			{
				var source = "public class TestClass { [Xunit.Fact] public void TestMethod() { } }";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[InlineData("MemberData(\"\")")]
			[InlineData("ClassData(typeof(string))")]
			public async void DoesNotFindError_WhenDataAttributesOtherThanInline(
				string dataAttribute)
			{
				var source =
					"public class TestClass " +
					"{" +
					"   [Xunit.Theory, Xunit." + dataAttribute + "] public void TestMethod() { } " +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}
		}

		public class ForUniqueInlineDataMethod : InlineDataShouldBeUniqueWithinTheoryTests
		{
			[Fact]
			public async void DoesNotFindError_WhenNonTheorySingleInlineData()
			{
				var source =
					"public class TestClass " +
					"{" +
					"   [Xunit.Fact, Xunit.InlineData]" +
					"   public void TestMethod(int x) { } " +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindError_WhenNonTheoryDoubledInlineData()
			{
				var source =
					"public class TestClass " +
					"{" +
					"   [Xunit.Fact, Xunit.InlineData, Xunit.InlineData]" +
					"   public void TestMethod(int x) { } " +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindError_WhenSingleInlineDataContainingValue()
			{
				var source =
					"public class TestClass " +
					"{" +
					"   [Xunit.Theory, Xunit.InlineData(10)]" +
					"   public void TestMethod(int x) { } " +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindError_WhenInlineDataAttributesHaveDifferentParameterValues()
			{
				var source =
					"public class TestClass " +
					"{" +
					"   [Xunit.Theory, Xunit.InlineData(10), Xunit.InlineData(20)]" +
					"   public void TestMethod(int x) { } " +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindError_WhenInlineDataAttributesDifferAtLastParameterValue()
			{
				var source =
					"public class TestClass " +
					"{ " +
					"   [Xunit.Theory, Xunit.InlineData(10, \"foo\"), Xunit.InlineData(10, \"bar\")]" +
					"   public void TestMethod(int x, string y) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Theory]
			[InlineData("new object[] { 1, 3}")]
			[InlineData("data: new object[] { 1, 3 }")]
			[InlineData("new object[] {}")]
			[InlineData("data: new object[] { 1 }")]
			public async void DoesNotFindError_WhenUniquenessProvidedWithParamsInitializerValues(string secondInlineDataParams)
			{
				var source =
					"public class TestClass " +
					"{ " +
					$"   [Xunit.Theory, Xunit.InlineData(1, 2), Xunit.InlineData({secondInlineDataParams})]" +
					"   public void TestMethod(params int[] args) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindError_WhenUniquenessProvidedWithOverridingDefaultValues()
			{
				var source =
					"public class TestClass " +
					"{ " +
					"   [Xunit.Theory, Xunit.InlineData(1), Xunit.InlineData(1, \"non-default-val\")]" +
					"   public void TestMethod(int x, string a = \"default-val\") { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}

			[Fact]
			public async void DoesNotFindError_WhenNullAndEmptyInlineDataAttributes()
			{
				var source =
					"public class TestClass " +
					"{ " +
					"   [Xunit.Theory, Xunit.InlineData(null), Xunit.InlineData]" +
					"   public void TestMethod(string s) { }" +
					"}";

				await Verify.VerifyAnalyzerAsync(source);
			}
		}

		public class ForDuplicatedInlineDataMethod : InlineDataShouldBeUniqueWithinTheoryTests
		{
			[Fact]
			public async void FindsError_WhenEmptyInlineDataRepeatedTwice()
			{
				var source =
					"public class TestClass " +
					"{" +
					"   [Xunit.Theory, Xunit.InlineData, Xunit.InlineData]" +
					"   public void TestMethod(int x) { }" +
					"}";

				var expected = Verify.Diagnostic().WithSpan(1, 61, 1, 77).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsError_WhenNullInlineDataRepeatedTwice()
			{
				var source =
					"public class TestClass " +
					"{" +
					"   [Xunit.Theory, Xunit.InlineData(null), Xunit.InlineData(null)]" +
					"   public void TestMethod(string x) { }" +
					"}";

				var expected = Verify.Diagnostic().WithSpan(1, 67, 1, 89).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsError_WhenInlineDataAttributesHaveExactlySameDeclarations()
			{
				var source =
					"public class TestClass " +
					"{" +
					"   [Xunit.Theory, Xunit.InlineData(10), Xunit.InlineData(10)]" +
					"   public void TestMethod(int x) { }" +
					"}";

				var expected = Verify.Diagnostic().WithSpan(1, 65, 1, 85).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsError_WhenInlineDataAttributesHaveSameCompilationTimeEvaluation()
			{
				var source =
					"public class TestClass" +
					"{" +
					"   private const int X = 10; " +
					"   [Xunit.Theory, Xunit.InlineData(10), Xunit.InlineData(X)]" +
					"   public void TestMethod(int x) { }" +
					"}";

				var expected = Verify.Diagnostic().WithSpan(1, 93, 1, 112).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[InlineData("new object[] { 10, 20 }")]
			[InlineData("data: new object[] { 10, 20 }")]
			public async void FindsError_WhenInlineDataHaveSameParameterValuesButDeclaredArrayCollectionOfArguments(
				string secondInlineDataArguments)
			{
				var source =
					"public class TestClass " +
					"{" +
					$"   [Xunit.Theory, Xunit.InlineData(10, 20), Xunit.InlineData({secondInlineDataArguments})]" +
					"   public void TestMethod(int x, int y) { } " +
					"}";

				var expected = Verify.Diagnostic().WithSpan(1, 69, 1, 87 + secondInlineDataArguments.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[InlineData("new object[] { 10, 20 }")]
			[InlineData("data: new object[] { 10, 20 }")]
			public async void FindsError_WhenTestMethodIsDefinedWithParamsArrayOfArguments(string secondInlineDataArguments)
			{
				var source =
					"public class TestClass" +
					"{" +
					$"   [Xunit.Theory, Xunit.InlineData(10, 20), Xunit.InlineData({secondInlineDataArguments})]" +
					"   public void TestMethod(params int[] args) { }" +
					"}";

				var expected = Verify.Diagnostic().WithSpan(1, 68, 1, 86 + secondInlineDataArguments.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsError_WhenBothInlineDataHaveObjectArrayCollectionOfArguments()
			{
				var source =
					"public class TestClass" +
					"{" +
					"   [Xunit.Theory, Xunit.InlineData(new object[] {10, 20}), Xunit.InlineData(new object[] {10, 20})]" +
					"   public void TestMethod(int x, int y) { }" +
					"}";

				var expected = Verify.Diagnostic().WithSpan(1, 83, 1, 122).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsError_WhenArgumentsAreArrayOfValues()
			{
				var source =
					"public class TestClass" +
					"{" +
					"   [Xunit.Theory]" +
					"   [Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}})]" +
					"   [Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}})]" +
					"   public void TestMethod(object x, object y) { }" +
					"}";

				var expected = Verify.Diagnostic().WithSpan(1, 124, 1, 198).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsError_WhenArgumentsAreArrayOfValuesAndTestMethodOffersDefaultParameterValues()
			{
				var source =
					"public class TestClass" +
					"{" +
					"   [Xunit.Theory]" +
					"   [Xunit.InlineData(10, new object[] { new object[] {20}, 30}, 40)]" + Environment.NewLine +
					"   [Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}})]" + Environment.NewLine +
					"   [Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}, 50})]" + Environment.NewLine +
					"   [Xunit.InlineData(new object[] {10, new object[] { new object[] {90}, 30}, 40})]" + Environment.NewLine +
					"   public void TestMethod(object x, object y, int z = 40) { }" +
					"}";

				var expected = Verify.Diagnostic().WithSpan(2, 5, 2, 79).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsError_WhenDuplicatedByDefaultValueOfParameter()
			{
				var source =
					"public class TestClass" +
					"{" +
					"   [Xunit.Theory, Xunit.InlineData(10, 1), Xunit.InlineData(10)]" +
					"   public void TestMethod(int x, int y = 1) { }" +
					"}";

				var expected = Verify.Diagnostic().WithSpan(1, 67, 1, 87).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[InlineData("null", "null")]
			[InlineData("null", "")]
			[InlineData("", "null")]
			[InlineData("", "")]
			public async void FindsError_WhenBothNullEntirelyOrBySingleDefaultParameterNullValue(string firstArg, string secondArg)
			{
				var source =
					"public class TestClass " +
					"{" +
					$"   [Xunit.Theory, Xunit.InlineData({firstArg}), Xunit.InlineData({secondArg})]" +
					"   public void TestMethod(string x = null) { }" +
					"}";

				var expected = Verify.Diagnostic().WithSpan(1, 63 + firstArg.Length, 1, 81 + firstArg.Length + secondArg.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsError_WhenDuplicateContainsNulls()
			{
				var source =
					"public class TestClass" +
					"{" +
					"   [Xunit.Theory, Xunit.InlineData(1, null), Xunit.InlineData(new object[] {1, null})]" +
					"   public void TestMethod(object x, object y) { }" +
					"}";

				var expected = Verify.Diagnostic().WithSpan(1, 69, 1, 109).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[InlineData("", "")]
			[InlineData("", ", null")]
			[InlineData(", null", "")]
			[InlineData(", null", ", null")]
			public async void FindsError_WhenDuplicateContainsDefaultOfStruct(string firstDefaultOverride,
				string secondDefaultOverride)
			{
				var source =
					"public class TestClass" +
					"{" +
					"   [Xunit.Theory]" +
					$"   [Xunit.InlineData(1 {firstDefaultOverride})]" +
					$"   [Xunit.InlineData(1 {secondDefaultOverride})]" +
					"   public void TestMethod(int x, System.DateTime date = default(System.DateTime)) { }" +
					"}";

				var expected = Verify.Diagnostic().WithSpan(1, 70 + firstDefaultOverride.Length, 1, 90 + firstDefaultOverride.Length + secondDefaultOverride.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Theory]
			[InlineData("", "")]
			[InlineData("", ", null")]
			[InlineData(", null", "")]
			[InlineData(", null", ", null")]
			public async void FindsError_WhenDuplicateContainsDefaultOfString(string firstDefaultOverride,
				string secondDefaultOverride)
			{
				var source =
					"public class TestClass" +
					"{" +
					"   [Xunit.Theory]" +
					$"   [Xunit.InlineData(1 {firstDefaultOverride})]" +
					$"   [Xunit.InlineData(1 {secondDefaultOverride})]" +
					"   public void TestMethod(int x, string y = null) { }" +
					"}";

				var expected = Verify.Diagnostic().WithSpan(1, 70 + firstDefaultOverride.Length, 1, 90 + firstDefaultOverride.Length + secondDefaultOverride.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsError_WhenInlineDataDuplicateAndOriginalAreItemsOfDistinctAttributesLists()
			{
				var source =
					"public class TestClass" +
					"{" +
					"   [Xunit.Theory]" +
					"   [Xunit.InlineData(10, 20), Xunit.InlineData(30, 40)]" + Environment.NewLine +
					"   [Xunit.InlineData(50, 60), Xunit.InlineData(10, 20)]" +
					"   public void TestMethod(int x, int y) { } " +
					"}";

				var expected = Verify.Diagnostic().WithSpan(2, 31, 2, 55).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsErrorsTwiceOnCorrectLinesReferringToInitialOccurence_WhenThreeInlineDataAttributesConstituteDuplication()
			{
				var source =
					"public class TestClass" +
					"{" +
					"   [Xunit.Theory]" +
					"   [Xunit.InlineData(10)]" + Environment.NewLine +
					"   [Xunit.InlineData(10)]" + Environment.NewLine +
					"   [Xunit.InlineData(10)]" +
					"   public void TestMethod(int x) { } " +
					"}";

				DiagnosticResult[] expected =
				{
					Verify.Diagnostic().WithSpan(2, 5, 2, 25).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass"),
					Verify.Diagnostic().WithSpan(3, 5, 3, 25).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass"),
				};
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsErrorOnCorrectLineReferringToInitialOccurence_WhenDuplicateIsSeparatedByOtherNonDuplicateData()
			{
				var source =
					"public class TestClass" +
					"{" +
					"   [Xunit.Theory]" +
					"   [Xunit.InlineData(10)]" + Environment.NewLine +
					"   [Xunit.InlineData(50)]" + Environment.NewLine +
					"   [Xunit.InlineData(10)]" +
					"   public void TestMethod(int x) { } " +
					"}";

				var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 25).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass");
				await Verify.VerifyAnalyzerAsync(source, expected);
			}

			[Fact]
			public async void FindsErrorOnCorrectLineReferringToInitialOccurence_WhenTwoDuplicationEquivalenceSetsExistWithinTheory()
			{
				var source =
					"public class TestClass" +
					"{" +
					"   [Xunit.Theory]" +
					"   [Xunit.InlineData(10)]" + Environment.NewLine +
					"   [Xunit.InlineData(20)]" + Environment.NewLine +
					"   [Xunit.InlineData(10)]" + Environment.NewLine +
					"   [Xunit.InlineData(20)]" + Environment.NewLine +
					"   public void TestMethod(int x) { }" +
					"}";

				DiagnosticResult[] expected =
				{
					Verify.Diagnostic().WithSpan(3, 5, 3, 25).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass"),
					Verify.Diagnostic().WithSpan(4, 5, 4, 25).WithSeverity(DiagnosticSeverity.Warning).WithArguments("TestMethod", "TestClass"),
				};
				await Verify.VerifyAnalyzerAsync(source, expected);
			}
		}
	}
}
