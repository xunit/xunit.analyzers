using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.SetEqualityAnalyzer>;

public class SetEqualityAnalyzerTests
{
	const string customSetAndComparer = /* lang=c#-test */ """
		using System.Collections;
		using System.Collections.Generic;

		public class MySet : ISet<int> {
			public int Count => throw new System.NotImplementedException();
			public bool IsReadOnly => throw new System.NotImplementedException();

			public bool Add(int item) => throw new System.NotImplementedException();
			public void Clear() => throw new System.NotImplementedException();
			public bool Contains(int item) => throw new System.NotImplementedException();
			public void CopyTo(int[] array, int arrayIndex) => throw new System.NotImplementedException();
			public void ExceptWith(IEnumerable<int> other) => throw new System.NotImplementedException();
			public IEnumerator<int> GetEnumerator() => throw new System.NotImplementedException();
			public void IntersectWith(IEnumerable<int> other) => throw new System.NotImplementedException();
			public bool IsProperSubsetOf(IEnumerable<int> other) => throw new System.NotImplementedException();
			public bool IsProperSupersetOf(IEnumerable<int> other) => throw new System.NotImplementedException();
			public bool IsSubsetOf(IEnumerable<int> other) => throw new System.NotImplementedException();
			public bool IsSupersetOf(IEnumerable<int> other) => throw new System.NotImplementedException();
			public bool Overlaps(IEnumerable<int> other) => throw new System.NotImplementedException();
			public bool Remove(int item) => throw new System.NotImplementedException();
			public bool SetEquals(IEnumerable<int> other) => throw new System.NotImplementedException();
			public void SymmetricExceptWith(IEnumerable<int> other) => throw new System.NotImplementedException();
			public void UnionWith(IEnumerable<int> other) => throw new System.NotImplementedException();
			void ICollection<int>.Add(int item) => throw new System.NotImplementedException();
			IEnumerator IEnumerable.GetEnumerator() => throw new System.NotImplementedException();
		}

		public class MyComparer : IEqualityComparer<int> {
			public bool Equals(int x, int y) => throw new System.NotImplementedException();
			public int GetHashCode(int obj) => throw new System.NotImplementedException();
		}
		""";

	public class X2026_SetsMustBeComparedWithEqualityComparer
	{
		public static MatrixTheoryData<string, string, string> MethodWithCollectionCreationData =>
			new(
				/* lang=c#-test */ ["Equal", "NotEqual"],
				/* lang=c#-test */ ["new HashSet<int>()", "new HashSet<int>().ToImmutableHashSet()", "new MySet()"],
				/* lang=c#-test */ ["new HashSet<int>()", "new HashSet<int>().ToImmutableHashSet()", "new MySet()"]
			);

		[Theory]
		[MemberData(nameof(MethodWithCollectionCreationData))]
		public async Task WithCollectionComparer_DoesNotTrigger(
			string method,
			string collection1,
			string collection2)
		{
			var code = string.Format(/* lang=c#-test */ """
				using Xunit;
				using System.Collections.Generic;
				using System.Collections.Immutable;

				public class TestClass {{
					[Fact]
					public void TestMethod() {{
						var collection1 = {1};
						var collection2 = {2};

						Assert.{0}(collection1, collection2, (IEnumerable<int> e1, IEnumerable<int> e2) => true);
					}}
				}}
				""", method, collection1, collection2);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, [code, customSetAndComparer]);
		}

		[Theory]
		[MemberData(nameof(MethodWithCollectionCreationData))]
		public async Task WithEqualityComparer_DoesNotTrigger(
			string method,
			string collection1,
			string collection2)
		{
			var code = string.Format(/* lang=c#-test */ """
				using Xunit;
				using System.Collections.Generic;
				using System.Collections.Immutable;

				public class TestEqualityComparer : IEqualityComparer<int> {{
					public bool Equals(int x, int y) {{
						return true;
					}}

					public int GetHashCode(int obj) {{
						return 0;
					}}
				}}

				public class TestClass {{
					[Fact]
					public void TestMethod() {{
						var collection1 = {1};
						var collection2 = {2};

						Assert.{0}(collection1, collection2, new TestEqualityComparer());
					}}
				}}
				""", method, collection1, collection2);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, [code, customSetAndComparer]);
		}

		[Theory]
		[MemberData(nameof(MethodWithCollectionCreationData))]
		public async Task WithComparerLambda_Triggers(
			string method,
			string collection1,
			string collection2)
		{
			var code = string.Format(/* lang=c#-test */ """
				using Xunit;
				using System.Collections.Generic;
				using System.Collections.Immutable;

				public class TestClass {{
					[Fact]
					public void TestMethod() {{
						var collection1 = {1};
						var collection2 = {2};

						{{|#0:Assert.{0}(collection1, collection2, (int e1, int e2) => true)|}};
					}}
				}}
				""", method, collection1, collection2);
			var expected = Verify.Diagnostic("xUnit2026").WithLocation(0).WithArguments(method);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, [code, customSetAndComparer], expected);
		}

#if ROSLYN_LATEST  // C# 10 is required for local functions

		public static MatrixTheoryData<string, string, string, string> ComparerFunctionData() =>
			new(
				/* lang=c#-test */ ["Equal", "NotEqual"],
				/* lang=c#-test */ ["(int e1, int e2) => true", "FuncComparer", "LocalFunc", "funcDelegate"],
				/* lang=c#-test */ ["new HashSet<int>()", "new HashSet<int>().ToImmutableHashSet()", "new MySet()"],
				/* lang=c#-test */ ["new HashSet<int>()", "new HashSet<int>().ToImmutableHashSet()", "new MySet()"]
			);

		[Theory]
		[MemberData(nameof(ComparerFunctionData))]
		public async Task WithComparerFunction_Triggers(
			string method,
			string comparerFuncSyntax,
			string collection1,
			string collection2)
		{
			var code = string.Format(/* lang=c#-test */ """
				using Xunit;
				using System.Collections.Generic;
				using System.Collections.Immutable;

				public class TestClass {{
					private bool FuncComparer(int obj1, int obj2) {{
						return true;
					}}

					private delegate bool FuncDelegate(int obj1, int obj2);

					[Fact]
					public void TestMethod() {{
						var collection1 = {2};
						var collection2 = {3};

						bool LocalFunc(int obj1, int obj2) {{
							return true;
						}}

						var funcDelegate = FuncComparer;

						{{|#0:Assert.{0}(collection1, collection2, {1})|}};
					}}
				}}
				""", method, comparerFuncSyntax, collection1, collection2);
			var expected = Verify.Diagnostic("xUnit2026").WithLocation(0).WithArguments(method);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp10, new[] { code, customSetAndComparer }, expected);
		}

#endif
	}

	public class X2027_SetsShouldNotBeComparedToLinearContainers
	{
		public static MatrixTheoryData<string, string> MethodAndLinearContainers =>
			new(
				/* lang=c#-test */ ["Equal", "NotEqual"],
				/* lang=c#-test */ ["new List<int>()", "new SortedSet<int>()", "new HashSet<int>().OrderBy(x => x)", "new MySet().OrderBy(x => x)"]
			);

		[Theory]
		[MemberData(nameof(MethodAndLinearContainers))]
		public async Task LinearContainers_DoesNotTrigger(
			string method,
			string collection)
		{
			var code = string.Format(/* lang=c#-test */ """
				using Xunit;
				using System.Collections.Generic;
				using System.Linq;

				public class TestClass {{
					[Fact]
					public void TestMethod() {{
						var collection1 = new List<int>();
						var collection2 = {1};

						Assert.{0}(collection1, collection2);
						Assert.{0}(collection1, collection2, (int e1, int e2) => true);
						Assert.{0}(collection1, collection2, new MyComparer());
					}}
				}}
				""", method, collection);

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, [code, customSetAndComparer]);
		}

		[Fact]
		public async Task CastedSet_DoesNotTrigger()
		{
			var code = /* lang=c#-test */ """
				using Xunit;
				using System.Collections.Generic;

				public class TestClass {
					[Fact]
					public void TestMethod() {
						var expected = new HashSet<string> { "bar", "foo" };
						var actual = new HashSet<string> { "foo", "bar" };

							Assert.Equal(expected, actual);
							Assert.Equal(expected, (ISet<string>)actual);
							Assert.Equal((ISet<string>)expected, actual);
							Assert.Equal((ISet<string>)expected, (ISet<string>)actual);
					}
				}
				""";

			await Verify.VerifyAnalyzer(code);
		}

		public static MatrixTheoryData<string, (string type, string initializer)> MethodAndTypeAndInitializer =>
			new(
				/* lang=c#-test */ ["Equal", "NotEqual"],
				/* lang=c#-test */
				[
					("System.Collections.Generic.HashSet<int>", "new HashSet<int>()"),
					("System.Collections.Immutable.ImmutableHashSet<int>", "new HashSet<int>().ToImmutableHashSet()"),
					("MySet", "new MySet()")
				]
			);

		[Theory]
		[MemberData(nameof(MethodAndTypeAndInitializer), DisableDiscoveryEnumeration = true)]
		public async Task SetWithLinearContainer_Triggers(
			string method,
			(string type, string initializer) collection)
		{
			var code = string.Format(/* lang=c#-test */ """
				using Xunit;
				using System.Collections.Generic;
				using System.Collections.Immutable;
				using System.Linq;

				public class TestClass {{
					[Fact]
					public void TestMethod() {{
						var collection1 = new List<int>();
						var collection2 = {1};

						{{|#0:Assert.{0}(collection1, collection2)|}};
						{{|#1:Assert.{0}(collection1, collection2, (int e1, int e2) => true)|}};
						{{|#2:Assert.{0}(collection1, collection2, new MyComparer())|}};

						{{|#3:Assert.{0}(collection2, collection1)|}};
						{{|#4:Assert.{0}(collection2, collection1, (int e1, int e2) => true)|}};
						{{|#5:Assert.{0}(collection2, collection1, new MyComparer())|}};
					}}
				}}
				""", method, collection.initializer);
			var expected = new[]
			{
				Verify.Diagnostic("xUnit2027").WithLocation(0).WithArguments("System.Collections.Generic.List<int>", collection.type),
				Verify.Diagnostic("xUnit2027").WithLocation(1).WithArguments("System.Collections.Generic.List<int>", collection.type),
				Verify.Diagnostic("xUnit2027").WithLocation(2).WithArguments("System.Collections.Generic.List<int>", collection.type),
				Verify.Diagnostic("xUnit2027").WithLocation(3).WithArguments(collection.type, "System.Collections.Generic.List<int>"),
				Verify.Diagnostic("xUnit2027").WithLocation(4).WithArguments(collection.type, "System.Collections.Generic.List<int>"),
				Verify.Diagnostic("xUnit2027").WithLocation(5).WithArguments(collection.type, "System.Collections.Generic.List<int>"),
			};

			await Verify.VerifyAnalyzer(LanguageVersion.CSharp7, [code, customSetAndComparer], expected);
		}
	}
}
