using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.AssertEqualShouldNotBeUsedForCollectionSizeCheck>;

public class AssertEqualShouldNotBeUsedForCollectionSizeCheckTests
{
	public static TheoryData<string> AllowedCollections =
	[
		"new System.ArraySegment<int>().Count",
		"Microsoft.Extensions.Primitives.StringValues.Empty.Count",
	];
	public static TheoryData<string> DisallowedCollections =
	[
		"new int[0].Length",
		"new System.Collections.ArrayList().Count",
		"new System.Collections.Generic.List<int>().Count",
		"new System.Collections.Generic.HashSet<int>().Count",
		"System.Collections.Immutable.ImmutableArray.Create<int>().Length",
		"new System.Collections.ObjectModel.Collection<int>().Count",
		"new System.Collections.Generic.List<int>().AsReadOnly().Count",
		"System.Linq.Enumerable.Empty<int>().Count()",
	];
	public static TheoryData<string> DisallowedCollectionInterfaces =
	[
		"ICollection",
		"ICollection<string>",
		"IReadOnlyCollection<string>",
	];

	[Theory]
	[MemberData(nameof(AllowedCollections))]
	public async Task AllowedCollection_DoesNotTrigger(string collection)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
				void TestMethod() {{
					Xunit.Assert.Equal(0, {0});
					Xunit.Assert.Equal(1, {0});
					Xunit.Assert.NotEqual(0, {0});
				}}
			}}
			""", collection);

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(AllowedCollections))]
	[MemberData(nameof(DisallowedCollections))]
	public async Task AllowedCheck_DoesNotTrigger(string collection)
	{
		// Anything that's non-zero for Equal/NotEqual and non-one for Equal is allowed
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
				void TestMethod() {{
					Xunit.Assert.Equal(2, {0});
					Xunit.Assert.NotEqual(1, {0});
					Xunit.Assert.NotEqual(2, {0});
				}}
			}}
			""", collection);

		await Verify.VerifyAnalyzer(source);
	}


	[Theory]
	[MemberData(nameof(DisallowedCollections))]
	public async Task InvalidCheckWithConcreteType_Triggers(string collection)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Linq;

			class TestClass {{
				void TestMethod() {{
					{{|#0:Xunit.Assert.Equal(0, {0})|}};
					{{|#1:Xunit.Assert.Equal(1, {0})|}};
					{{|#2:Xunit.Assert.NotEqual(0, {0})|}};
				}}
			}}
			""", collection);
		var expected = new[]
		{
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify.Diagnostic().WithLocation(2).WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(DisallowedCollectionInterfaces))]
	public async Task InvalidCheckWithInterfaceType_Triggers(string @interface)
	{
		var source = string.Format(/* lang=c#-test */ """
			using System.Collections;
			using System.Collections.Generic;

			class TestClass {{
				void TestMethod() {{
					{0} collection = null;
					{{|#0:Xunit.Assert.Equal(0, collection.Count)|}};
					{{|#1:Xunit.Assert.Equal(1, collection.Count)|}};
					{{|#2:Xunit.Assert.NotEqual(0, collection.Count)|}};
				}}
			}}
			""", @interface);
		var expected = new[]
		{
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify.Diagnostic().WithLocation(2).WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task InvalidCheckWithCustomNonGenericCollection_Triggers()
	{
		var source = /* lang=c#-test */ """
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

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

			class TestClass {
				void TestMethod() {
					{|#0:Assert.Equal(0, new IntCollection().Count)|};
					{|#1:Assert.Equal(1, new IntCollection().Count)|};
					{|#2:Assert.NotEqual(0, new IntCollection().Count)|};
				}
			}
			""";
		var expected = new[]
		{
			Verify.Diagnostic().WithLocation(0).WithArguments("Assert.Equal()", Constants.Asserts.Empty),
			Verify.Diagnostic().WithLocation(1).WithArguments("Assert.Equal()", Constants.Asserts.Single),
			Verify.Diagnostic().WithLocation(2).WithArguments("Assert.NotEqual()", Constants.Asserts.NotEmpty),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async Task OverridingCountMethod_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using System.Collections.Generic;

			interface IIntCollection : ICollection<int> {
				new int Count { get; }
			}

			interface ICustomCollection<T> : ICollection<T> {
				new int Count { get; }
			}

			interface ICustomDictionary<K, V> : ICollection<KeyValuePair<K, V>> {
				new int Count { get; }
			}

			class TestClass {
				void TestMethod() {
					Xunit.Assert.Equal(1, ((IIntCollection)null).Count);
					Xunit.Assert.Equal(1, ((ICustomCollection<int>)null).Count);
					Xunit.Assert.Equal(1, ((ICustomDictionary<int, int>)null).Count);
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async Task ForNonIntArguments_DoesNotCrash()
	{
		var source = /* lang=c#-test */ """
			class TestClass {
				void TestMethod() {
					Xunit.Assert.Equal('b', new int[0].Length);
				}
			}
			""";

		await Verify.VerifyAnalyzer(source);
	}
}
