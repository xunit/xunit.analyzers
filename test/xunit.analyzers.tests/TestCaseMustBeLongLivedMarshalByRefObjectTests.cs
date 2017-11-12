using System.Linq;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TestCaseMustBeLongLivedMarshalByRefObjectTests
    {
        readonly DiagnosticAnalyzer analyzer = new TestCaseMustBeLongLivedMarshalByRefObject();

        readonly static string Template = @"
public class Foo {{ }}
public class MyLLMBRO: Xunit.LongLivedMarshalByRefObject {{ }}
public class MyTestCase: {0} {{ }}
";

        public static TheoryData<string> Interfaces = new TheoryData<string> { "Xunit.Abstractions.ITestCase", "Xunit.Sdk.IXunitTestCase" };

        public static TheoryData<string, string> InterfacesWithBaseClasses
        {
            get
            {
                var result = new TheoryData<string, string>();

                foreach (var @interface in Interfaces.Select(x => (string)x[0]))
                {
                    result.Add(@interface, "MyLLMBRO");
                    result.Add(@interface, "Xunit.LongLivedMarshalByRefObject");
                }

                return result;
            }
        }

        [Fact]
        public async void NonTestCase_NoDiagnostics()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class Foo { }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async void XunitTestCase_NoDiagnostics()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, "public class MyTestCase : Xunit.Sdk.XunitTestCase { }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(InterfacesWithBaseClasses))]
        public async void InterfaceWithProperBaseClass_NoDiagnostics(string @interface, string baseClass)
        {
            var code = string.Format(Template, $"{baseClass}, {@interface}");

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, CompilationReporting.IgnoreErrors, code);

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Interfaces))]
        public async void InterfaceWithoutBaseClass_ReturnsError(string @interface)
        {
            var code = string.Format(Template, @interface);

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, CompilationReporting.IgnoreErrors, code);

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Test case class MyTestCase must derive directly or indirectly from Xunit.LongLivedMarshalByRefObject", d.GetMessage());
                    Assert.Equal("xUnit3000", d.Descriptor.Id);
                }
            );
        }

        [Theory]
        [MemberData(nameof(Interfaces))]
        public async void InterfaceWithBadBaseClass_ReturnsError(string @interface)
        {
            var code = string.Format(Template, $"Foo, {@interface}");

            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, CompilationReporting.IgnoreErrors, code);

            Assert.Collection(diagnostics,
                d =>
                {
                    Assert.Equal("Test case class MyTestCase must derive directly or indirectly from Xunit.LongLivedMarshalByRefObject", d.GetMessage());
                    Assert.Equal("xUnit3000", d.Descriptor.Id);
                }
            );
        }
    }
}
