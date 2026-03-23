using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.EnsureFixturesHaveASource>;

#if NETCOREAPP && ROSLYN_LATEST
using Microsoft.CodeAnalysis.CSharp;
#endif

public class X1041_EnsureFixturesHaveASourceTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class Fixture { }
			public class Fixture<T> { }

			public class NonTestClass {
				public NonTestClass(object _) { }
			}

			// Supported non-fixture argument types

			public class OptionalParameter_DoesNotTrigger {
				public OptionalParameter_DoesNotTrigger(bool value = true) { }

				[Fact] public void TestMethod() { }
			}

			public class ParamsParameter_DoesNotTrigger {
				public ParamsParameter_DoesNotTrigger(params object[] _) { }

				[Fact] public void TestMethod() { }
			}

			public class NonCollection {
				public NonCollection(ITestOutputHelper _) { }

				[Fact] public void TestMethod() { }
			}

			// Unresolved fixture argument

			public class MissingClassFixture {
				public MissingClassFixture(object [|_|]) { }

				[Fact] public void TestMethod() { }
			}

			// Built-in non-fixture type for v2 & v3

			[Collection("Test Collection")]
			public class WithCollection {
				public WithCollection(ITestOutputHelper _) { }

				[Fact] public void TestMethod() { }
			}

			// Class fixtures

			public abstract class ClassFixtureOnBase: IClassFixture<object> { }

			public class ClassFixtureOnBase_Derived : ClassFixtureOnBase {
				public ClassFixtureOnBase_Derived(object _) { }

				[Fact] public void TestMethod() { }
			}

			public abstract class ClassFixtureOnDerived { }

			public class ClassFixtureOnDerived_Derived : ClassFixtureOnDerived, IClassFixture<object> {
				public ClassFixtureOnDerived_Derived(object _) { }

				[Fact] public void TestMethod() { }
			}

			[CollectionDefinition(nameof(ClassFixtureOnCollection))]
			public class ClassFixtureOnCollection : IClassFixture<object> { }

			[Collection(nameof(ClassFixtureOnCollection))]
			public class ClassFixtureOnCollectionTestClass {
				public ClassFixtureOnCollectionTestClass(object _) { }

				[Fact] public void TestMethod() { }
			}

			// Collection fixtures

			[CollectionDefinition(nameof(FixtureCollection))]
			public class FixtureCollection : ICollectionFixture<Fixture> { }

			[Collection(nameof(FixtureCollection))]
			public class WithDirectFixture_DoesNotTrigger {
				public WithDirectFixture_DoesNotTrigger(Fixture fixture) { }

				[Fact]
				public void TestMethod() { }
			}

			public abstract class TestContext {
				protected TestContext(Fixture fixture) { }
			}

			[CollectionDefinition(nameof(InheritedFixtureCollection))]
			public class InheritedFixtureCollection : ICollectionFixture<Fixture> { }

			[Collection(nameof(InheritedFixtureCollection))]
			public class WithInheritedFixture_DoesNotTrigger : TestContext {
				public WithInheritedFixture_DoesNotTrigger(Fixture fixture) : base(fixture) { }

				[Fact]
				public void TestMethod() { }
			}

			// Direct generic collection fixture (v3 only)

			[CollectionDefinition("Generic test collection")]
			public class GenericTestCollection<TCollectionFixture> : ICollectionFixture<Fixture<TCollectionFixture>> { }

			[Collection("Generic test collection")]
			public class DirectGenericFixture {
				public DirectGenericFixture(Fixture<int> {|#0:fixture|}) { }

				[Fact]
				public void TestMethod() { }
			}

			// Inherited generic collection fixture (v3 only)

			[Collection("Generic test collection")]
			public abstract class TestContext<TContextFixture> {
				protected TestContext(Fixture<TContextFixture> fixture) { }
			}

			public class InheritedGenericFixture : TestContext<int> {
				public InheritedGenericFixture(Fixture<int> {|#1:fixture|}) : base(fixture) { }

				[Fact]
				public void TestMethod() { }
			}

			// Mixed fixtures

			public class ClassFixture { }
			public class CollectionFixture { }

			[CollectionDefinition(nameof(WithoutCollectionFixtureCollection))]
			public class WithoutCollectionFixtureCollection { }

			[CollectionDefinition(nameof(WithCollectionFixtureCollection))]
			public class WithCollectionFixtureCollection : ICollectionFixture<CollectionFixture> { }

			[Collection(nameof(WithCollectionFixtureCollection))]
			public class WithBoth_DoesNotTrigger : IClassFixture<ClassFixture> {
				public WithBoth_DoesNotTrigger(ClassFixture _1, CollectionFixture _2, ITestOutputHelper _3) { }

				[Fact] public void TestMethod() { }
			}

			[Collection(nameof(WithCollectionFixtureCollection))]
			public class MissingClassFixture_Triggers {
				public MissingClassFixture_Triggers(ClassFixture [|_1|], CollectionFixture _2, ITestOutputHelper _3) { }

				[Fact] public void TestMethod() { }
			}

			[Collection(nameof(WithoutCollectionFixtureCollection))]
			public class MissingCollectionFixture_Triggers : IClassFixture<ClassFixture> {
				public MissingCollectionFixture_Triggers(ClassFixture _1, CollectionFixture [|_2|], ITestOutputHelper _3) { }

				[Fact] public void TestMethod() {{ }}
			}
			""";
		var expectedV2 = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("fixture"),
			Verify.Diagnostic().WithLocation(1).WithArguments("fixture"),
		};

		await Verify.VerifyAnalyzerV2("using Xunit.Abstractions; " + source, expectedV2);
		await Verify.VerifyAnalyzerV3(source);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			[assembly: AssemblyFixture(typeof(int))]

			public class Fixture { }
			public class Fixture<T> { }

			// Assembly fixtures

			public class WithAssemblyFixture {
				public WithAssemblyFixture(int _) { }

				[Fact] public void TestMethod() { }
			}

			// Built-in non-fixture types for v3

			public class NonCollection {
				public NonCollection(ITestOutputHelper _1, ITestContextAccessor _2) { }

				[Fact] public void TestMethod() { }
			}

			[Collection("Test Collection")]
			public class WithCollection {
				public WithCollection(ITestOutputHelper _1, ITestContextAccessor _2) { }

				[Fact] public void TestMethod() { }
			}

			// Collection fixture via implictly typed collection

			public abstract class TestContext {
				protected TestContext(Fixture fixture) { }
			}

			[CollectionDefinition]
			public class InheritedFixtureByTypeCollection : ICollectionFixture<Fixture> { }

			[Collection(typeof(InheritedFixtureByTypeCollection))]
			public class WithInheritedFixtureByType_DoesNotTrigger : TestContext {
				public WithInheritedFixtureByType_DoesNotTrigger(Fixture fixture) : base(fixture) { }

				[Fact]
				public void TestMethod() { }
			}
			""";

		await Verify.VerifyAnalyzerV3(source);
	}

#if NETCOREAPP && ROSLYN_LATEST  // C# 11 is required for generic attributes

	[Fact]
	public async ValueTask V3_only_GenericAttribute()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class Fixture { }

			// Inherited collection fixture (by type, generic)

			public abstract class TestContext {
				protected TestContext(Fixture fixture) { }
			}

			[CollectionDefinition]
			public class InheritedFixtureByTypeCollection : ICollectionFixture<Fixture> { }

			[Collection<InheritedFixtureByTypeCollection>]
			public class WithInheritedFixtureByType_DoesNotTrigger : TestContext {
				public WithInheritedFixtureByType_DoesNotTrigger(Fixture fixture) : base(fixture) { }

				[Fact]
				public void TestMethod() { }
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp11, source);
	}

#endif  // NETCOREAPP && ROSLYN_LATEST
}
