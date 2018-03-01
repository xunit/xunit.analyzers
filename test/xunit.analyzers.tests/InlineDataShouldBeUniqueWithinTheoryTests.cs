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
            public async void DoesNotFind_WhenNoDataAttributes()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Fact] public void TestMethod() { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("MemberData(\"\")")]
            [InlineData("ClassData(typeof(string))")]
            public async void DoesNotFind_WhenDataAttributesOtherThanInline(
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
            public async void DoesNotFind_WhenNonTheorySingleInlineData()
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
            public async void DoesNotFind_WhenNonTheoryDoubledInlineData()
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
            public async void DoesNotFind_WhenSingleInlineDataContainingValue()
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
            public async void DoesNotFind_WhenInlineDataAttributesHaveDifferentParameterValues()
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
            public async void DoesNotFind_WhenInlineDataAttributesDifferAtLastParameterValue()
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
            public async void DoesNotFind_WhenUniquenessProvidedWithParamsInitializerValues(string secondInlineDataParams)
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
            public async void DoesNotFind_WhenUniquenessProvidedWithOverridingDefaultValues()
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
            public async void DoesNotFind_WhenNullAndEmptyInlineDataAttributes()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass " +
                    "{ " +
                    "   [Xunit.Theory, Xunit.InlineData(null), Xunit.InlineData]" +
                    "   public void TestMethod(string s) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("0.0", "double")]
            [InlineData("0.0f", "float")]
            public async void DoesNotFind_WhenZeroAndNegativeZeroInlineDataOfDoubleOrFloat(string value, string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "    [Xunit.Theory]" +
                    $"   [Xunit.InlineData({value})]" +
                    $"   [Xunit.InlineData(-{value})]" +
                    $"   public void TestMethod({type} x) {{ }}" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("0.0", "double")]
            [InlineData("0.0f", "float")]
            public async void DoesNotFind_WhenZeroAndNegativeZeroInlineDataOfDoubleOrFloatAsSecondTestMethodArguments(
                string value, string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "    [Xunit.Theory]" +
                    $"   [Xunit.InlineData(1, {value})]" +
                    $"   [Xunit.InlineData(1, -{value})]" +
                    $"   public void TestMethod(int x, {type} y) {{ }}" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("0.0", "-0.0", "double")]
            [InlineData("-0.0", "0.0", "double")]
            [InlineData("-0.0", "default(double)", "double")]
            [InlineData("0.0f", "-0.0f", "float")]
            [InlineData("-0.0f", "0.0f", "float")]
            [InlineData("-0.0f", "default(float)", "float")]
            public async void DoesNotFind_WhenZeroAndNegativeZeroInlineDataOfDoubleOrFloatIncludingParameterDefaults(
                string value, string paramDefaultValue, string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "    [Xunit.Theory]" +
                    "    [Xunit.InlineData]" +
                    $"   [Xunit.InlineData({value})]" +
                    $"   public void TestMethod({type} x = {paramDefaultValue}) {{ }}" +
                    "}");

                Assert.Empty(diagnostics);
            }
        }

        public class ForDuplicatedInlineDataMethod : InlineDataShouldBeUniqueWithinTheoryTests
        {
            [Fact]
            public async void Finds_WhenEmptyInlineDataRepeatedTwice()
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
            public async void Finds_WhenNullInlineDataRepeatedTwice()
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
            public async void Finds_WhenInlineDataAttributesHaveExactlySameDeclarations()
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
            public async void Finds_WhenInlineDataAttributesHaveSameCompilationTimeEvaluation()
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
            [InlineData("0", "int")]
            [InlineData("0", "uint")]
            [InlineData("0", "short")]
            [InlineData("0", "byte")]
            [InlineData("0", "sbyte")]
            [InlineData("0L", "long")]
            public async void Finds_WhenZeroAndNegativeZeroInlineDataOfIntegerTypes(string value, string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory]" +
                    $"   [Xunit.InlineData({value})]" +
                    $"   [Xunit.InlineData(-{value})]" +
                    $"   public void TestMethod({type} x) {{ }}" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Theory]
            [InlineData("0.0")]
            [InlineData("0.0f")]
            // this is deliberate as catching such cases would make the analyzer lot more complex
            public async void Finds_WhenZeroAndNegativeZeroInlineDataNestedInArray(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory]" +
                    $"   [Xunit.InlineData(1, new object[] {{ {value} }})]" +
                    $"   [Xunit.InlineData(1, new object[] {{ -{value} }})]" +
                    "   public void TestMethod(int x, object[] y) {{ }}" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Theory]
            [InlineData("0.0", "double")]
            [InlineData("-0.0", "double")]
            [InlineData("0.0f", "float")]
            [InlineData("-0.0f", "float")]
            public async void Finds_WhenDoubleNegativeOrPositiveZerosOfDoubleOrFloat(string value, string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory]" +
                    $"   [Xunit.InlineData({value})]" +
                    $"   [Xunit.InlineData({value})]" +
                    $"   public void TestMethod({type} x) {{ }}" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Theory]
            [InlineData("0.0", "double")]
            [InlineData("-0.0", "double")]
            [InlineData("default(double)", "double")]
            [InlineData("0.0f", "float")]
            [InlineData("-0.0f", "float")]
            [InlineData("default(float)", "float")]
            public async void Finds_WhenDoubleNegativeOrPositiveZerosOfDoubleOrFloatFromParameterDefault(
                string value, string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory]" +
                    $"   [Xunit.InlineData({value})]" +
                    $"   [Xunit.InlineData]" +
                    $"   public void TestMethod({type} x = {value}) {{ }}" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Theory]
            [InlineData("new object[] { 10, 20 }")]
            [InlineData("data: new object[] { 10, 20 }")]
            public async void Finds_WhenInlineDataHaveSameParameterValuesButDeclaredArrayCollectionOfArguments(
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
            public async void Finds_WhenTestMethodIsDefinedWithParamsArrayOfArguments(string secondInlineDataArguments)
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
            public async void Finds_WhenBothInlineDataHaveObjectArrayCollectionOfArguments()
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
            public async void Finds_WhenArgumentsAreArrayOfValues()
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
            public async void Finds_WhenArgumentsAreArrayOfValuesAndTestMethodOffersDefaultParameterValues()
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

            [Fact]
            public async void Finds_WhenDuplicatedByDefaultValueOfParameter()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass" +
                    "{" +
                    "   [Xunit.Theory, Xunit.InlineData(10, 1), Xunit.InlineData(10)]" +
                    "   public void TestMethod(int x, int y = 1) { }" +
                    "}");

                Assert.Collection(diagnostics, VerifyDiagnostic);
            }

            [Theory]
            [InlineData("null", "null")]
            [InlineData("null", "")]
            [InlineData("", "null")]
            [InlineData("", "")]
            public async void Finds_WhenBothNullEntirelyOrBySingleDefaultParameterNullValue(string firstArg, string secondArg)
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
            public async void Finds_WhenDuplicateContainsNulls()
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
            public async void Finds_WhenDuplicateContainsDefaultOfStruct(string firstDefaultOverride,
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
            public async void Finds_WhenDuplicateContainsDefaultOfString(string firstDefaultOverride,
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
            public async void Finds_WhenInlineDataDuplicateAndOriginalAreItemsOfDistinctAttributesLists()
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
            public async void FindsTwiceOnCorrectLinesReferringToInitialOccurence_WhenThreeInlineDataAttributesConstituteDuplication()
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
            public async void FindsOnCorrectLineReferringToInitialOccurence_WhenDuplicateIsSeparatedByOtherNonDuplicateData()
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
            public async void FindsOnCorrectLineReferringToInitialOccurence_WhenTwoDuplicationEquivalenceSetsExistWithinTheory()
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
