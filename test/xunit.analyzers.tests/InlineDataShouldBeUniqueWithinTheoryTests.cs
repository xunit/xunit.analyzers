using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public abstract class InlineDataShouldBeUniqueWithinTheoryTests
    {
        readonly DiagnosticAnalyzer analyzer = new InlineDataShouldBeUniqueWithinTheory();

        public class ForNonRelatedToInlineDataMethod : InlineDataShouldBeUniqueWithinTheoryTests
        {
            [Fact]
            public async void DoesNotFindError_WhenNoDataAttributes()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Fact] public void TestMethod() { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("MemberData(\"\")")]
            [InlineData("ClassData(typeof(string))")]
            public async void DoesNotFindError_WhenDataAttributesOtherThanInline(
                string dataAttribute)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{" +
                    "   [Xunit.Theory, Xunit." + dataAttribute + "] public void TestMethod() { } " +
                    "}");

                Assert.Empty(diagnostics);
            }
        }

        public class ForUniqueInlineDataMethod : InlineDataShouldBeUniqueWithinTheoryTests
        {
            [Fact]
            public async void DoesNotFindError_WhenNonTheorySingleInlineData()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{" +
                    "   [Xunit.Fact, Xunit.InlineData]" +
                    "   public void TestMethod(int x) { } " +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_WhenNonTheoryDoubledInlineData()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{" +
                    "   [Xunit.Fact, Xunit.InlineData, Xunit.InlineData]" +
                    "   public void TestMethod(int x) { } " +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_WhenSingleInlineDataContainingValue()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{" +
                    "   [Xunit.Theory, Xunit.InlineData(10)]" +
                    "   public void TestMethod(int x) { } " +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_WhenInlineDataAttributesHaveDifferentParameterValues()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{" +
                    "   [Xunit.Theory, Xunit.InlineData(10), Xunit.InlineData(20)]" +
                    "   public void TestMethod(int x) { } " +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_WhenInlineDataAttributesDifferAtLastParameterValue()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{ " +
                    "   [Xunit.Theory, Xunit.InlineData(10, \"foo\"), Xunit.InlineData(10, \"bar\")]" +
                    "   public void TestMethod(int x, string y) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("new object[] { 1, 3}")]
            [InlineData("data: new object[] { 1, 3 }")]
            [InlineData("new object[] {}")]
            [InlineData("data: new object[] { 1 }")]
            public async void DoesNotFindError_WhenUniquenessProvidedWithParamsInitializerValues(string secondInlineDataParams)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{ " +
                    $"   [Xunit.Theory, Xunit.InlineData(1, 2), Xunit.InlineData({secondInlineDataParams})]" +
                    "   public void TestMethod(params int[] args) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_WhenUniquenessProvidedWithOverridingDefaultValues()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{ " +
                    "   [Xunit.Theory, Xunit.InlineData(1), Xunit.InlineData(1, \"non-default-val\")]" +
                    "   public void TestMethod(int x, string a = \"default-val\") { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_WhenNullAndEmptyInlineDataAttributes()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{ " +
                    "   [Xunit.Theory, Xunit.InlineData(null), Xunit.InlineData]" +
                    "   public void TestMethod(string s) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }
            
            [Fact]
            public async void DoesNotFindError_WhenFirstArrayIsEqualAndEmptyArraysAreUsed()
            {
                //Specially crafted InlineData values that will cause the InlineDataUniquenessComparer
                //to return same hashcodes, because GetFlattenedArgumentPrimitives ignores empty arrays.
                //This will trigger the actual bug, where the first parameter object array being equal
                //would cause the other parameters to not be evaluated for equality at all.
                var code = 
                    @"public class TestClass { 
        [Xunit.Theory]
        [Xunit.InlineData(new int[] { 1 }, new int[0], new int[] { 1 })]
        [Xunit.InlineData(new int[] { 1 }, new int[] { 1 }, new int[0])]
        public static void Test(int[] x, int[] y, int[] z) { }
}";
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    code);

                Assert.Empty(diagnostics);
                
            }
        }

        public class ForDuplicatedInlineDataMethod : InlineDataShouldBeUniqueWithinTheoryTests
        {
            [Fact]
            public async void FindsError_WhenEmptyInlineDataRepeatedTwice()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{" +
                    "   [Xunit.Theory, Xunit.InlineData, Xunit.InlineData]" +
                    "   public void TestMethod(int x) { }" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Fact]
            public async void FindsError_WhenNullInlineDataRepeatedTwice()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{" +
                    "   [Xunit.Theory, Xunit.InlineData(null), Xunit.InlineData(null)]" +
                    "   public void TestMethod(string x) { }" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Fact]
            public async void FindsError_WhenInlineDataAttributesHaveExactlySameDeclarations()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{" +
                    "   [Xunit.Theory, Xunit.InlineData(10), Xunit.InlineData(10)]" +
                    "   public void TestMethod(int x) { }" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Fact]
            public async void FindsError_WhenInlineDataAttributesHaveSameCompilationTimeEvaluation()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   private const int X = 10; " +
                    "   [Xunit.Theory, Xunit.InlineData(10), Xunit.InlineData(X)]" +
                    "   public void TestMethod(int x) { }" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Theory]
            [InlineData("new object[] { 10, 20 }")]
            [InlineData("data: new object[] { 10, 20 }")]
            public async void FindsError_WhenInlineDataHaveSameParameterValuesButDeclaredArrayCollectionOfArguments(
                string secondInlineDataArguments)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{" +
                    $"   [Xunit.Theory, Xunit.InlineData(10, 20), Xunit.InlineData({secondInlineDataArguments})]" +
                    "   public void TestMethod(int x, int y) { } " +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Theory]
            [InlineData("new object[] { 10, 20 }")]
            [InlineData("data: new object[] { 10, 20 }")]
            public async void FindsError_WhenTestMethodIsDefinedWithParamsArrayOfArguments(string secondInlineDataArguments)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    $"   [Xunit.Theory, Xunit.InlineData(10, 20), Xunit.InlineData({secondInlineDataArguments})]" +
                    "   public void TestMethod(params int[] args) { }" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Fact]
            public async void FindsError_WhenBothInlineDataHaveObjectArrayCollectionOfArguments()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory, Xunit.InlineData(new object[] {10, 20}), Xunit.InlineData(new object[] {10, 20})]" +
                    "   public void TestMethod(int x, int y) { }" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Fact]
            public async void FindsError_WhenArgumentsAreArrayOfValues()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory]" +
                    "   [Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}})]" +
                    "   [Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}})]" +
                    "   public void TestMethod(object x, object y) { }" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Fact]
            public async void FindsError_WhenArgumentsAreArrayOfValuesAndTestMethodOffersDefaultParameterValues()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory]" +
                    "   [Xunit.InlineData(10, new object[] { new object[] {20}, 30}, 40)]" + Environment.NewLine +
                    "   [Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}})]" + Environment.NewLine +
                    "   [Xunit.InlineData(new object[] {10, new object[] { new object[] {20}, 30}, 50})]" + Environment.NewLine +
                    "   [Xunit.InlineData(new object[] {10, new object[] { new object[] {90}, 30}, 40})]" + Environment.NewLine +
                    "   public void TestMethod(object x, object y, int z = 40) { }" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
                Assert.Collection(diagnostics, d => Assert.Equal(1, d.Location.GetLineSpan().StartLinePosition.Line));
            }

            public static TheoryData<int> DefaultValueData {get;} = new TheoryData<int>() 
            {
                //the value 1 doesn't seem to trigger bugs related to comparing boxed values, but 2 does
                1,
                2
            };
            [Theory]
            [MemberData(nameof(DefaultValueData))]
            public async void FindsError_WhenFirstDuplicatedByDefaultValueOfParameter_DefaultInlineDataFirst(int defaultValue)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    $"   [Xunit.Theory, Xunit.InlineData(), Xunit.InlineData({defaultValue})]" +
                    $"   public void TestMethod(int y = {defaultValue}) {{ }}" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Theory]
            [MemberData(nameof(DefaultValueData))]
            public async void FindsError_WhenSecondDuplicatedByDefaultValueOfParameter(int defaultValue)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    $"   [Xunit.Theory, Xunit.InlineData({defaultValue}), Xunit.InlineData()]" +
                    $"   public void TestMethod(int y = {defaultValue}) {{ }}" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }
            
            [Theory]
            [MemberData(nameof(DefaultValueData))]
            public async void FindsError_WhenTwoDuplicatedByDefaultValueOfParameter(int defaultValue)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "    [Xunit.Theory, Xunit.InlineData(), Xunit.InlineData()]" +
                    $"   public void TestMethod(int y = {defaultValue}) {{ }}" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Theory]
            [InlineData("null", "null")]
            [InlineData("null", "")]
            [InlineData("", "null")]
            [InlineData("", "")]
            public async void FindsError_WhenBothNullEntirelyOrBySingleDefaultParameterNullValue(string firstArg, string secondArg)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{" +
                    $"   [Xunit.Theory, Xunit.InlineData({firstArg}), Xunit.InlineData({secondArg})]" +
                    "   public void TestMethod(string x = null) { }" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Fact]
            public async void FindsError_WhenDuplicateContainsNulls()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory, Xunit.InlineData(1, null), Xunit.InlineData(new object[] {1, null})]" +
                    "   public void TestMethod(object x, object y) { }" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Theory]
            [InlineData("", "")]
            [InlineData("", ", null")]
            [InlineData(", null", "")]
            [InlineData(", null", ", null")]
            public async void FindsError_WhenDuplicateContainsDefaultOfStruct(string firstDefaultOverride,
                string secondDefaultOverride)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory]" +
                    $"   [Xunit.InlineData(1 {firstDefaultOverride})]" +
                    $"   [Xunit.InlineData(1 {secondDefaultOverride})]" +
                    "   public void TestMethod(int x, System.DateTime date = default(System.DateTime)) { }" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Theory]
            [InlineData("", "")]
            [InlineData("", ", null")]
            [InlineData(", null", "")]
            [InlineData(", null", ", null")]
            public async void FindsError_WhenDuplicateContainsDefaultOfString(string firstDefaultOverride,
                string secondDefaultOverride)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory]" +
                    $"   [Xunit.InlineData(1 {firstDefaultOverride})]" +
                    $"   [Xunit.InlineData(1 {secondDefaultOverride})]" +
                    "   public void TestMethod(int x, string y = null) { }" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Fact]
            public async void FindsError_WhenInlineDataDuplicateAndOriginalAreItemsOfDistinctAttributesLists()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory]" +
                    "   [Xunit.InlineData(10, 20), Xunit.InlineData(30, 40)]" + Environment.NewLine +
                    "   [Xunit.InlineData(50, 60), Xunit.InlineData(10, 20)]" +
                    "   public void TestMethod(int x, int y) { } " +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Fact]
            public async void FindsErrorsTwiceOnCorrectLinesReferringToInitialOccurence_WhenThreeInlineDataAttributesConstituteDuplication()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory]" +
                    "   [Xunit.InlineData(10)]" + Environment.NewLine +
                    "   [Xunit.InlineData(10)]" + Environment.NewLine +
                    "   [Xunit.InlineData(10)]" +
                    "   public void TestMethod(int x) { } " +
                    "}");

                Assert.All(diagnostics, VerifyDiagnostic);
                Assert.Collection(diagnostics,
                    d => Assert.Equal(1, d.Location.GetLineSpan().StartLinePosition.Line),
                    d => Assert.Equal(2, d.Location.GetLineSpan().StartLinePosition.Line));
            }

            [Fact]
            public async void FindsErrorOnCorrectLineReferringToInitialOccurence_WhenDuplicateIsSeparatedByOtherNonDuplicateData()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory]" +
                    "   [Xunit.InlineData(10)]" + Environment.NewLine +
                    "   [Xunit.InlineData(50)]" + Environment.NewLine +
                    "   [Xunit.InlineData(10)]" +
                    "   public void TestMethod(int x) { } " +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
                Assert.Collection(diagnostics, d => Assert.Equal(2, d.Location.GetLineSpan().StartLinePosition.Line));
            }

            [Fact]
            public async void FindsErrorOnCorrectLineReferringToInitialOccurence_WhenTwoDuplicationEquivalenceSetsExistWithinTheory()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory]" +
                    "   [Xunit.InlineData(10)]" + Environment.NewLine +
                    "   [Xunit.InlineData(20)]" + Environment.NewLine +
                    "   [Xunit.InlineData(10)]" + Environment.NewLine +
                    "   [Xunit.InlineData(20)]" + Environment.NewLine +
                    "   public void TestMethod(int x) { }" +
                    "}");

                Assert.All(diagnostics, VerifyDiagnostic);
                Assert.Collection(diagnostics,
                    d => Assert.Equal(2, d.Location.GetLineSpan().StartLinePosition.Line),
                    d => Assert.Equal(3, d.Location.GetLineSpan().StartLinePosition.Line));
            }
        }

        public static void VerifyDiagnostic(Diagnostic diagnostic)
        {
            Assert.Equal("Theory method 'TestMethod' on test class 'TestClass' has InlineData duplicate(s).", diagnostic.GetMessage());
            Assert.Equal("xUnit1025", diagnostic.Descriptor.Id);
            Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        }
    }
}
