using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class TestMethodShouldNotHaveReturnTypeTests
    {
        private static DiagnosticAnalyzer Analyzer { get; } = new TestMethodShouldNotHaveReturnType();

        public static TheoryData<string> TestMethodAttributes_Data { get; } = new TheoryData<string>
        {
            "[Xunit.Fact]",
            "[Xunit.Theory]"
        };

        public static TheoryData<string, string> TestMethodAttributes_Modifiers_Data { get; } = new TheoryData<string, string>
        {
            { "[Xunit.Fact]", "async" },
            { "[Xunit.Fact]", "" },
            { "[Xunit.Theory]", "async" },
            { "[Xunit.Theory]", "" }
        };

        private static void CheckDiagnostics(IEnumerable<Diagnostic> diagnostics, params (string method, string @class)[] messageArgs)
        {
            var array = diagnostics.ToArray();
            Assert.Equal(messageArgs.Length, array.Length);

            for (int i = 0; i < array.Length; i++)
            {
                var d = array[i];
                var (method, @class) = messageArgs[i];

                Assert.Equal($"Test method '{method}' on test class '{@class}' should not have a return type.", d.GetMessage());
                Assert.Equal("xUnit1027", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            }
        }

        [Theory]
        [MemberData(nameof(TestMethodAttributes_Data))]
        public async void Warns_NotTaskOrVoid(string attribute)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, $@"
class TestClass
{{
    {attribute}
    public int TestMethod() {{ throw null; }}
}}");

            CheckDiagnostics(diagnostics,
                (method: "TestMethod", @class: "TestClass"));
        }

        [Theory]
        [MemberData(nameof(TestMethodAttributes_Modifiers_Data))]
        public async void Warns_DerivedFromTask(string attribute, string modifier)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, $@"
using System.Threading.Tasks;

class TestClass
{{
#pragma warning disable 1998
    {attribute}
    public {modifier} Task<int> TestMethod() {{ throw null; }}
#pragma warning restore 1998
}}");

            CheckDiagnostics(diagnostics,
                (method: "TestMethod", @class: "TestClass"));
        }

        [Theory]
        [MemberData(nameof(TestMethodAttributes_Modifiers_Data))]
        public async void DoesNotWarn_Task(string attribute, string modifier)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, $@"
using System.Threading.Tasks;

class TestClass
{{
#pragma warning disable 1998
    {attribute}
    public {modifier} Task TestMethod() {{ throw null; }}
#pragma warning restore 1998
}}");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(TestMethodAttributes_Modifiers_Data))]
        public async void DoesNotWarn_Void(string attribute, string modifier)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, $@"
class TestClass
{{
#pragma warning disable 1998
    {attribute}
    public {modifier} void TestMethod() {{ }}
#pragma warning restore 1998
}}");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async void DoesNotWarn_NotTestMethod()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(Analyzer, @"
class NotTestClass
{
    public int NotTestMethod() { throw null; }
}");

            Assert.Empty(diagnostics);
        }
    }
}
