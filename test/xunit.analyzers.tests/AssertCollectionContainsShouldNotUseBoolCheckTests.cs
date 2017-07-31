using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertCollectionContainsShouldNotUseBoolCheckTests
    {
        private readonly DiagnosticAnalyzer analyzer = new AssertCollectionContainsShouldNotUseBoolCheck();

        public static TheoryData<string> Collections { get; } = new TheoryData<string>
        {
            "new System.Collections.Generic.List<int>()",
            "new System.Collections.Generic.HashSet<int>()",
            "new System.Collections.ObjectModel.Collection<int>()"
        };

        public static TheoryData<string> Enumerables { get; } = new TheoryData<string>
        {
            "new int[0]",
            "System.Linq.Enumerable.Empty<int>()"
        };

        [Theory]
        [MemberData(nameof(Collections))]
        public async void FindsWarningForTrueCollectionContainsCheck(string collection)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() { 
    Xunit.Assert.True(" + collection + @".Contains(1));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Contains() to check if a value exists in a collection.", d.GetMessage());
                Assert.Equal("xUnit2017", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async void FindsWarningForFalseCollectionContainsCheck(string collection)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() { 
    Xunit.Assert.False(" + collection + @".Contains(1));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Contains() to check if a value exists in a collection.", d.GetMessage());
                Assert.Equal("xUnit2017", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Enumerables))]
        public async void FindsWarningForTrueLinqContainsCheck(string enumerable)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.True(" + enumerable + @".Contains(1));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Contains() to check if a value exists in a collection.", d.GetMessage());
                Assert.Equal("xUnit2017", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Enumerables))]
        public async void FindsWarningForTrueLinqContainsCheckWithEqualityComparer(string enumerable)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.True(" + enumerable + @".Contains(1, System.Collections.Generic.EqualityComparer<int>.Default));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Contains() to check if a value exists in a collection.", d.GetMessage());
                Assert.Equal("xUnit2017", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Enumerables))]
        public async void FindsWarningForFalseLinqContainsCheck(string enumerable)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.False(" + enumerable + @".Contains(1));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Contains() to check if a value exists in a collection.", d.GetMessage());
                Assert.Equal("xUnit2017", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Enumerables))]
        public async void FindsWarningForFalseLinqContainsCheckWithEqualityComparer(string enumerable)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.False(" + enumerable + @".Contains(1, System.Collections.Generic.EqualityComparer<int>.Default));
} }");

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Contains() to check if a value exists in a collection.", d.GetMessage());
                Assert.Equal("xUnit2017", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async void DoesNotFindWarningForTrueCollectionContainsCheckWithAssertionMessage(string collection)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() { 
    Xunit.Assert.True(" + collection + @".Contains(1), ""Custom message"");
} }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async void DoesNotFindWarningForFalseCollectionContainsCheckWithAssertionMessage(string collection)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"class TestClass { void TestMethod() { 
    Xunit.Assert.False(" + collection + @".Contains(1), ""Custom message"");
} }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Enumerables))]
        public async void DoesNotFindWarningForTrueLinqContainsCheckWithAssertionMessage(string enumerable)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.True(" + enumerable + @".Contains(1), ""Custom message"");
} }");

            Assert.Empty(diagnostics);
        }

        [Theory]
        [MemberData(nameof(Enumerables))]
        public async void DoesNotFindWarningForFalseLinqContainsCheckWithAssertionMessage(string enumerable)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.False(" + enumerable + @".Contains(1), ""Custom message"");
} }");

            Assert.Empty(diagnostics);
        }
    }
}