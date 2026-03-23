using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.SetEqualityAnalyzer>;

public class X2026_SetEqualityAnalyzerTests
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
			using Xunit;
			using System.Collections.Generic;
			using System.Collections.Immutable;

			class TestClass {
				void WithCollectionComparerLambda_DoesNotTrigger() {
					var set1 = new HashSet<int>();
					var set2 = ImmutableHashSet.Create<int>();
					var set3 = new MySet();

					Assert.Equal(set1, set1, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.Equal(set1, set2, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.Equal(set1, set3, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.Equal(set2, set1, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.Equal(set2, set2, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.Equal(set2, set3, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.Equal(set3, set1, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.Equal(set3, set2, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.Equal(set3, set3, (IEnumerable<int> e1, IEnumerable<int> e2) => true);

					Assert.NotEqual(set1, set1, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.NotEqual(set1, set2, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.NotEqual(set1, set3, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.NotEqual(set2, set1, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.NotEqual(set2, set2, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.NotEqual(set2, set3, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.NotEqual(set3, set1, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.NotEqual(set3, set2, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					Assert.NotEqual(set3, set3, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
				}

				void WithEqualityComparer_DoesNotTrigger() {
					var set1 = new HashSet<int>();
					var set2 = ImmutableHashSet.Create<int>();
					var set3 = new MySet();
					var comparer = new TestEqualityComparer();

					Assert.Equal(set1, set1, comparer);
					Assert.Equal(set1, set2, comparer);
					Assert.Equal(set1, set3, comparer);
					Assert.Equal(set2, set1, comparer);
					Assert.Equal(set2, set2, comparer);
					Assert.Equal(set2, set3, comparer);
					Assert.Equal(set3, set1, comparer);
					Assert.Equal(set3, set2, comparer);
					Assert.Equal(set3, set3, comparer);

					Assert.NotEqual(set1, set1, comparer);
					Assert.NotEqual(set1, set2, comparer);
					Assert.NotEqual(set1, set3, comparer);
					Assert.NotEqual(set2, set1, comparer);
					Assert.NotEqual(set2, set2, comparer);
					Assert.NotEqual(set2, set3, comparer);
					Assert.NotEqual(set3, set1, comparer);
					Assert.NotEqual(set3, set2, comparer);
					Assert.NotEqual(set3, set3, comparer);
				}
			}

			public class TestEqualityComparer : IEqualityComparer<int> {
				public bool Equals(int x, int y) => true;
				public int GetHashCode(int obj) => 0;
			}
			""";

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, [code, customSetAndComparer]);
	}

	[Fact]
	public async ValueTask V2_and_V3_NonAOT()
	{
		var code = /* lang=c#-test */ """
			using System;
			using System.Collections.Generic;
			using System.Collections.Immutable;
			using Xunit;

			class TestClass {
				bool FuncComparer(int obj1, int obj2) => true;

				void WithComparerLambda_Triggers() {
					var set1 = new HashSet<int>();
					var set2 = ImmutableHashSet.Create<int>();
					var set3 = new MySet();
					Func<int, int, bool> funcDelegate = FuncComparer;

					bool LocalFuncComparer(int obj1, int obj2) => true;

					{|#0:Assert.Equal(set1, set1, (int e1, int e2) => true)|};
					{|#1:Assert.Equal(set1, set2, (int e1, int e2) => true)|};
					{|#2:Assert.Equal(set1, set3, (int e1, int e2) => true)|};
					{|#3:Assert.Equal(set2, set1, (int e1, int e2) => true)|};
					{|#4:Assert.Equal(set2, set2, (int e1, int e2) => true)|};
					{|#5:Assert.Equal(set2, set3, (int e1, int e2) => true)|};
					{|#6:Assert.Equal(set3, set1, (int e1, int e2) => true)|};
					{|#7:Assert.Equal(set3, set2, (int e1, int e2) => true)|};
					{|#8:Assert.Equal(set3, set3, (int e1, int e2) => true)|};

					{|#10:Assert.Equal(set1, set1, FuncComparer)|};
					{|#11:Assert.Equal(set1, set2, FuncComparer)|};
					{|#12:Assert.Equal(set1, set3, FuncComparer)|};
					{|#13:Assert.Equal(set2, set1, FuncComparer)|};
					{|#14:Assert.Equal(set2, set2, FuncComparer)|};
					{|#15:Assert.Equal(set2, set3, FuncComparer)|};
					{|#16:Assert.Equal(set3, set1, FuncComparer)|};
					{|#17:Assert.Equal(set3, set2, FuncComparer)|};
					{|#18:Assert.Equal(set3, set3, FuncComparer)|};

					{|#20:Assert.Equal(set1, set1, LocalFuncComparer)|};
					{|#21:Assert.Equal(set1, set2, LocalFuncComparer)|};
					{|#22:Assert.Equal(set1, set3, LocalFuncComparer)|};
					{|#23:Assert.Equal(set2, set1, LocalFuncComparer)|};
					{|#24:Assert.Equal(set2, set2, LocalFuncComparer)|};
					{|#25:Assert.Equal(set2, set3, LocalFuncComparer)|};
					{|#26:Assert.Equal(set3, set1, LocalFuncComparer)|};
					{|#27:Assert.Equal(set3, set2, LocalFuncComparer)|};
					{|#28:Assert.Equal(set3, set3, LocalFuncComparer)|};

					{|#30:Assert.Equal(set1, set1, funcDelegate)|};
					{|#31:Assert.Equal(set1, set2, funcDelegate)|};
					{|#32:Assert.Equal(set1, set3, funcDelegate)|};
					{|#33:Assert.Equal(set2, set1, funcDelegate)|};
					{|#34:Assert.Equal(set2, set2, funcDelegate)|};
					{|#35:Assert.Equal(set2, set3, funcDelegate)|};
					{|#36:Assert.Equal(set3, set1, funcDelegate)|};
					{|#37:Assert.Equal(set3, set2, funcDelegate)|};
					{|#38:Assert.Equal(set3, set3, funcDelegate)|};

					{|#40:Assert.NotEqual(set1, set1, (int e1, int e2) => true)|};
					{|#41:Assert.NotEqual(set1, set2, (int e1, int e2) => true)|};
					{|#42:Assert.NotEqual(set1, set3, (int e1, int e2) => true)|};
					{|#43:Assert.NotEqual(set2, set1, (int e1, int e2) => true)|};
					{|#44:Assert.NotEqual(set2, set2, (int e1, int e2) => true)|};
					{|#45:Assert.NotEqual(set2, set3, (int e1, int e2) => true)|};
					{|#46:Assert.NotEqual(set3, set1, (int e1, int e2) => true)|};
					{|#47:Assert.NotEqual(set3, set2, (int e1, int e2) => true)|};
					{|#48:Assert.NotEqual(set3, set3, (int e1, int e2) => true)|};

					{|#50:Assert.NotEqual(set1, set1, FuncComparer)|};
					{|#51:Assert.NotEqual(set1, set2, FuncComparer)|};
					{|#52:Assert.NotEqual(set1, set3, FuncComparer)|};
					{|#53:Assert.NotEqual(set2, set1, FuncComparer)|};
					{|#54:Assert.NotEqual(set2, set2, FuncComparer)|};
					{|#55:Assert.NotEqual(set2, set3, FuncComparer)|};
					{|#56:Assert.NotEqual(set3, set1, FuncComparer)|};
					{|#57:Assert.NotEqual(set3, set2, FuncComparer)|};
					{|#58:Assert.NotEqual(set3, set3, FuncComparer)|};

					{|#60:Assert.NotEqual(set1, set1, LocalFuncComparer)|};
					{|#61:Assert.NotEqual(set1, set2, LocalFuncComparer)|};
					{|#62:Assert.NotEqual(set1, set3, LocalFuncComparer)|};
					{|#63:Assert.NotEqual(set2, set1, LocalFuncComparer)|};
					{|#64:Assert.NotEqual(set2, set2, LocalFuncComparer)|};
					{|#65:Assert.NotEqual(set2, set3, LocalFuncComparer)|};
					{|#66:Assert.NotEqual(set3, set1, LocalFuncComparer)|};
					{|#67:Assert.NotEqual(set3, set2, LocalFuncComparer)|};
					{|#68:Assert.NotEqual(set3, set3, LocalFuncComparer)|};

					{|#70:Assert.NotEqual(set1, set1, funcDelegate)|};
					{|#71:Assert.NotEqual(set1, set2, funcDelegate)|};
					{|#72:Assert.NotEqual(set1, set3, funcDelegate)|};
					{|#73:Assert.NotEqual(set2, set1, funcDelegate)|};
					{|#74:Assert.NotEqual(set2, set2, funcDelegate)|};
					{|#75:Assert.NotEqual(set2, set3, funcDelegate)|};
					{|#76:Assert.NotEqual(set3, set1, funcDelegate)|};
					{|#77:Assert.NotEqual(set3, set2, funcDelegate)|};
					{|#78:Assert.NotEqual(set3, set3, funcDelegate)|};
				}
			}

			public class TestEqualityComparer : IEqualityComparer<int> {
				public bool Equals(int x, int y) => true;
				public int GetHashCode(int obj) => 0;
			}
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit2026").WithLocation(0).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(1).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(2).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(3).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(4).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(5).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(6).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(7).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(8).WithArguments("Equal"),

			Verify.Diagnostic("xUnit2026").WithLocation(10).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(11).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(12).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(13).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(14).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(15).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(16).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(17).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(18).WithArguments("Equal"),

			Verify.Diagnostic("xUnit2026").WithLocation(20).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(21).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(22).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(23).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(24).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(25).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(26).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(27).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(28).WithArguments("Equal"),

			Verify.Diagnostic("xUnit2026").WithLocation(30).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(31).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(32).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(33).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(34).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(35).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(36).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(37).WithArguments("Equal"),
			Verify.Diagnostic("xUnit2026").WithLocation(38).WithArguments("Equal"),

			Verify.Diagnostic("xUnit2026").WithLocation(40).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(41).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(42).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(43).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(44).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(45).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(46).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(47).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(48).WithArguments("NotEqual"),

			Verify.Diagnostic("xUnit2026").WithLocation(50).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(51).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(52).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(53).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(54).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(55).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(56).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(57).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(58).WithArguments("NotEqual"),

			Verify.Diagnostic("xUnit2026").WithLocation(60).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(61).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(62).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(63).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(64).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(65).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(66).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(67).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(68).WithArguments("NotEqual"),

			Verify.Diagnostic("xUnit2026").WithLocation(70).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(71).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(72).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(73).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(74).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(75).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(76).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(77).WithArguments("NotEqual"),
			Verify.Diagnostic("xUnit2026").WithLocation(78).WithArguments("NotEqual"),
		};

		await Verify.VerifyAnalyzerNonAot(LanguageVersion.CSharp7, [code, customSetAndComparer], expected);
	}
}
