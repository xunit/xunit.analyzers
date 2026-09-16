using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TypeMustHaveSinglePublicConstructor>;

public class X1056_TypeMustHaveSinglePublicConstructorTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			// Test class constructors

			public class NonTestClass {
				public NonTestClass() { }
				public NonTestClass(int x) { }
			}

			public static class StaticTestClass {
				[Fact] public static void TestMethod() { }
			}

			public abstract class AbstractTestClass {
				[Fact] public void TestMethod() { }
			}

			public class TestClass_DefaultConstructor {
				[Fact] public void TestMethod() { }
			}

			public class TestClass_EmptyConstructor {
				public TestClass_EmptyConstructor() { }
				[Fact] public void TestMethod() { }
			}

			public class TestClass_EmptyConstructorWithNonPublicConstructors {
				public TestClass_EmptyConstructorWithNonPublicConstructors() { }
				protected TestClass_EmptyConstructorWithNonPublicConstructors(int x) { }
				static TestClass_EmptyConstructorWithNonPublicConstructors() { }
				[Fact] public void TestMethod() { }
			}

			public class TestClass_NonEmptyConstructor {
				public TestClass_NonEmptyConstructor(int x) { }
				[Fact] public void TestMethod() { }
			}

			public class {|#0:TestClass_DualConstructor|} {
				public TestClass_DualConstructor() { }
				public TestClass_DualConstructor(int x) { }
				protected TestClass_DualConstructor(string x) { }
				static TestClass_DualConstructor() { }
				[Fact] public void TestMethod() { }
			}

			public class InheritedTestClass_DefaultConstructor : AbstractTestClass { }

			public class {|#1:InheritedTestClass_DualConstructor|} : AbstractTestClass {
				public InheritedTestClass_DualConstructor() { }
				public InheritedTestClass_DualConstructor(int x) { }
			}

			// Fixture constructors

			public class Fixture_DefaultConstructor { }

			public class Fixture_EmptyConstructor {
				public Fixture_EmptyConstructor() { }
			}

			public class Fixture_EmptyConstructorWithNonPublicConstructors {
				public Fixture_EmptyConstructorWithNonPublicConstructors() { }
				protected Fixture_EmptyConstructorWithNonPublicConstructors(int x) { }
				static Fixture_EmptyConstructorWithNonPublicConstructors() { }
			}

			public class Fixture_NonEmptyConstructor {
				public Fixture_NonEmptyConstructor(int x) { }
			}

			public class Fixture_DualConstructor {
				public Fixture_DualConstructor() { }
				public Fixture_DualConstructor(int x) { }
				protected Fixture_DualConstructor(string x) { }
				static Fixture_DualConstructor() { }
			}

			public abstract class Fixture_Abstract { }

			public class {|#10:ClassFixtureContainer|} :
				IClassFixture<Fixture_DefaultConstructor>,
				IClassFixture<Fixture_EmptyConstructor>,
				IClassFixture<Fixture_EmptyConstructorWithNonPublicConstructors>,
				IClassFixture<Fixture_NonEmptyConstructor>,
				IClassFixture<Fixture_DualConstructor>,
				IClassFixture<Fixture_Abstract>
			{ }

			public class {|#11:CollectionFixtureContainer|} :
				ICollectionFixture<Fixture_DefaultConstructor>,
				ICollectionFixture<Fixture_EmptyConstructor>,
				ICollectionFixture<Fixture_EmptyConstructorWithNonPublicConstructors>,
				ICollectionFixture<Fixture_NonEmptyConstructor>,
				ICollectionFixture<Fixture_DualConstructor>,
				ICollectionFixture<Fixture_Abstract>
			{ }
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Test class", "TestClass_DualConstructor"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Test class", "InheritedTestClass_DualConstructor"),

			Verify.Diagnostic().WithLocation(10).WithArguments("Fixture", "Fixture_DualConstructor"),
			Verify.Diagnostic().WithLocation(10).WithArguments("Fixture", "Fixture_Abstract"),
			Verify.Diagnostic().WithLocation(11).WithArguments("Fixture", "Fixture_DualConstructor"),
			Verify.Diagnostic().WithLocation(11).WithArguments("Fixture", "Fixture_Abstract"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
