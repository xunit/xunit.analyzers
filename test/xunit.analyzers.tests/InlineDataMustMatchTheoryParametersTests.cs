using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class InlineDataMustMatchTheoryParametersTests
    {
        public class Analyzer
        {
            readonly DiagnosticAnalyzer analyzer = new InlineDataMustMatchTheoryParameters();

            [Fact]
            public async void DoesNotFindErrorForFactMethodWithNoDataAttributes()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { [Xunit.Fact] public void TestMethod() { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindErrorForFactMethodWithAttribute()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Fact, Xunit.InlineData] public void TestMethod(string a) { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindErrorForTheoryWithCorrectAttributeUsingParamsArray()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Fact, Xunit.InlineData(\"abc\", 1, null)] public void TestMethod(string a, int b, object c) { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindErrorForTheoryWithCorrectAttributeUsingExplicitArray()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Fact, Xunit.InlineData(new object[] {\"abc\", 1, null})] public void TestMethod(string a, int b, object c) { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindErrorForTheoryWithCorrectAttributeUsingExplicitNamedArray()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Fact, Xunit.InlineData(data: new object[] {\"abc\", 1, null})] public void TestMethod(string a, int b, object c) { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindErrorForTheoryWithCorrectAttributeUsingImplicitArray()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Fact, Xunit.InlineData(new [] {(object)\"abc\", 1, null})] public void TestMethod(string a, int b, object c) { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindErrorForTheoryWithCorrectAttributeUsingImplicitNamedArray()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Fact, Xunit.InlineData(data: new [] {(object)\"abc\", 1, null})] public void TestMethod(string a, int b, object c) { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void FindsErrorForAttributeWithTooFewValues()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(1)] public void TestMethod(int a, int b, string c) { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("InlineData values must match the number of method parameters", d.GetMessage());
                      Assert.Equal("xUnit1009", d.Descriptor.Id);
                      Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                  });
            }

            [Fact]
            public async void FindsErrorForAttributeWithTooManyValues()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, 2, \"abc\")] public void TestMethod(int a) { } }");

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("There is no matching method parameter for value: 2.", d.GetMessage());
                        Assert.Equal("xUnit1011", d.Descriptor.Id);
                        Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                    },
                    d =>
                    {
                        Assert.Equal("There is no matching method parameter for value: \"abc\".", d.GetMessage());
                        Assert.Equal("xUnit1011", d.Descriptor.Id);
                        Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                    });
            }

            public static TheoryData<string> ValueTypes { get; } = new TheoryData<string>
            {
                "bool",
                "int",
                "byte",
                "short",
                "long",
                "decimal",
                "double",
                "float",
                "char",
                "ulong",
                "uint",
                "ushort",
                "sbyte",
                "System.StringComparison",
                "System.Guid",
            };

            [Fact]
            public async void FindsWarningForAttributeWithSingleNullValue()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(null)] public void TestMethod(int a) { } }");

                Assert.Collection(diagnostics,
                   d =>
                   {
                       Assert.Equal("Null should not be used for value type parameter 'a' of type 'int'.", d.GetMessage());
                       Assert.Equal("xUnit1012", d.Descriptor.Id);
                       Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                   });
            }

            [Theory]
            [MemberData(nameof(ValueTypes))]
            public async void FindsWarningForAttributeWithNullValueForValueTypeParameter(string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, null)] public void TestMethod(int a, " + type + " b) { } }");

                Assert.Collection(diagnostics,
                   d =>
                   {
                       Assert.Equal("Null should not be used for value type parameter 'b' of type '" + type + "'.", d.GetMessage());
                       Assert.Equal("xUnit1012", d.Descriptor.Id);
                       Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                   });
            }

            [Theory]
            [MemberData(nameof(ValueTypes))]
            public async void DoesNotFindWarningForAttributeWithNullValueForNullableValueType(string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, null)] public void TestMethod(int a, " + type + "? b) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("object")]
            [InlineData("string")]
            [InlineData("System.Exception")]
            public async void DoesNotFindWarningForAttributeWithNullValueForReferenceParameterType(string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, null)] public void TestMethod(int a, " + type + " b) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("true", "string")]
            [InlineData("0", "string")]
            [InlineData("0.0", "string")]
            [InlineData("System.StringComparison.OrdinalIgnoreCase", "string")]
            [InlineData("true", "int")]
            [InlineData("System.StringComparison.OrdinalIgnoreCase", "int")]
            [InlineData("0", "bool")]
            [InlineData("0.0", "bool")]
            [InlineData("\"abc\"", "bool")]
            [InlineData("System.StringComparison.OrdinalIgnoreCase", "bool")]
            public async void FindsErrorForAttributeWithValueNotConvertibleToMethodParameterType(string value, string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, " + value + ")] public void TestMethod(int a, " + type + " b) { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("The value is not convertible to the method parameter 'b' of type '" + type + "'.", d.GetMessage());
                      Assert.Equal("xUnit1010", d.Descriptor.Id);
                  });
            }
        }
    }
}

