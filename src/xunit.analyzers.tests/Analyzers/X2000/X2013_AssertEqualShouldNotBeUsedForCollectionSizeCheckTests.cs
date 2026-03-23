using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForCollectionSizeCheck>;

public class X2013_AssertEqualShouldNotBeUsedForCollectionSizeCheckTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Collections;
			using System.Collections.Generic;
			using System.Collections.Immutable;
			using System.Linq;
			using Microsoft.Extensions.Primitives;
			using Xunit;

			class TestClass {
				void ExemptedCollection_DoesNotTrigger() {
					// ArraySegment<T>.GetEnumerator() can throw
					Assert.Equal(0, new ArraySegment<int>().Count);
					Assert.Equal(1, new ArraySegment<int>().Count);
					Assert.NotEqual(0, new ArraySegment<int>().Count);

					// StringValues has an implicit string conversion that's preferred by the compiler, https://github.com/xunit/xunit/issues/2859
					Assert.Equal(0, StringValues.Empty.Count);
					Assert.Equal(1, StringValues.Empty.Count);
					Assert.NotEqual(0, StringValues.Empty.Count);
				}

				void DisallowedCollection_AllowedCheck_DoesNotTrigger() {
					// Array
					Assert.Equal(2, new int[0].Length);
					Assert.NotEqual(1, new int[0].Length);
					Assert.NotEqual(2, new int[0].Length);

					// Collection with .Length
					Assert.Equal(2, ImmutableArray.Create<int>().Length);
					Assert.NotEqual(1, ImmutableArray.Create<int>().Length);
					Assert.NotEqual(2, ImmutableArray.Create<int>().Length);

					// Collection with .Count
					Assert.Equal(2, new List<int>().Count);
					Assert.NotEqual(1, new List<int>().Count);
					Assert.NotEqual(2, new List<int>().Count);

					// Linq .Count() method
					Assert.Equal(2, Enumerable.Empty<int>().Count());
					Assert.NotEqual(1, Enumerable.Empty<int>().Count());
					Assert.NotEqual(2, Enumerable.Empty<int>().Count());

					// ICollection
					Assert.Equal(2, default(ICollection).Count);
					Assert.NotEqual(1, default(ICollection).Count);
					Assert.NotEqual(2, default(ICollection).Count);

					// ICollection<T>
					Assert.Equal(2, default(ICollection<int>).Count);
					Assert.NotEqual(1, default(ICollection<int>).Count);
					Assert.NotEqual(2, default(ICollection<int>).Count);

					// IReadOnlyCollection<T>
					Assert.Equal(2, default(IReadOnlyCollection<int>).Count);
					Assert.NotEqual(1, default(IReadOnlyCollection<int>).Count);
					Assert.NotEqual(2, default(IReadOnlyCollection<int>).Count);

					// Custom collection
					Assert.Equal(2, new IntCollection().Count);
					Assert.NotEqual(1, new IntCollection().Count);
					Assert.NotEqual(2, new IntCollection().Count);
				}

				void DisallowedCollection_DisallowedCheck_Triggers() {
					// Array
					{|#0:Assert.Equal(0, new int[0].Length)|};
					{|#1:Assert.Equal(1, new int[0].Length)|};
					{|#2:Assert.NotEqual(0, new int[0].Length)|};

					// Collection with .Length
					{|#10:Assert.Equal(0, ImmutableArray.Create<int>().Length)|};
					{|#11:Assert.Equal(1, ImmutableArray.Create<int>().Length)|};
					{|#12:Assert.NotEqual(0, ImmutableArray.Create<int>().Length)|};

					// Collection with .Count
					{|#20:Assert.Equal(0, new List<int>().Count)|};
					{|#21:Assert.Equal(1, new List<int>().Count)|};
					{|#22:Assert.NotEqual(0, new List<int>().Count)|};

					// Linq .Count() method
					{|#30:Assert.Equal(0, Enumerable.Empty<int>().Count())|};
					{|#31:Assert.Equal(1, Enumerable.Empty<int>().Count())|};
					{|#32:Assert.NotEqual(0, Enumerable.Empty<int>().Count())|};

					// ICollection
					{|#40:Assert.Equal(0, default(ICollection).Count)|};
					{|#41:Assert.Equal(1, default(ICollection).Count)|};
					{|#42:Assert.NotEqual(0, default(ICollection).Count)|};

					// ICollection<T>
					{|#50:Assert.Equal(0, default(ICollection<int>).Count)|};
					{|#51:Assert.Equal(1, default(ICollection<int>).Count)|};
					{|#52:Assert.NotEqual(0, default(ICollection<int>).Count)|};

					// IReadOnlyCollection<T>
					{|#60:Assert.Equal(0, default(IReadOnlyCollection<int>).Count)|};
					{|#61:Assert.Equal(1, default(IReadOnlyCollection<int>).Count)|};
					{|#62:Assert.NotEqual(0, default(IReadOnlyCollection<int>).Count)|};

					// Custom collection
					{|#70:Assert.Equal(0, new IntCollection().Count)|};
					{|#71:Assert.Equal(1, new IntCollection().Count)|};
					{|#72:Assert.NotEqual(0, new IntCollection().Count)|};
				}

				void OverridingCount_DoesNotTrigger() {
					Assert.Equal(1, default(IIntCollection).Count);
					Assert.Equal(1, default(ICustomCollection<int>).Count);
					Assert.Equal(1, default(ICustomDictionary<int, int>).Count);
				}

				void NonIntExpected_DoesNotTrigger() {
					Assert.Equal('b', new int[0].Length);
				}
			}

			class IntCollection : ICollection<int> {
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

			interface IIntCollection : ICollection<int> {
				new int Count { get; }
			}

			interface ICustomCollection<T> : ICollection<T> {
				new int Count { get; }
			}

			interface ICustomDictionary<K, V> : ICollection<KeyValuePair<K, V>> {
				new int Count { get; }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify.Diagnostic().WithLocation(2).WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),

			Verify.Diagnostic().WithLocation(10).WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify.Diagnostic().WithLocation(11).WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify.Diagnostic().WithLocation(12).WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),

			Verify.Diagnostic().WithLocation(20).WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify.Diagnostic().WithLocation(21).WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify.Diagnostic().WithLocation(22).WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),

			Verify.Diagnostic().WithLocation(30).WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify.Diagnostic().WithLocation(31).WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify.Diagnostic().WithLocation(32).WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),

			Verify.Diagnostic().WithLocation(40).WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify.Diagnostic().WithLocation(41).WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify.Diagnostic().WithLocation(42).WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),

			Verify.Diagnostic().WithLocation(50).WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify.Diagnostic().WithLocation(51).WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify.Diagnostic().WithLocation(52).WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),

			Verify.Diagnostic().WithLocation(60).WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify.Diagnostic().WithLocation(61).WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify.Diagnostic().WithLocation(62).WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),

			Verify.Diagnostic().WithLocation(70).WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify.Diagnostic().WithLocation(71).WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify.Diagnostic().WithLocation(72).WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
