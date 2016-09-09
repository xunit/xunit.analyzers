using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class ClassDataAttributeMustPointAtValidClassTests
    {
        public class Analyzer
        {
            private static readonly string TestMethodSource = "public class TestClass { [Xunit.Theory][Xunit.ClassData(typeof(DataClass))] public void TestMethod() { } }";
            readonly DiagnosticAnalyzer analyzer = new ClassDataAttributeMustPointAtValidClass();

            [Fact]
            public async void DoesNotFindErrorForFactMethod()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, TestMethodSource,
@"class DataClass : System.Collections.Generic.IEnumerable<object[]> {
    public System.Collections.Generic.IEnumerator<object[]> GetEnumerator() => null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
}");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void FindsErrorForDataClassNotImplementingInterface()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, TestMethodSource,
@"class DataClass : System.Collections.Generic.IEnumerable<object> {
    public System.Collections.Generic.IEnumerator<object> GetEnumerator() => null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
}");

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("ClassData must point at a valid class", d.GetMessage());
                        Assert.Equal("xUnit1007", d.Descriptor.Id);
                    });
            }

            [Fact]
            public async void FindsErrorForAbstractDataClass()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, TestMethodSource,
@"abstract class DataClass : System.Collections.Generic.IEnumerable<object[]> {
    public DataClass() {}
    public System.Collections.Generic.IEnumerator<object[]> GetEnumerator() => null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
}");

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("ClassData must point at a valid class", d.GetMessage());
                        Assert.Equal("xUnit1007", d.Descriptor.Id);
                    });
            }

            [Fact]
            public async void FindsErrorForDataClassWithImplicitPrivateConstructor()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, TestMethodSource,
@"class DataClass : System.Collections.Generic.IEnumerable<object[]> {
    public DataClass(string parameter) {}
    public System.Collections.Generic.IEnumerator<object[]> GetEnumerator() => null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
}");

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("ClassData must point at a valid class", d.GetMessage());
                        Assert.Equal("xUnit1007", d.Descriptor.Id);
                    });
            }

            [Theory]
            [InlineData("private")]
            [InlineData("internal")]
            public async void FindsErrorForDataClassWithExplicitNonPublicConstructor(string accessiblity)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, TestMethodSource,
string.Format(@"class DataClass : System.Collections.Generic.IEnumerable<object[]> {{
    {0} DataClass() {{}}
    public System.Collections.Generic.IEnumerator<object[]> GetEnumerator() => null;
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => null;
}}", accessiblity));

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("ClassData must point at a valid class", d.GetMessage());
                        Assert.Equal("xUnit1007", d.Descriptor.Id);
                    });
            }
        }
    }
}

