using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertEqualShouldNotBeUsedForCollectionSizeCheckTests
    {
        private readonly DiagnosticAnalyzer analyzer = new AssertEqualShouldNotBeUsedForCollectionSizeCheck();

        public static TheoryData<string> Collections { get; } = new TheoryData<string>
        {
            "new int[0].Length",
            "new System.Collections.ArrayList().Count",
            "new System.Collections.Generic.List<int>().Count",
            "new System.Collections.Generic.HashSet<int>().Count",
            "new System.Collections.ObjectModel.Collection<int>().Count()",
            "System.Linq.Enumerable.Empty<int>().Count()",
            "System.Collections.Immutable.ImmutableArray.Create<int>().Length",
        };

        public static TheoryData<string, int> CollectionsWithUnsupportedSize { get; } = new TheoryData<string, int>
        {
            { "new int[0].Length", -1 },
            { "new System.Collections.ArrayList().Count", -2 },
            { "new System.Collections.Generic.List<int>().Count", 2 },
            { "new System.Collections.Generic.HashSet<int>().Count", 3 },
            { "new System.Collections.ObjectModel.Collection<int>().Count()", 13 },
            { "System.Linq.Enumerable.Empty<int>().Count()", 354 },
            { "System.Collections.Immutable.ImmutableArray.Create<int>().Length", 42 },
        };

        [Theory]
        [MemberData(nameof(Collections))]
        public async void FindsWarningForEmptyCollectionSizeCheck(string collection)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.Equal(0, " + collection + @");
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Assert.Equal() to check for collection size.", d.GetMessage());
                Assert.Equal("xUnit2013", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async void FindsWarningForNonEmptyCollectionSizeCheck(string collection)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
        class TestClass { void TestMethod() { 
            Xunit.Assert.NotEqual(0, " + collection + @");
        } }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Assert.NotEqual() to check for collection size.", d.GetMessage());
                Assert.Equal("xUnit2013", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async void FindsWarningForSingleItemCollectionSizeCheck(string collection)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
        class TestClass { void TestMethod() { 
            Xunit.Assert.Equal(1, " + collection + @");
        } }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Assert.Equal() to check for collection size.", d.GetMessage());
                Assert.Equal("xUnit2013", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async void DoesNotFindWarningForNonSingleItemCollectionSizeCheck(string collection)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
        class TestClass { void TestMethod() { 
            Xunit.Assert.NotEqual(1, " + collection + @");
        } }");
            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(CollectionsWithUnsupportedSize))]
        public async void DoesNotFindWarningForUnsupportedCollectionSizeCheck(string collection, int size)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
        class TestClass { void TestMethod() { 
            Xunit.Assert.Equal(" + size + ", " + collection + @");
        } }");
            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(CollectionsWithUnsupportedSize))]
        public async void DoesNotFindWarningForUnsupportedNonEqualCollectionSizeCheck(string collection, int size)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
        class TestClass { void TestMethod() { 
            Xunit.Assert.NotEqual(" + size + ", " + collection + @");
        } }");
            Assert.Empty(diagnostics);
        }
    }
}