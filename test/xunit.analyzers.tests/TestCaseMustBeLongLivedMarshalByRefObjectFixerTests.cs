using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TestCaseMustBeLongLivedMarshalByRefObjectFixerTests
    {
        readonly DiagnosticAnalyzer analyzer = new TestCaseMustBeLongLivedMarshalByRefObject();
        readonly CodeFixProvider fixer = new TestCaseMustBeLongLivedMarshalByRefObjectFixer();

        [Fact]
        public async void WithNoBaseClass_AddsBaseClass()
        {
            var code = "public class MyTestCase : Xunit.Abstractions.ITestCase { }";

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, CompilationReporting.IgnoreErrors, code);

            Assert.Equal("public class MyTestCase : Xunit.LongLivedMarshalByRefObject, Xunit.Abstractions.ITestCase { }", result);
        }

        [Fact]
        public async void WithBadBaseClass_ReplacesBaseClass()
        {
            var code = "public class Foo { } public class MyTestCase : Foo, Xunit.Abstractions.ITestCase { }";

            var result = await CodeAnalyzerHelper.GetFixedCodeAsync(analyzer, fixer, CompilationReporting.IgnoreErrors, code);

            Assert.Equal("public class Foo { } public class MyTestCase : Xunit.LongLivedMarshalByRefObject, Xunit.Abstractions.ITestCase { }", result);
        }
    }
}
