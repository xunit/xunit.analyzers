using System.Collections.Generic;
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
            "System.Collections.Immutable.ImmutableArray.Create<int>().Length",
            "new System.Collections.ObjectModel.Collection<int>().Count",
            "new System.Collections.Generic.List<int>().AsReadOnly().Count",
            "System.Linq.Enumerable.Empty<int>().Count()",
        };

        public static TheoryData<string, int> CollectionsWithUnsupportedSize { get; } = new TheoryData<string, int>
        {
            { "new int[0].Length", -1 },
            { "new System.Collections.ArrayList().Count", -2 },
            { "new System.Collections.Generic.List<int>().Count", 2 },
            { "new System.Collections.Generic.HashSet<int>().Count", 3 },
            { "System.Collections.Immutable.ImmutableArray.Create<int>().Length", 42 },
            { "new System.Collections.ObjectModel.Collection<int>().Count", 13 },
            { "new System.Collections.Generic.List<int>().AsReadOnly().Count", 2 },
            { "System.Linq.Enumerable.Empty<int>().Count()", 354 },
        };

        private static void CheckDiagnostics(IEnumerable<Diagnostic> diagnostics, string method)
        {
            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal($"Do not use Assert.{method}() to check for collection size.", d.GetMessage());
                Assert.Equal("xUnit2013", d.Id);
                Assert.Equal(DiagnosticSeverity.Warning, d.Severity);
            });
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async void FindsWarningForEmptyCollectionSizeCheck(string collection)
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.Equal(0, " + collection + @");
} }");

            CheckDiagnostics(diagnostics, "Equal");
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

            CheckDiagnostics(diagnostics, "NotEqual");
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

            CheckDiagnostics(diagnostics, "Equal");
        }

        [Fact]
        public async void FindsWarningForSymbolDeclaringTypeHasZeroArity_ImplementsICollectionOfT()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, @"
using System.Collections;
using System.Collections.Generic;
using Xunit;

class IntCollection : ICollection<int>
{
    public int Count { get { throw null; } }
    public bool IsReadOnly { get { throw null; } }
    public void Add(int item) { throw null; }
    public void Clear() { throw null; }
    public bool Contains(int item) { throw null; }
    public void CopyTo(int[] array, int arrayIndex) { throw null; }
    public IEnumerator<int> GetEnumerator() { throw null; }
    public bool Remove(int item) { throw null; }
    IEnumerator IEnumerable.GetEnumerator() { throw null; }
}

class TestClass
{
    void TestMethod()
    {
        Assert.Equal(1, new IntCollection().Count);
    }
}");

            CheckDiagnostics(diagnostics, "Equal");
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

        [Fact]
        public async void DoesNotCrashForSymbolDeclaringTypeHasDifferentArityThanICollection_Zero()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Collections.Generic;
        interface IIntCollection : ICollection<int> {
            new int Count { get; }
        }
        class TestClass { void TestMethod() {
            Xunit.Assert.Equal(1, ((IIntCollection)null).Count);
        } }");

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async void DoesNotCrashForSymbolDeclaringTypeHasDifferentArityThanICollection_Two()
        {
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer,
                @"using System.Collections.Generic;
        interface IDictionary2<K, V> : ICollection<KeyValuePair<K, V>> {
            new int Count { get; }
        }
        class TestClass { void TestMethod() {
            Xunit.Assert.Equal(1, ((IDictionary2<int, int>)null).Count);
        } }");

            Assert.Empty(diagnostics);
        }
    }
}
