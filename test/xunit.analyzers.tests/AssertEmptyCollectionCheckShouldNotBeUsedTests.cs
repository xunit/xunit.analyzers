using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertEmptyCollectionCheckShouldNotBeUsedTests
    {
        public class Analyzer
        {
            private readonly DiagnosticAnalyzer analyzer = new AssertEmptyCollectionCheckShouldNotBeUsed();

            public static TheoryData<string> Collections { get; } = new TheoryData<string>
            {
                "new int[0]",
                "new System.Collections.Generic.List<int>()",
                "new System.Collections.Generic.HashSet<int>()",
                "new System.Collections.ObjectModel.Collection<int>()",
                "System.Linq.Enumerable.Empty<int>()",
            };

            [Theory]
            [MemberData(nameof(Collections))]
            public async void FindsWarningForCollectionCheckWithoutAction(string collection)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() { 
    Xunit.Assert.Collection(" + collection + @");
} }");

                Assert.Collection(diagnostics, d =>
                {
                    Assert.Equal("Do not use Assert.Collection() to check for empty collections.", d.GetMessage());
                    Assert.Equal("xUnit2011", d.Id);
                    Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
                });
            }

            [Theory]
            [MemberData(nameof(Collections))]
            public async void DoesNotFindWarningForCollectionCheckWithAction(string collection)
            {
                var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                    @"class TestClass { void TestMethod() { 
    Xunit.Assert.Collection(" + collection + @", i => Xunit.Assert.True(true));
} }");
                Assert.Empty(diagnostics);
            }
        }
    }
}