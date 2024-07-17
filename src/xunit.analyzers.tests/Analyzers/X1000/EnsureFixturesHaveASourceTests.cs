using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.EnsureFixturesHaveASource>;

public class EnsureFixturesHaveASourceTests
{
	public class NonTestClass
	{
		[Fact]
		public async Task DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public class NonTestClass {
				    public NonTestClass(object _) { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class SupportedNonFixtureData
	{
		[Theory]
		[InlineData("")]
		[InlineData("[Collection(\"TestCollection\")]")]
		public async Task SupportedTypes_V2_DoesNotTrigger(string attribute)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;
				using Xunit.Abstractions;

				{0} public class TestClass {{
				    public TestClass(ITestOutputHelper _) {{ }}

				    [Fact] public void TestMethod() {{ }}
				}}
				""", attribute);

			await Verify.VerifyAnalyzerV2(source);
		}

		[Theory]
		[InlineData("")]
		[InlineData("[Collection(\"TestCollection\")]")]
		public async Task SupportedTypes_V3_DoesNotTrigger(string attribute)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;
				using Xunit.v3;

				{0} public class TestClass {{
				    public TestClass(ITestOutputHelper _1, ITestContextAccessor _2) {{ }}

				    [Fact] public void TestMethod() {{ }}
				}}
				""", attribute);

			await Verify.VerifyAnalyzerV3(source);
		}

		[Fact]
		public async Task OptionalParameter_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
				    public TestClass(bool value = true) { }

				    [Fact] public void TestMethod() { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class ClassFixtures
	{
		[Theory]
		// Everything on the base type
		[InlineData(
			"[Collection(\"TestCollection\")]", ": IClassFixture<object>",
			"", "")]
		// Everything on the derived type
		[InlineData(
			"", "",
			"[Collection(\"TestCollection\")]", ", IClassFixture<object>")]
		// Fixture on the base type, collection on the derived type
		[InlineData(
			"", ": IClassFixture<object>",
			"[Collection(\"TestCollection\")]", "")]
		// Collection on the base type, fixture on the derived type
		[InlineData(
			"[Collection(\"TestCollection\")]", "",
			"", ", IClassFixture<object>")]
		public async Task BaseClassParameter_DerivedClassFixture_DoesNotTrigger(
			string baseAttribute,
			string baseInterface,
			string derivedAttribute,
			string derivedInterface)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				{0}
				public abstract class BaseClass {1} {{ }}

				{2}
				public class TestClass : BaseClass {3} {{
				    public TestClass(object _) {{ }}

				    [Fact] public void TestMethod() {{ }}
				}}
				""", baseAttribute, baseInterface, derivedAttribute, derivedInterface);

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task ClassFixtureOnCollectionDefinition_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				[CollectionDefinition(nameof(TestCollection))]
				public class TestCollection : IClassFixture<object> { }

				[Collection(nameof(TestCollection))]
				public class TestClass {
				    public TestClass(object _) { }

				    [Fact] public void TestMethod() { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MissingClassFixtureDefinition_Triggers()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
				    public TestClass(object [|_|]) { }

				    [Fact] public void TestMethod() { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class CollectionFixtures
	{
		[Theory]
		[InlineData("")]
		[InlineData("[CollectionDefinition(nameof(TestCollection))]")]
		public async Task NoFixture_DoesNotTrigger(string definitionAttribute)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				{0}
				public class TestCollection {{ }}

				[Collection(nameof(TestCollection))]
				public class TestClass {{
				    [Fact] public void TestMethod() {{ }}
				}}
				""", definitionAttribute);

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task WithInheritedFixture_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class Fixture { }

				[CollectionDefinition("test")]
				public class TestCollection : ICollectionFixture<Fixture> { }

				public abstract class TestContext {
				    protected TestContext(Fixture fixture) { }
				}

				[Collection("test")]
				public class TestClass : TestContext {
				    public TestClass(Fixture fixture) : base(fixture) { }

				    [Fact]
				    public void TestMethod() { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task WithGenericFixture_TriggersWithV2_DoesNotTriggerWithV3()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class Fixture<T> { }

				[CollectionDefinition("test")]
				public class TestCollection<TCollectionFixture> : ICollectionFixture<Fixture<TCollectionFixture>> { }

				[Collection("test")]
				public class TestClass {
				    public TestClass(Fixture<int> {|#0:fixture|}) { }

				    [Fact]
				    public void TestMethod() { }
				}
				""";
			var expectedV2 = Verify.Diagnostic().WithLocation(0).WithArguments("fixture");

			await Verify.VerifyAnalyzerV2(source, expectedV2);
			await Verify.VerifyAnalyzerV3(source);
		}

		[Fact]
		public async Task WithInheritedGenericFixture_TriggersWithV2_DoesNotTriggerWithV3()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class Fixture<T> { }

				[CollectionDefinition("test")]
				public class TestCollection<TCollectionFixture> : ICollectionFixture<Fixture<TCollectionFixture>> { }

				[Collection("test")]
				public abstract class TestContext<TContextFixture> {
				    protected TestContext(Fixture<TContextFixture> fixture) { }
				}

				public class TestClass : TestContext<int> {
				    public TestClass(Fixture<int> {|#0:fixture|}) : base(fixture) { }

				    [Fact]
				    public void TestMethod() { }
				}
				""";
			var expectedV2 = Verify.Diagnostic().WithLocation(0).WithArguments("fixture");

			await Verify.VerifyAnalyzerV2(source, expectedV2);
			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[InlineData("[Collection(nameof(TestCollection))]", "")]
		[InlineData("", "[Collection(nameof(TestCollection))]")]
		public async Task WithFixture_SupportsDerivation(
			string baseAttribute,
			string derivedAttribute)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				[CollectionDefinition(nameof(TestCollection))]
				public class TestCollection : ICollectionFixture<object> {{ }}

				{0}
				public abstract class BaseClass {{ }}

				{1}
				public class TestClass : BaseClass {{
				    public TestClass(object _) {{ }}
				}}
				""", baseAttribute, derivedAttribute);

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task WithFixture_WithDefinition_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				[CollectionDefinition(nameof(TestCollection))]
				public class TestCollection : ICollectionFixture<object> { }

				[Collection(nameof(TestCollection))]
				public class TestClass {
				    public TestClass(object _) { }

				    [Fact] public void TestMethod() { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("")]
		[InlineData("[CollectionDefinition(nameof(TestCollection))]")]
		public async Task WithFixture_WithoutCollectionFixtureInterface_Triggers(string definitionAttribute)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				{0}
				public class TestCollection {{ }}

				[Collection(nameof(TestCollection))]
				public class TestClass {{
				    public TestClass(object [|_|]) {{ }}

				    [Fact] public void TestMethod() {{ }}
				}}
				""", definitionAttribute);

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class AssemblyFixtures
	{
		[Fact]
		public async Task WithAssemblyFixture_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				[assembly: AssemblyFixture(typeof(object))]

				public class TestClass {
				    public TestClass(object _) { }

				    [Fact] public void TestMethod() { }
				}
				""";

			await Verify.VerifyAnalyzerV3(source);
		}
	}

	public class MixedFixtures
	{
		[Theory]
		[InlineData("")]
		[InlineData("[CollectionDefinition(nameof(TestCollection))]")]
		public async Task WithClassFixture_WithCollection_DoesNotTrigger(string definitionAttribute)
		{
			var source = string.Format(/* lang=c#-test */ """
				using Xunit;

				{0}
				public class TestCollection {{ }}

				[Collection(nameof(TestCollection))]
				public class TestClass : IClassFixture<object> {{
				    public TestClass(object _) {{ }}

				    [Fact] public void TestMethod() {{ }}
				}}
				""", definitionAttribute);

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task WithMixedClassAndCollectionFixture_AndSupportedNonFixture_DoesNotTrigger()
		{
			var sourceTemplate = /* lang=c#-test */ """
				using Xunit;

				public class ClassFixture {{ }}
				public class CollectionFixture {{ }}

				[CollectionDefinition(nameof(TestCollection))]
				public class TestCollection : ICollectionFixture<CollectionFixture> {{ }}

				[Collection(nameof(TestCollection))]
				public class TestClass : IClassFixture<ClassFixture> {{
				    public TestClass(ClassFixture _1, CollectionFixture _2, {0} _3) {{ }}

				    [Fact] public void TestMethod() {{ }}
				}}
				""";

			await Verify.VerifyAnalyzerV2(string.Format(sourceTemplate, "Xunit.Abstractions.ITestOutputHelper"));
			await Verify.VerifyAnalyzerV3(string.Format(sourceTemplate, "Xunit.ITestContextAccessor"));
		}

		[Fact]
		public async Task MissingClassFixture_Triggers()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class ClassFixture { }
				public class CollectionFixture { }

				[CollectionDefinition(nameof(TestCollection))]
				public class TestCollection : ICollectionFixture<CollectionFixture> { }

				[Collection(nameof(TestCollection))]
				public class TestClass {
				    public TestClass(ClassFixture [|_1|], CollectionFixture _2) { }

				    [Fact] public void TestMethod() { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task MissingCollectionFixture_Triggers()
		{
			var source = /* lang=c#-test */ """
				using Xunit;

				public class ClassFixture { }
				public class CollectionFixture { }

				[CollectionDefinition(nameof(TestCollection))]
				public class TestCollection { }

				[Collection(nameof(TestCollection))]
				public class TestClass : IClassFixture<ClassFixture> {
				    public TestClass(ClassFixture _1, CollectionFixture [|_2|]) { }

				    [Fact] public void TestMethod() { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}
	}
}
