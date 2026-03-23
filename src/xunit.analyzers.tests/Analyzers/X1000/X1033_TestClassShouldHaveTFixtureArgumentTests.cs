using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TestClassShouldHaveTFixtureArgument>;

public class X1033_TestClassShouldHaveTFixtureArgumentTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class FixtureData { }

			// Class fixtures on class

			public class {|#0:Fact_ForClassWithIClassFixtureWithoutConstructorArg_Triggers|}: IClassFixture<FixtureData> {
				[Fact]
				public void TestMethod() { }
			}

			public class {|#1:Theory_ForClassWithIClassFixtureWithoutConstructorArg_Triggers|}: IClassFixture<FixtureData> {
				[Theory]
				public void TestMethod() { }
			}

			public class Fact_ForClassWithIClassFixtureWithConstructorArg_DoesNotTrigger: IClassFixture<FixtureData> {
				public Fact_ForClassWithIClassFixtureWithConstructorArg_DoesNotTrigger(FixtureData fixtureData) { }

				[Fact]
				public void TestMethod() { }
			}

			public class Theory_ForClassWithIClassFixtureWithConstructorArg_DoesNotTrigger: IClassFixture<FixtureData> {
				public Theory_ForClassWithIClassFixtureWithConstructorArg_DoesNotTrigger(FixtureData fixtureData) { }

				[Theory]
				public void TestMethod() { }
			}

			// Class fixtures on collection

			[CollectionDefinition(nameof(ClassFixtureCollection))]
			public class ClassFixtureCollection : ICollectionFixture<FixtureData> { }

			[Collection(nameof(ClassFixtureCollection))]
			public class {|#10:Fact_ForClassWithIClassFixtureOnCollectionWithoutConstructorArg_Triggers|} {
				[Fact]
				public void TestMethod() { }
			}

			[Collection(nameof(ClassFixtureCollection))]
			public class {|#11:Theory_ForClassWithIClassFixtureOnCollectionWithoutConstructorArg_Triggers|} {
				[Theory]
				public void TestMethod() { }
			}

			[Collection(nameof(ClassFixtureCollection))]
			public class Fact_ForClassWithIClassFixtureOnCollectionWithConstructorArg_DoesNotTrigger {
				public Fact_ForClassWithIClassFixtureOnCollectionWithConstructorArg_DoesNotTrigger(FixtureData fixtureData) { }

				[Fact]
				public void TestMethod() { }
			}

			[Collection(nameof(ClassFixtureCollection))]
			public class Theory_ForClassWithIClassFixtureOnCollectionWithConstructorArg_DoesNotTrigger {
				public Theory_ForClassWithIClassFixtureOnCollectionWithConstructorArg_DoesNotTrigger(FixtureData fixtureData) { }

				[Theory]
				public void TestMethod() { }
			}

			// Collection fixtures

			[CollectionDefinition(nameof(CollectionFixtureCollection))]
			public class CollectionFixtureCollection : ICollectionFixture<FixtureData> { }

			[Collection(nameof(CollectionFixtureCollection))]
			public class {|#20:Fact_ForClassWithICollectionFixtureWithoutConstructorArg_Triggers|} {
				[Fact]
				public void TestMethod() { }
			}

			[Collection(nameof(CollectionFixtureCollection))]
			public class {|#21:Theory_ForClassWithICollectionFixtureWithoutConstructorArg_Triggers|} {
				[Theory]
				public void TestMethod() { }
			}

			[Collection(nameof(CollectionFixtureCollection))]
			public class Fact_ForClassWithICollectionFixtureWithConstructorArg_DoesNotTrigger {
				public Fact_ForClassWithICollectionFixtureWithConstructorArg_DoesNotTrigger(FixtureData fixtureData) { }

				[Fact]
				public void TestMethod() { }
			}

			[Collection(nameof(CollectionFixtureCollection))]
			public class Theory_ForClassWithICollectionFixtureWithConstructorArg_DoesNotTrigger {
				public Theory_ForClassWithICollectionFixtureWithConstructorArg_DoesNotTrigger(FixtureData fixtureData) { }

				[Theory]
				public void TestMethod() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Fact_ForClassWithIClassFixtureWithoutConstructorArg_Triggers", "FixtureData"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Theory_ForClassWithIClassFixtureWithoutConstructorArg_Triggers", "FixtureData"),

			// TODO: These should trigger, but they don't right now

			// Verify.Diagnostic().WithLocation(10).WithArguments("Fact_ForClassWithIClassFixtureOnCollectionWithoutConstructorArg_Triggers", "FixtureData"),
			// Verify.Diagnostic().WithLocation(11).WithArguments("Theory_ForClassWithIClassFixtureOnCollectionWithoutConstructorArg_Triggers", "FixtureData"),

			// Verify.Diagnostic().WithLocation(20).WithArguments("Fact_ForClassWithICollectionFixtureWithoutConstructorArg_Triggers", "FixtureData"),
			// Verify.Diagnostic().WithLocation(21).WithArguments("Theory_ForClassWithICollectionFixtureWithoutConstructorArg_Triggers", "FixtureData"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class FixtureData { }

			// Class fixtures on class

			public class {|#0:Fact_ForClassWithIClassFixtureWithoutConstructorArg_Triggers|}: IClassFixture<FixtureData> {
				[CulturedFact(new[] { "en-US" })]
				public void TestMethod() { }
			}

			public class {|#1:Theory_ForClassWithIClassFixtureWithoutConstructorArg_Triggers|}: IClassFixture<FixtureData> {
				[CulturedTheory(new[] { "en-US" })]
				public void TestMethod() { }
			}

			public class Fact_ForClassWithIClassFixtureWithConstructorArg_DoesNotTrigger: IClassFixture<FixtureData> {
				public Fact_ForClassWithIClassFixtureWithConstructorArg_DoesNotTrigger(FixtureData fixtureData) { }

				[CulturedFact(new[] { "en-US" })]
				public void TestMethod() { }
			}

			public class Theory_ForClassWithIClassFixtureWithConstructorArg_DoesNotTrigger: IClassFixture<FixtureData> {
				public Theory_ForClassWithIClassFixtureWithConstructorArg_DoesNotTrigger(FixtureData fixtureData) { }

				[CulturedTheory(new[] { "en-US" })]
				public void TestMethod() { }
			}

			// Class fixtures on collection

			[CollectionDefinition(nameof(ClassFixtureCollection))]
			public class ClassFixtureCollection : ICollectionFixture<FixtureData> { }

			[Collection(nameof(ClassFixtureCollection))]
			public class {|#10:Fact_ForClassWithIClassFixtureOnCollectionWithoutConstructorArg_Triggers|} {
				[CulturedFact(new[] { "en-US" })]
				public void TestMethod() { }
			}

			[Collection(nameof(ClassFixtureCollection))]
			public class {|#11:Theory_ForClassWithIClassFixtureOnCollectionWithoutConstructorArg_Triggers|} {
				[CulturedTheory(new[] { "en-US" })]
				public void TestMethod() { }
			}

			[Collection(nameof(ClassFixtureCollection))]
			public class Fact_ForClassWithIClassFixtureOnCollectionWithConstructorArg_DoesNotTrigger {
				public Fact_ForClassWithIClassFixtureOnCollectionWithConstructorArg_DoesNotTrigger(FixtureData fixtureData) { }

				[CulturedFact(new[] { "en-US" })]
				public void TestMethod() { }
			}

			[Collection(nameof(ClassFixtureCollection))]
			public class Theory_ForClassWithIClassFixtureOnCollectionWithConstructorArg_DoesNotTrigger {
				public Theory_ForClassWithIClassFixtureOnCollectionWithConstructorArg_DoesNotTrigger(FixtureData fixtureData) { }

				[CulturedTheory(new[] { "en-US" })]
				public void TestMethod() { }
			}

			// Collection fixtures

			[CollectionDefinition(nameof(CollectionFixtureCollection))]
			public class CollectionFixtureCollection : ICollectionFixture<FixtureData> { }

			[Collection(nameof(CollectionFixtureCollection))]
			public class {|#20:Fact_ForClassWithICollectionFixtureWithoutConstructorArg_Triggers|} {
				[CulturedFact(new[] { "en-US" })]
				public void TestMethod() { }
			}

			[Collection(nameof(CollectionFixtureCollection))]
			public class {|#21:Theory_ForClassWithICollectionFixtureWithoutConstructorArg_Triggers|} {
				[CulturedTheory(new[] { "en-US" })]
				public void TestMethod() { }
			}

			[Collection(nameof(CollectionFixtureCollection))]
			public class Fact_ForClassWithICollectionFixtureWithConstructorArg_DoesNotTrigger {
				public Fact_ForClassWithICollectionFixtureWithConstructorArg_DoesNotTrigger(FixtureData fixtureData) { }

				[CulturedFact(new[] { "en-US" })]
				public void TestMethod() { }
			}

			[Collection(nameof(CollectionFixtureCollection))]
			public class Theory_ForClassWithICollectionFixtureWithConstructorArg_DoesNotTrigger {
				public Theory_ForClassWithICollectionFixtureWithConstructorArg_DoesNotTrigger(FixtureData fixtureData) { }

				[CulturedTheory(new[] { "en-US" })]
				public void TestMethod() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("Fact_ForClassWithIClassFixtureWithoutConstructorArg_Triggers", "FixtureData"),
			Verify.Diagnostic().WithLocation(1).WithArguments("Theory_ForClassWithIClassFixtureWithoutConstructorArg_Triggers", "FixtureData"),

			// TODO: These should trigger, but they don't right now

			// Verify.Diagnostic().WithLocation(10).WithArguments("Fact_ForClassWithIClassFixtureOnCollectionWithoutConstructorArg_Triggers", "FixtureData"),
			// Verify.Diagnostic().WithLocation(11).WithArguments("Theory_ForClassWithIClassFixtureOnCollectionWithoutConstructorArg_Triggers", "FixtureData"),

			// Verify.Diagnostic().WithLocation(20).WithArguments("Fact_ForClassWithICollectionFixtureWithoutConstructorArg_Triggers", "FixtureData"),
			// Verify.Diagnostic().WithLocation(21).WithArguments("Theory_ForClassWithICollectionFixtureWithoutConstructorArg_Triggers", "FixtureData"),
		};

		await Verify.VerifyAnalyzerV3(source, expected);
	}
}
