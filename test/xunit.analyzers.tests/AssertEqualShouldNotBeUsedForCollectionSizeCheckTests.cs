using Microsoft.CodeAnalysis;
using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForCollectionSizeCheck>;

namespace Xunit.Analyzers
{
    public class AssertEqualShouldNotBeUsedForCollectionSizeCheckTests
    {
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

        [Theory]
        [MemberData(nameof(Collections))]
        public async void FindsWarningForEmptyCollectionSizeCheck(string collection)
        {
            var source =
                @"using System.Linq;
class TestClass { void TestMethod() { 
    Xunit.Assert.Equal(0, " + collection + @");
} }";

            var expected = Verify.Diagnostic().WithSpan(3, 5, 3, 28 + collection.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments("Assert.Equal()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async void FindsWarningForNonEmptyCollectionSizeCheck(string collection)
        {
            var source =
                @"using System.Linq;
        class TestClass { void TestMethod() { 
            Xunit.Assert.NotEqual(0, " + collection + @");
        } }";

            var expected = Verify.Diagnostic().WithSpan(3, 13, 3, 39 + collection.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments("Assert.NotEqual()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async void FindsWarningForSingleItemCollectionSizeCheck(string collection)
        {
            var source =
                @"using System.Linq;
        class TestClass { void TestMethod() { 
            Xunit.Assert.Equal(1, " + collection + @");
        } }";

            var expected = Verify.Diagnostic().WithSpan(3, 13, 3, 36 + collection.Length).WithSeverity(DiagnosticSeverity.Warning).WithArguments("Assert.Equal()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async void FindsWarningForSymbolDeclaringTypeHasZeroArity_ImplementsICollectionOfT()
        {
            var source = @"
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
}";

            var expected = Verify.Diagnostic().WithSpan(23, 9, 23, 51).WithSeverity(DiagnosticSeverity.Warning).WithArguments("Assert.Equal()");
            await Verify.VerifyAnalyzerAsync(source, expected);
        }

        [Theory]
        [MemberData(nameof(Collections))]
        public async void DoesNotFindWarningForNonSingleItemCollectionSizeCheck(string collection)
        {
            var source =
                @"using System.Linq;
        class TestClass { void TestMethod() { 
            Xunit.Assert.NotEqual(1, " + collection + @");
        } }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(CollectionsWithUnsupportedSize))]
        public async void DoesNotFindWarningForUnsupportedCollectionSizeCheck(string collection, int size)
        {
            var source =
                @"using System.Linq;
        class TestClass { void TestMethod() { 
            Xunit.Assert.Equal(" + size + ", " + collection + @");
        } }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Theory]
        [MemberData(nameof(CollectionsWithUnsupportedSize))]
        public async void DoesNotFindWarningForUnsupportedNonEqualCollectionSizeCheck(string collection, int size)
        {
            var source =
                @"using System.Linq;
        class TestClass { void TestMethod() { 
            Xunit.Assert.NotEqual(" + size + ", " + collection + @");
        } }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotCrashForSymbolDeclaringTypeHasDifferentArityThanICollection_Zero()
        {
            var source =
                @"using System.Collections.Generic;
        interface IIntCollection : ICollection<int> {
            new int Count { get; }
        }
        class TestClass { void TestMethod() {
            Xunit.Assert.Equal(1, ((IIntCollection)null).Count);
        } }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotCrashForSymbolDeclaringTypeHasDifferentArityThanICollection_Two()
        {
            var source =
                @"using System.Collections.Generic;
        interface IDictionary2<K, V> : ICollection<KeyValuePair<K, V>> {
            new int Count { get; }
        }
        class TestClass { void TestMethod() {
            Xunit.Assert.Equal(1, ((IDictionary2<int, int>)null).Count);
        } }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async void DoesNotCrash_ForNonIntArguments()
        {
            var source =
                @"class TestClass { void TestMethod() {
            Xunit.Assert.Equal('b', new int[0].Length);
        } }";

            await Verify.VerifyAnalyzerAsync(source);
        }
    }
}
