using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.EnsureFixturesHaveASource>;

public class EnsureFixturesHaveASourceTests
{
	public class NonTestClass
	{
		[Fact]
		public async void DoesNotTrigger()
		{
			var source = @"
public class NonTestClass {
    public NonTestClass(object _) { }
}";

			await Verify.VerifyAnalyzer(source);
		}
	}

	public class SupportedNonFixtureData
	{
		[Theory]
		[InlineData("")]
		[InlineData("[Collection(\"TestCollection\")]")]
		public async void V2SupportedTypes(string attribute)
		{
			var source = $@"
using Xunit;
using Xunit.Abstractions;

{attribute} public class TestClass {{
    public TestClass(ITestOutputHelper _) {{ }}

    [Fact] public void TestMethod() {{ }}
}}";

			await Verify.VerifyAnalyzerV2(source);
		}

		[Theory]
		[InlineData("")]
		[InlineData("[Collection(\"TestCollection\")]")]
		public async void V3SupportedTypes(string attribute)
		{
			// TODO: This will need to be updated when v3 names are finalized
			var source = $@"
using Xunit;
using Xunit.v3;

{attribute} public class TestClass {{
    public TestClass(_ITestOutputHelper _1, ITestContextAccessor _2) {{ }}

    [Fact] public void TestMethod() {{ }}
}}";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Fact]
		public async void OptionalParameter_DoesNotTrigger()
		{
			var source = @"
using Xunit;

public class TestClass {
    public TestClass(bool value = true) { }

    [Fact] public void TestMethod() { }
}";

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
		public async void SupportsDerivation(
			string baseAttribute,
			string baseInterface,
			string derivedAttribute,
			string derivedInterface)
		{
			var source = @$"
using Xunit;

{baseAttribute}
public abstract class BaseClass {baseInterface} {{
}}

{derivedAttribute}
public class TestClass : BaseClass {derivedInterface} {{
    public TestClass(object _) {{ }}

    [Fact] public void TestMethod() {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void ClassFixtureOnCollectionDefinition_DoesNotTrigger()
		{
			var source = @"
using Xunit;

[CollectionDefinition(nameof(TestCollection))]
public class TestCollection : IClassFixture<object> { }

[Collection(nameof(TestCollection))]
public class TestClass {
    public TestClass(object _) { }

    [Fact] public void TestMethod() { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void MissingClassFixtureDefinition_Triggers()
		{
			var source = @"
using Xunit;

public class TestClass {
    public TestClass(object _) { }

    [Fact] public void TestMethod() { }
}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(5, 29, 5, 30)
					.WithArguments("_");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class CollectionFixtures
	{
		[Theory]
		[InlineData("")]
		[InlineData("[CollectionDefinition(nameof(TestCollection))]")]
		public async void NoFixture_DoesNotTrigger(string definitionAttribute)
		{
			var source = $@"
using Xunit;

{definitionAttribute}
public class TestCollection {{ }}

[Collection(nameof(TestCollection))]
public class TestClass {{
    [Fact] public void TestMethod() {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void WithInheritedFixture_DoesNotTrigger()
		{
			var source = @"
using Xunit;

public class Fixture { }

[CollectionDefinition(""test"")]
public class TestCollection : ICollectionFixture<Fixture> { }

[Collection(""test"")]
public abstract class TestContext {
    protected TestContext(Fixture fixture) { }
}

public class TestClass : TestContext {
    public TestClass(Fixture fixture) : base(fixture) { }

    [Fact]
    public void TestMethod() { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("[Collection(nameof(TestCollection))]", "")]
		[InlineData("", "[Collection(nameof(TestCollection))]")]
		public async void WithFixture_SupportsDerivation(
			string baseAttribute,
			string derivedAttribute)
		{
			var source = @$"
using Xunit;

[CollectionDefinition(nameof(TestCollection))]
public class TestCollection : ICollectionFixture<object> {{ }}

{baseAttribute}
public abstract class BaseClass {{
}}

{derivedAttribute}
public class TestClass : BaseClass {{
    public TestClass(object _) {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void WithFixture_WithDefinition_DoesNotTrigger()
		{
			var source = @"
using Xunit;

[CollectionDefinition(nameof(TestCollection))]
public class TestCollection : ICollectionFixture<object> { }

[Collection(nameof(TestCollection))]
public class TestClass {
    public TestClass(object _) { }

    [Fact] public void TestMethod() { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[InlineData("")]
		[InlineData("[CollectionDefinition(nameof(TestCollection))]")]
		public async void WithFixture_WithoutCollectionFixtureInterface_Triggers(string definitionAttribute)
		{
			var source = @$"
using Xunit;

{definitionAttribute}
public class TestCollection {{ }}

[Collection(nameof(TestCollection))]
public class TestClass {{
    public TestClass(object _) {{ }}

    [Fact] public void TestMethod() {{ }}
}}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(9, 29, 9, 30)
					.WithArguments("_");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public class AssemblyFixtures
	{
		[Fact]
		public async void WithAssemblyFixture_DoesNotTrigger()
		{
			var source = @"
using Xunit;

[assembly: AssemblyFixture(typeof(object))]

public class TestClass {
    public TestClass(object _) { }

    [Fact] public void TestMethod() { }
}";

			await Verify.VerifyAnalyzerV3(source);
		}
	}

	public class MixedFixtures
	{
		[Theory]
		[InlineData("")]
		[InlineData("[CollectionDefinition(nameof(TestCollection))]")]
		public async void WithClassFixture_WithCollection_DoesNotTrigger(string definitionAttribute)
		{
			var source = $@"
using Xunit;

{definitionAttribute}
public class TestCollection {{ }}

[Collection(nameof(TestCollection))]
public class TestClass : IClassFixture<object> {{
    public TestClass(object _) {{ }}

    [Fact] public void TestMethod() {{ }}
}}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void WithMixedClassAndCollectionFixture_AndSupportedNonFixture_DoesNotTrigger()
		{
			var source = @"
using Xunit;

public class ClassFixture {{ }}
public class CollectionFixture {{ }}

[CollectionDefinition(nameof(TestCollection))]
public class TestCollection : ICollectionFixture<CollectionFixture> {{ }}

[Collection(nameof(TestCollection))]
public class TestClass : IClassFixture<ClassFixture> {{
    public TestClass(ClassFixture _1, CollectionFixture _2, {0} _3) {{ }}

    [Fact] public void TestMethod() {{ }}
}}";

			await Verify.VerifyAnalyzerV2(string.Format(source, "Xunit.Abstractions.ITestOutputHelper"));
			await Verify.VerifyAnalyzerV3(string.Format(source, "Xunit.ITestContextAccessor"));
		}

		[Fact]
		public async void MissingClassFixture_Triggers()
		{
			var source = @"
using Xunit;

public class ClassFixture { }
public class CollectionFixture { }

[CollectionDefinition(nameof(TestCollection))]
public class TestCollection : ICollectionFixture<CollectionFixture> { }

[Collection(nameof(TestCollection))]
public class TestClass {
    public TestClass(ClassFixture _1, CollectionFixture _2) { }

    [Fact] public void TestMethod() { }
}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(12, 35, 12, 37)
					.WithArguments("_1");

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Fact]
		public async void MissingCollectionFixture_Triggers()
		{
			var source = @"
using Xunit;

public class ClassFixture { }
public class CollectionFixture { }

[CollectionDefinition(nameof(TestCollection))]
public class TestCollection { }

[Collection(nameof(TestCollection))]
public class TestClass : IClassFixture<ClassFixture> {
    public TestClass(ClassFixture _1, CollectionFixture _2) { }

    [Fact] public void TestMethod() { }
}";
			var expected =
				Verify
					.Diagnostic()
					.WithSpan(12, 57, 12, 59)
					.WithArguments("_2");

			await Verify.VerifyAnalyzer(source, expected);
		}
	}
}
