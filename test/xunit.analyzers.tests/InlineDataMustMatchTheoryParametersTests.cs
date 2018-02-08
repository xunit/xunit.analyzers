using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public abstract class InlineDataMustMatchTheoryParametersTests
    {
        readonly DiagnosticAnalyzer analyzer = new InlineDataMustMatchTheoryParameters();

        public class ForFactMethod : InlineDataMustMatchTheoryParametersTests
        {

            [Fact]
            public async void DoesNotFindError_WhenNoDataAttributes()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class TestClass { [Xunit.Fact] public void TestMethod() { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_WithAttribute()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Fact, Xunit.InlineData] public void TestMethod(string a) { } }");

                Assert.Empty(diagnostics);
            }
        }

        public class ForTheoryWithArgumentMatch : InlineDataMustMatchTheoryParametersTests
        {
            [Fact]
            public async void DoesNotFindErrorFor_MethodUsingParamsArgument()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass {" +
                    "   [Xunit.Theory, Xunit.InlineData(\"abc\", \"xyz\")]" +
                    "   public void TestMethod(params string[] args) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindErrorFor_MethodUsingNormalAndParamsArgument()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass {" +
                    "   [Xunit.Theory, Xunit.InlineData(\"abc\", \"xyz\")]" +
                    "   public void TestMethod(string first, params string[] theRest) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindErrorFor_MethodUsingNormalAndUnusedParamsArgument()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass {" +
                    "   [Xunit.Theory, Xunit.InlineData(\"abc\")]" +
                    "   public void TestMethod(string first, params string[] theRest) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindErrorFor_UsingParameters()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass {" +
                    "   [Xunit.Theory, Xunit.InlineData(\"abc\", 1, null)]" +
                    "   public void TestMethod(string a, int b, object c) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindErrorFor_UsingParametersWithDefaultValues()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass {" +
                    "   [Xunit.Theory, Xunit.InlineData(\"abc\")]" +
                    "   public void TestMethod(string a, string b = \"default\", string c = null) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindErrorFor_UsingParametersWithDefaultValuesAndParamsArgument()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass {" +
                    "   [Xunit.Theory, Xunit.InlineData(\"abc\")]" +
                    "   public void TestMethod(string a, string b = \"default\", string c = null, params string[] d) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_UsingExplicitArray()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass {" +
                    "   [Xunit.Theory, Xunit.InlineData(new object[] {\"abc\", 1, null})]" +
                    "   public void TestMethod(string a, int b, object c) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_UsingExplicitNamedArray()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass {" +
                    "   [Xunit.Theory, Xunit.InlineData(data: new object[] {\"abc\", 1, null})]" +
                    "   public void TestMethod(string a, int b, object c) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_UsingImplicitArray()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass {" +
                    "   [Xunit.Theory, Xunit.InlineData(new [] {(object)\"abc\", 1, null})]" +
                    "   public void TestMethod(string a, int b, object c) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_UsingImplicitNamedArray()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass {" +
                    "   [Xunit.Theory, Xunit.InlineData(data: new [] {(object)\"abc\", 1, null})]" +
                    "   public void TestMethod(string a, int b, object c) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_EmptyArray()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass {" +
                    "   [Xunit.Theory, Xunit.InlineData(new byte[0])]" +
                    "   public void TestMethod(byte[] input) { }" +
                    "}");

                Assert.Empty(diagnostics);
            }
        }

        public class ForAttributeWithTooFewArguments : InlineDataMustMatchTheoryParametersTests
        {
            [Fact]
            public async void FindsError()
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

            [Theory]
            [InlineData("Xunit.InlineData()")]
            [InlineData("Xunit.InlineData")]
            public async void FindsError_ForAttributeWithNoArguments(string attribute)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass {" +
                    $"  [Xunit.Theory, {attribute}]" +
                    "  public void TestMethod(int a) { }" +
                    "}");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("InlineData values must match the number of method parameters", d.GetMessage());
                      Assert.Equal("xUnit1009", d.Descriptor.Id);
                      Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                  });
            }

            [Fact]
            public async void FindsError_UsingParams()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(1)] public void TestMethod(int a, int b, params string[] value) { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("InlineData values must match the number of method parameters", d.GetMessage());
                      Assert.Equal("xUnit1009", d.Descriptor.Id);
                      Assert.Equal(DiagnosticSeverity.Error, d.Severity);
                  });
            }
        }

        public class ForAttributeWithTooManyArguments : InlineDataMustMatchTheoryParametersTests
        {
            [Fact]
            public async void FindsError()
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
        }

        public class ForAttributeNullValue : InlineDataMustMatchTheoryParametersTests
        {
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

            [Theory]
            [InlineData("int")]
            [InlineData("params int[]")]
            public async void FindsWarning_ForSingleNullValue(string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(null)] public void TestMethod(" + type + " a) { } }");

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
            public async void FindsWarning_ForValueTypeParameter(string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, null, null)] public void TestMethod(int a, " + type + " b, params " + type + "[] c) { } }");

                Assert.Collection(diagnostics,
                   d =>
                   {
                       Assert.Equal("Null should not be used for value type parameter 'b' of type '" + type + "'.", d.GetMessage());
                       Assert.Equal("xUnit1012", d.Descriptor.Id);
                       Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                   },
                   d =>
                   {
                       Assert.Equal("Null should not be used for value type parameter 'c' of type '" + type + "'.", d.GetMessage());
                       Assert.Equal("xUnit1012", d.Descriptor.Id);
                       Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                   });
            }

            [Theory]
            [MemberData(nameof(ValueTypes))]
            public async void DoesNotFindWarning_ForNullableValueType(string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, null)] public void TestMethod(int a, " + type + "? b) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("object")]
            [InlineData("string")]
            [InlineData("System.Exception")]
            public async void DoesNotFindWarning_ForReferenceParameterType(string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(1, null)] public void TestMethod(int a, " + type + " b) { } }");

                Assert.Empty(diagnostics);
            }
        }

        public class ForConversionToNumericValue : InlineDataMustMatchTheoryParametersTests
        {
            public static readonly IEnumerable<Tuple<string>> NumericTypes = new[] { "int", "long", "short", "byte", "float", "double", "decimal", "uint", "ulong", "ushort", "sbyte", }.Select(t => Tuple.Create(t));

            public static IEnumerable<Tuple<string, string>> NumericValuesAndNumericTypes { get; } = from value in NumericValues
                                                                                                     from type in NumericTypes
                                                                                                     select Tuple.Create(value.Item1, type.Item1);

            public static IEnumerable<Tuple<string, string>> BoolValuesAndNumericTypes { get; } = from value in BoolValues
                                                                                                  from type in NumericTypes
                                                                                                  select Tuple.Create(value.Item1, type.Item1);

            [Theory]
            [TupleMemberData(nameof(NumericValuesAndNumericTypes))]
            public async void DoesNotFindError_FromAnyOtherNumericType(string value, string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(" + type + " a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(NumericValuesAndNumericTypes))]
            public async void DoesNotFindError_FromAnyOtherNumericType_ToNullable(string value, string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(" + type + "? a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(BoolValuesAndNumericTypes))]
            public async void FindsError_ForBoolArgument(string value, string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(" + type + " a) { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("The value is not convertible to the method parameter 'a' of type '" + type + "'.", d.GetMessage());
                      Assert.Equal("xUnit1010", d.Descriptor.Id);
                  });
            }

            [Theory]
            [TupleMemberData(nameof(NumericTypes))]
            public async void DoesNotFindError_ForCharArgument(string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData('a')] public void TestMethod(" + type + " a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(NumericTypes))]
            public async void FindsError_ForEnumArgument(string type)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(System.StringComparison.InvariantCulture)] public void TestMethod(" + type + " a) { } }");


                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("The value is not convertible to the method parameter 'a' of type '" + type + "'.", d.GetMessage());
                      Assert.Equal("xUnit1010", d.Descriptor.Id);
                  });
            }
        }

        public class ForConversionToBoolValue : InlineDataMustMatchTheoryParametersTests
        {
            [Theory]
            [TupleMemberData(nameof(BoolValues))]
            public async void DoesNotFindError_FromBoolType(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(bool a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(BoolValues))]
            public async void DoesNotFindError_FromBoolType_ToNullable(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(bool? a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(NumericValues))]
            [InlineData("System.StringComparison.Ordinal")]
            [InlineData("'a'")]
            [InlineData("\"abc\"")]
            [InlineData("typeof(string)")]
            public async void FindsError_ForOtherArguments(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(bool a) { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("The value is not convertible to the method parameter 'a' of type 'bool'.", d.GetMessage());
                      Assert.Equal("xUnit1010", d.Descriptor.Id);
                  });
            }
        }

        public class ForConversionToCharValue : InlineDataMustMatchTheoryParametersTests
        {
            [Theory]
            [InlineData("'a'")]
            [TupleMemberData(nameof(IntegerValues))]
            public async void DoesNotFindError_FromCharOrIntegerType(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(char a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("'a'")]
            [TupleMemberData(nameof(IntegerValues))]
            public async void DoesNotFindError_FromCharType_ToNullable(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(char? a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(FloatingPointValues))]
            [InlineData("System.StringComparison.Ordinal")]
            [TupleMemberData(nameof(BoolValues))]
            [InlineData("\"abc\"")]
            [InlineData("typeof(string)")]
            public async void FindsError_ForOtherArguments(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(char a) { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("The value is not convertible to the method parameter 'a' of type 'char'.", d.GetMessage());
                      Assert.Equal("xUnit1010", d.Descriptor.Id);
                  });
            }
        }

        public class ForConversionToEnum : InlineDataMustMatchTheoryParametersTests
        {
            [Fact]
            public async void DoesNotFindError_FromEnum()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(System.StringComparison.Ordinal)] public void TestMethod(System.StringComparison a) { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_FromEnum_ToNullable()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(System.StringComparison.Ordinal)] public void TestMethod(System.StringComparison? a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(NumericValues))]
            [TupleMemberData(nameof(BoolValues))]
            [InlineData("'a'")]
            [InlineData("\"abc\"")]
            [InlineData("typeof(string)")]
            public async void FindsError_ForOtherArguments(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.StringComparison a) { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("The value is not convertible to the method parameter 'a' of type 'System.StringComparison'.", d.GetMessage());
                      Assert.Equal("xUnit1010", d.Descriptor.Id);
                  });
            }
        }

        public class ForConversionToType : InlineDataMustMatchTheoryParametersTests
        {
            [Theory]
            [InlineData("typeof(string)")]
            [InlineData("null")]
            public async void DoesNotFindError_FromType(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.Type a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("typeof(string)")]
            [InlineData("null")]
            public async void DoesNotFindError_FromType_UsingParams(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(params System.Type[] a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(NumericValues))]
            [TupleMemberData(nameof(BoolValues))]
            [InlineData("'a'")]
            [InlineData("\"abc\"")]
            [InlineData("System.StringComparison.Ordinal")]
            public async void FindsError_ForOtherArguments(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.Type a) { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("The value is not convertible to the method parameter 'a' of type 'System.Type'.", d.GetMessage());
                      Assert.Equal("xUnit1010", d.Descriptor.Id);
                  });
            }

            [Theory]
            [TupleMemberData(nameof(NumericValues))]
            [TupleMemberData(nameof(BoolValues))]
            [InlineData("'a'")]
            [InlineData("\"abc\"")]
            [InlineData("System.StringComparison.Ordinal")]
            public async void FindsError_ForOtherArguments_UsingParams(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(params System.Type[] a) { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("The value is not convertible to the method parameter 'a' of type 'System.Type'.", d.GetMessage());
                      Assert.Equal("xUnit1010", d.Descriptor.Id);
                  });
            }
        }

        public class ForConversionToString : InlineDataMustMatchTheoryParametersTests
        {
            [Theory]
            [InlineData("\"abc\"")]
            [InlineData("null")]
            public async void DoesNotFindError_FromBoolType(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(string a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(NumericValues))]
            [TupleMemberData(nameof(BoolValues))]
            [InlineData("System.StringComparison.Ordinal")]
            [InlineData("'a'")]
            [InlineData("typeof(string)")]
            public async void FindsError_ForOtherArguments(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(string a) { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("The value is not convertible to the method parameter 'a' of type 'string'.", d.GetMessage());
                      Assert.Equal("xUnit1010", d.Descriptor.Id);
                  });
            }
        }

        public class ForConversionToInterface : InlineDataMustMatchTheoryParametersTests
        {
            [Theory]
            [TupleMemberData(nameof(NumericValues))]
            [InlineData("System.StringComparison.Ordinal")]
            [InlineData("null")]
            public async void DoesNotFindError_FromTypesImplementingInterface(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.IFormattable a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(BoolValues))]
            [InlineData("'a'")]
            [InlineData("\"abc\"")]
            [InlineData("typeof(string)")]
            public async void FindsError_ForOtherArguments(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(System.IFormattable a) { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("The value is not convertible to the method parameter 'a' of type 'System.IFormattable'.", d.GetMessage());
                      Assert.Equal("xUnit1010", d.Descriptor.Id);
                  });
            }
        }

        public class ForConversionToObject : InlineDataMustMatchTheoryParametersTests
        {
            [Theory]
            [TupleMemberData(nameof(BoolValues))]
            [TupleMemberData(nameof(NumericValues))]
            [InlineData("System.StringComparison.Ordinal")]
            [InlineData("'a'")]
            [InlineData("\"abc\"")]
            [InlineData("null")]
            [InlineData("typeof(string)")]
            public async void DoesNotFindError_FromAnyValue(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(object a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(BoolValues))]
            [TupleMemberData(nameof(NumericValues))]
            [InlineData("System.StringComparison.Ordinal")]
            [InlineData("'a'")]
            [InlineData("\"abc\"")]
            [InlineData("null")]
            [InlineData("typeof(string)")]
            public async void DoesNotFindError_FromAnyValues_UsingParams(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                   "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod(params object[] a) { } }");

                Assert.Empty(diagnostics);
            }
        }

        public class ForConversionToGeneric : InlineDataMustMatchTheoryParametersTests
        {
            [Theory]
            [TupleMemberData(nameof(BoolValues))]
            [TupleMemberData(nameof(NumericValues))]
            [InlineData("System.StringComparison.Ordinal")]
            [InlineData("'a'")]
            [InlineData("\"abc\"")]
            [InlineData("null")]
            [InlineData("typeof(string)")]
            public async void DoesNotFindError_FromAnyValue_WithNoConstraint(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(BoolValues))]
            [TupleMemberData(nameof(NumericValues))]
            [InlineData("System.StringComparison.Ordinal")]
            [InlineData("'a'")]
            public async void DoesNotFindError_FromAnyValueType_WithStructConstraint(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) where T: struct { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("\"abc\"")]
            [InlineData("typeof(string)")]
            public async void FindsError_FromAnyReferenceType_WithStructConstraint(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) where T: struct { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("The value is not convertible to the method parameter 'a' of type 'T'.", d.GetMessage());
                      Assert.Equal("xUnit1010", d.Descriptor.Id);
                  });
            }

            [Theory]
            [InlineData("\"abc\"")]
            [InlineData("typeof(string)")]
            [InlineData("null")]
            public async void DoesNotFindError_FromAnyReferenceType_WithClassConstraint(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) where T: class { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(BoolValues))]
            [TupleMemberData(nameof(NumericValues))]
            [InlineData("System.StringComparison.Ordinal")]
            [InlineData("'a'")]
            public async void FindsError_FromAnyValueType_WithClassConstraint(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) where T: class { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("The value is not convertible to the method parameter 'a' of type 'T'.", d.GetMessage());
                      Assert.Equal("xUnit1010", d.Descriptor.Id);
                  });
            }

            [Theory]
            [InlineData("null")]
            [InlineData("System.StringComparison.Ordinal")]
            [TupleMemberData(nameof(NumericValues))]
            public async void DoesNotFindError_FromAnyMatchingType_WithTypeConstraint(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) where T: System.IConvertible, System.IFormattable { } }");

                Assert.Empty(diagnostics);
            }

            [Theory]
            [InlineData("typeof(string)")]
            [InlineData("new int[] { 1, 2, 3 }")]
            [InlineData("'a'")]
            [InlineData("\"abc\"")]
            [TupleMemberData(nameof(BoolValues))]
            public async void FindsError_FromNonMatchingType_WithTypeConstraint(string value)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    "public class TestClass { [Xunit.Theory, Xunit.InlineData(" + value + ")] public void TestMethod<T>(T a) where T: System.IConvertible, System.IFormattable { } }");

                Assert.Collection(diagnostics,
                  d =>
                  {
                      Assert.Equal("The value is not convertible to the method parameter 'a' of type 'T'.", d.GetMessage());
                      Assert.Equal("xUnit1010", d.Descriptor.Id);
                  });
            }
        }

        public class ForConversionToDateTimeAndDateTimeOffset : InlineDataMustMatchTheoryParametersTests
        {
            public static readonly string[] AcceptableDateTimeArguments =
            {
                "\"\"",
                "\"2018-01-02\"",
                "\"2018-01-02 12:34\"",
                "\"obviously-rubbish-datetime-value\"",
                "MyConstString"
            };
            
            // combinations of (value type value, date time like types)
            public static IEnumerable<Tuple<string, string>> ValueTypedArgumentsCombinedWithDateTimeLikeTypes { get; } =
                ValueTypedValues.SelectMany(v => 
                    new string[] { "System.DateTime", "System.DateTimeOffset" }.Select(
                        dateTimeLikeType => Tuple.Create(v.Item1, dateTimeLikeType)));

            public static IEnumerable<Tuple<string, string>> DateTimeValueStringsCombinedWithDateTimeType { get; } =
                AcceptableDateTimeArguments.Select(v => Tuple.Create(v, "System.DateTime"));

            public static IEnumerable<Tuple<string, string>> DateTimeValueStringsCombinedWithDateTimeOffsetType { get; } =
                AcceptableDateTimeArguments.Select(v => Tuple.Create(v, "System.DateTimeOffset"));

            [Theory]
            [TupleMemberData(nameof(ValueTypedArgumentsCombinedWithDateTimeLikeTypes))]
            [InlineData("MyConstInt", "System.DateTime")]
            [InlineData("MyConstInt", "System.DateTimeOffset")]
            public async void FindsError_FromNonString(string inlineData, string parameterType)
            {
                var source = @"
public class TestClass 
{
    private const int MyConstInt = 1;
    
    [Xunit.Theory, Xunit.InlineData(" + inlineData + @")]
    public void TestMethod(" + parameterType + @" parameter) 
    {
    } 
}";
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("The value is not convertible to the method parameter 'parameter' " +
                                     $"of type '{parameterType}'.", d.GetMessage());
                        Assert.Equal("xUnit1010", d.Descriptor.Id);
                    });
            }

            [Theory]
            [TupleMemberData(nameof(DateTimeValueStringsCombinedWithDateTimeType))]
            public async void DoesNotFindError_ForDateTime_FromString(string inlineData, string parameterType)
            {
                var source = CreateSourceWithStringConst(inlineData, parameterType);
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);

                Assert.Empty(diagnostics);
            }

            [Theory]
            [TupleMemberData(nameof(DateTimeValueStringsCombinedWithDateTimeOffsetType))]
            public async void FindsError_ForDateTimeOffsetAndAnalyzerLessThan_2_4_0v_FromString(string inlineData, string parameterType)
            {
                var source = CreateSourceWithStringConst(inlineData, parameterType);
                var diagnosticAnalyzer = new InlineDataMustMatchTheoryParameters("2.3.1");
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(diagnosticAnalyzer, source);

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("The value is not convertible to the method parameter 'parameter' " +
                                     $"of type '{parameterType}'.", d.GetMessage());
                        Assert.Equal("xUnit1010", d.Descriptor.Id);
                    });
            }

            [Theory]
            [TupleMemberData(nameof(DateTimeValueStringsCombinedWithDateTimeOffsetType))]
            public async void DoesNotFindError_ForDateTimeOffsetAndForAnalyzerGreaterThanEqual_2_4_0v_FromString(string inlineData, string parameterType)
            {
                var source = CreateSourceWithStringConst(inlineData, parameterType);
                var diagnosticAnalyzer = new InlineDataMustMatchTheoryParameters("2.4.0");
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(diagnosticAnalyzer, source);

                Assert.Empty(diagnostics);
            }

            private static string CreateSourceWithStringConst(string inlineData, string parameterType)
            {
                return @"
public class TestClass 
{
    private const string MyConstString = ""some string"";
    
    [Xunit.Theory, Xunit.InlineData(" + inlineData + @")]
    public void TestMethod(" + parameterType + @" parameter) 
    {
    } 
}";
            }
        }

        public class ForConversionToGuid : InlineDataMustMatchTheoryParametersTests
        {
            public static IEnumerable<Tuple<string>> AcceptableGuidStrings { get; } =
                new[]
                {
                    "\"\"",
                    "\"{5B21E154-15EB-4B1E-BC30-127E8A41ECA1}\"",
                    "\"4EBCD32C-A2B8-4600-9E72-3873347E285C\"",
                    "\"39A3B4C85FEF43A988EB4BB4AC4D4103\"",
                    "\"obviously-rubbish-guid-value\""
                }.Select(x => Tuple.Create(x)).ToArray();

            [Theory]
            [TupleMemberData(nameof(ValueTypedValues))]
            [InlineData("MyConstInt")]
            public async void FindsError_FromNonString(string inlineData)
            {
                var source = @"
public class TestClass 
{
    private const int MyConstInt = 1;
    
    [Xunit.Theory, Xunit.InlineData(" + inlineData + @")]
    public void TestMethod(System.Guid parameter) 
    {
    } 
}";
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("The value is not convertible to the method parameter 'parameter' " +
                                     "of type 'System.Guid'.", d.GetMessage());
                        Assert.Equal("xUnit1010", d.Descriptor.Id);
                    });
            }

            [Theory]
            [TupleMemberData(nameof(AcceptableGuidStrings))]
            public async void FindsError__ForAnalyzerLessThan_2_4_0v_FromString(string inlineData)
            {
                var source = CreateSource(inlineData);

                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, source);

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("The value is not convertible to the method parameter 'parameter' " +
                                     "of type 'System.Guid'.", d.GetMessage());
                        Assert.Equal("xUnit1010", d.Descriptor.Id);
                    });
            }

            [Theory]
            [TupleMemberData(nameof(AcceptableGuidStrings))]
            public async void DoesNotFindError_ForAnalyzerGreaterThanEqual_2_4_0v_FromString(string inlineData)
            {
                var source = CreateSource(inlineData);

                var diagnosticAnalyzer = new InlineDataMustMatchTheoryParameters("2.5.0");
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(diagnosticAnalyzer, source);

                Assert.Empty(diagnostics);
            }

            private static string CreateSource(string inlineData)
            {
                return @"
public class TestClass 
{
    [Xunit.Theory, Xunit.InlineData(" + inlineData + @")]
    public void TestMethod(System.Guid parameter) 
    {
    } 
}";
            }
        }

        // Note: decimal literal 42M is not valid as an attribute argument
        public static IEnumerable<Tuple<string>> IntegerValues { get; } = new[] { "42", "42L", "42u", "42ul", "(short)42", "(byte)42", "(ushort)42", "(sbyte)42", }.Select(v => Tuple.Create(v)).ToArray();

        public static IEnumerable<Tuple<string>> FloatingPointValues { get; } = new[] { "42f", "42d" }.Select(v => Tuple.Create(v)).ToArray();

        public static IEnumerable<Tuple<string>> NumericValues { get; } = IntegerValues.Concat(FloatingPointValues).ToArray();

        public static IEnumerable<Tuple<string>> BoolValues { get; } = new[] { "true", "false" }.Select(v => Tuple.Create(v)).ToArray();

        public static IEnumerable<Tuple<string>> ValueTypedValues { get; } = NumericValues.Concat(BoolValues).Concat(new [] {"typeof(int)"}.Select(v => Tuple.Create(v)));
    }
}
