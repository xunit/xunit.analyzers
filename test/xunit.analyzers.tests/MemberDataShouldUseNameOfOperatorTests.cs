using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class MemberDataShouldUseNameOfOperatorTests
    {
        public class Analyzer
        {
            readonly DiagnosticAnalyzer analyzer = new MemberDataShouldUseNameOfOperator();

            private const string SharedCode = @"public partial class TestClass { public static System.Collections.Generic.IEnumerable<object[]> Data { get;set; } }
public class OtherClass { public static System.Collections.Generic.IEnumerable<object[]> OtherData { get;set; } }
";

            [Fact]
            public async void DoesNotFindError_ForNameofOnSameClass()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    SharedCode,
                    "public partial class TestClass { [Xunit.MemberData(nameof(Data))] public void TestMethod() { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_ForNameofOnOtherClass()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    SharedCode,
                    "public partial class TestClass { [Xunit.MemberData(nameof(OtherClass.OtherData), MemberType = typeof(OtherClass))] public void TestMethod() { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_ForInvalidStringReferenceOnSameClass()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    SharedCode,
                    "public partial class TestClass { [Xunit.MemberData(\"Typo\")] public void TestMethod() { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void DoesNotFindError_ForInvalidStringReferenceOnOtherClass()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    SharedCode,
                    "public partial class TestClass { [Xunit.MemberData(\"Typo\", MemberType = typeof(OtherClass))] public void TestMethod() { } }");

                Assert.Empty(diagnostics);
            }

            [Fact]
            public async void FindsError_ForStringReferenceOnSameClass()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    SharedCode,
                    "public partial class TestClass { [Xunit.MemberData(\"Data\")] public void TestMethod() { } }");

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("MemberData should use nameof operator to reference member 'Data' on type 'TestClass'.", d.GetMessage());
                        Assert.Equal("xUnit1014", d.Descriptor.Id);
                    });
            }

            [Fact]
            public async void FindsError_ForStringReferenceOnOtherClass()
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    SharedCode,
                    "public partial class TestClass { [Xunit.MemberData(\"OtherData\", MemberType = typeof(OtherClass))] public void TestMethod() { } }");

                Assert.Collection(diagnostics,
                    d =>
                    {
                        Assert.Equal("MemberData should use nameof operator to reference member 'OtherData' on type 'OtherClass'.", d.GetMessage());
                        Assert.Equal("xUnit1014", d.Descriptor.Id);
                    });
            }
        }
    }
}

