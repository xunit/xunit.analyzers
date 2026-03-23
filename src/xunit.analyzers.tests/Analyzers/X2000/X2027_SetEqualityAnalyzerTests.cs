using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.SetEqualityAnalyzer>;

public class X2027_SetEqualityAnalyzerTests
{
	const string customSetAndComparer = /* lang=c#-test */ """
		using System;
		using System.Collections;
		using System.Collections.Generic;

		public class MySet : ISet<int> {
			public int Count => throw new NotImplementedException();
			public bool IsReadOnly => throw new NotImplementedException();

			public bool Add(int item) => throw new NotImplementedException();
			public void Clear() => throw new NotImplementedException();
			public bool Contains(int item) => throw new NotImplementedException();
			public void CopyTo(int[] array, int arrayIndex) => throw new NotImplementedException();
			public void ExceptWith(IEnumerable<int> other) => throw new NotImplementedException();
			public IEnumerator<int> GetEnumerator() => throw new NotImplementedException();
			public void IntersectWith(IEnumerable<int> other) => throw new NotImplementedException();
			public bool IsProperSubsetOf(IEnumerable<int> other) => throw new NotImplementedException();
			public bool IsProperSupersetOf(IEnumerable<int> other) => throw new NotImplementedException();
			public bool IsSubsetOf(IEnumerable<int> other) => throw new NotImplementedException();
			public bool IsSupersetOf(IEnumerable<int> other) => throw new NotImplementedException();
			public bool Overlaps(IEnumerable<int> other) => throw new NotImplementedException();
			public bool Remove(int item) => throw new NotImplementedException();
			public bool SetEquals(IEnumerable<int> other) => throw new NotImplementedException();
			public void SymmetricExceptWith(IEnumerable<int> other) => throw new NotImplementedException();
			public void UnionWith(IEnumerable<int> other) => throw new NotImplementedException();
			void ICollection<int>.Add(int item) => throw new NotImplementedException();
			IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
		}

		public class MyComparer : IEqualityComparer<int> {
			public bool Equals(int x, int y) => throw new NotImplementedException();
			public int GetHashCode(int obj) => throw new NotImplementedException();
		}
		""";

	[Fact]
	public async ValueTask V2_and_V3()
	{
		var code = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Linq;
			using Xunit;

			class TestClass {
				void LinearContainers_DoesNotTrigger() {
					var list = new List<int>();
					var orderedSet = new HashSet<int>().OrderBy(x => x);
					var orderedCustomSet = new MySet().OrderBy(x => x);
					var comparer = new MyComparer();

					Assert.Equal(list, list);
					Assert.Equal(list, orderedSet);
					Assert.Equal(list, orderedCustomSet);
					Assert.Equal(list, list, (int e1, int e2) => true);
					Assert.Equal(list, orderedSet, (int e1, int e2) => true);
					Assert.Equal(list, orderedCustomSet, (int e1, int e2) => true);
					Assert.Equal(list, list, comparer);
					Assert.Equal(list, orderedSet, comparer);
					Assert.Equal(list, orderedCustomSet, comparer);

					Assert.NotEqual(list, list);
					Assert.NotEqual(list, orderedSet);
					Assert.NotEqual(list, orderedCustomSet);
					Assert.NotEqual(list, list, (int e1, int e2) => true);
					Assert.NotEqual(list, orderedSet, (int e1, int e2) => true);
					Assert.NotEqual(list, orderedCustomSet, (int e1, int e2) => true);
					Assert.NotEqual(list, list, comparer);
					Assert.NotEqual(list, orderedSet, comparer);
					Assert.NotEqual(list, orderedCustomSet, comparer);
				}

				void CastedSet_DoesNotTrigger() {
					var expected = new HashSet<string> { "bar", "foo" };
					var actual = new HashSet<string> { "foo", "bar" };

					Assert.Equal(expected, actual);
					Assert.Equal(expected, (ISet<string>)actual);
					Assert.Equal((ISet<string>)expected, actual);
					Assert.Equal((ISet<string>)expected, (ISet<string>)actual);
				}

				void SetWithLinearContainer_Triggers() {
					var list = new List<int>();
					var unorderedSet = new HashSet<int>();
					var unorderedCustomSet = new MySet();
					var comparer = new MyComparer();

					{|#0:Assert.Equal(list, unorderedSet)|};
					{|#1:Assert.Equal(list, unorderedCustomSet)|};
					{|#2:Assert.Equal(list, unorderedSet, comparer)|};
					{|#3:Assert.Equal(list, unorderedCustomSet, comparer)|};

					{|#10:Assert.Equal(unorderedSet, list)|};
					{|#11:Assert.Equal(unorderedCustomSet, list)|};
					{|#12:Assert.Equal(unorderedSet, list, comparer)|};
					{|#13:Assert.Equal(unorderedCustomSet, list, comparer)|};

					{|#20:Assert.NotEqual(list, unorderedSet)|};
					{|#21:Assert.NotEqual(list, unorderedCustomSet)|};
					{|#22:Assert.NotEqual(list, unorderedSet, comparer)|};
					{|#23:Assert.NotEqual(list, unorderedCustomSet, comparer)|};

					{|#30:Assert.NotEqual(unorderedSet, list)|};
					{|#31:Assert.NotEqual(unorderedCustomSet, list)|};
					{|#32:Assert.NotEqual(unorderedSet, list, comparer)|};
					{|#33:Assert.NotEqual(unorderedCustomSet, list, comparer)|};
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit2027").WithLocation(0).WithArguments("System.Collections.Generic.List<int>", "System.Collections.Generic.HashSet<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(1).WithArguments("System.Collections.Generic.List<int>", "MySet"),
			Verify.Diagnostic("xUnit2027").WithLocation(2).WithArguments("System.Collections.Generic.List<int>", "System.Collections.Generic.HashSet<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(3).WithArguments("System.Collections.Generic.List<int>", "MySet"),

			Verify.Diagnostic("xUnit2027").WithLocation(10).WithArguments("System.Collections.Generic.HashSet<int>", "System.Collections.Generic.List<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(11).WithArguments("MySet", "System.Collections.Generic.List<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(12).WithArguments("System.Collections.Generic.HashSet<int>", "System.Collections.Generic.List<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(13).WithArguments("MySet", "System.Collections.Generic.List<int>"),

			Verify.Diagnostic("xUnit2027").WithLocation(20).WithArguments("System.Collections.Generic.List<int>", "System.Collections.Generic.HashSet<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(21).WithArguments("System.Collections.Generic.List<int>", "MySet"),
			Verify.Diagnostic("xUnit2027").WithLocation(22).WithArguments("System.Collections.Generic.List<int>", "System.Collections.Generic.HashSet<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(23).WithArguments("System.Collections.Generic.List<int>", "MySet"),

			Verify.Diagnostic("xUnit2027").WithLocation(30).WithArguments("System.Collections.Generic.HashSet<int>", "System.Collections.Generic.List<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(31).WithArguments("MySet", "System.Collections.Generic.List<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(32).WithArguments("System.Collections.Generic.HashSet<int>", "System.Collections.Generic.List<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(33).WithArguments("MySet", "System.Collections.Generic.List<int>"),
		};

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, [code, customSetAndComparer], expected);
	}

	[Fact]
	public async ValueTask V2_and_V3_NonAot()
	{
		var code = /* lang=c#-test */ """
			using System.Collections.Generic;
			using System.Linq;
			using Xunit;

			class TestClass {
				void SetWithLinearContainer_Triggers() {
					var list = new List<int>();
					var unorderedSet = new HashSet<int>();
					var unorderedCustomSet = new MySet();

					{|#0:Assert.Equal(list, unorderedSet, (int e1, int e2) => true)|};
					{|#1:Assert.Equal(list, unorderedCustomSet, (int e1, int e2) => true)|};

					{|#10:Assert.Equal(unorderedSet, list, (int e1, int e2) => true)|};
					{|#11:Assert.Equal(unorderedCustomSet, list, (int e1, int e2) => true)|};

					{|#20:Assert.NotEqual(list, unorderedSet, (int e1, int e2) => true)|};
					{|#21:Assert.NotEqual(list, unorderedCustomSet, (int e1, int e2) => true)|};

					{|#30:Assert.NotEqual(unorderedSet, list, (int e1, int e2) => true)|};
					{|#31:Assert.NotEqual(unorderedCustomSet, list, (int e1, int e2) => true)|};
				}
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit2027").WithLocation(0).WithArguments("System.Collections.Generic.List<int>", "System.Collections.Generic.HashSet<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(1).WithArguments("System.Collections.Generic.List<int>", "MySet"),

			Verify.Diagnostic("xUnit2027").WithLocation(10).WithArguments("System.Collections.Generic.HashSet<int>", "System.Collections.Generic.List<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(11).WithArguments("MySet", "System.Collections.Generic.List<int>"),

			Verify.Diagnostic("xUnit2027").WithLocation(20).WithArguments("System.Collections.Generic.List<int>", "System.Collections.Generic.HashSet<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(21).WithArguments("System.Collections.Generic.List<int>", "MySet"),

			Verify.Diagnostic("xUnit2027").WithLocation(30).WithArguments("System.Collections.Generic.HashSet<int>", "System.Collections.Generic.List<int>"),
			Verify.Diagnostic("xUnit2027").WithLocation(31).WithArguments("MySet", "System.Collections.Generic.List<int>"),
		};

		await Verify.VerifyAnalyzerNonAot(LanguageVersion.CSharp7, [code, customSetAndComparer], expected);
	}
}
