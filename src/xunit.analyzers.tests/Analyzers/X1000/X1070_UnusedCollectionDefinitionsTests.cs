using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.UnusedCollectionDefinitions>;

public class X1070_UnusedCollectionDefinitionsTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			[CollectionDefinition("used by test class")]
			public class UsedByTestClassCollection { }

			[Collection("used by test class")]
			public class TestClass1 {
				[Fact]
				public void TestMethod() { }
			}

			[CollectionDefinition("used by non-test class")]
			public class UsedByNonTestClassCollection { }

			[Collection("used by non-test class")]
			public class NonTestClass { }

			[CollectionDefinition("used via constant")]
			public class UsedViaConstantCollection {
				public const string Name = "used via constant";
			}

			[Collection(UsedViaConstantCollection.Name)]
			public class TestClass2 {
				[Fact]
				public void TestMethod() { }
			}

			[CollectionDefinition("used on base class")]
			public class UsedOnBaseClassCollection { }

			[Collection("used on base class")]
			public abstract class BaseTestClass { }

			public class DerivedTestClass : BaseTestClass {
				[Fact]
				public void TestMethod() { }
			}

			[CollectionDefinition("unused")]
			public class {|#0:UnusedCollection|} { }

			[CollectionDefinition("misspelled")]
			public class {|#1:MisspelledCollection|} { }

			[Collection("mispelled")]
			public class TestClass3 {
				[Fact]
				public void TestMethod() { }
			}
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("unused"),
			Verify.Diagnostic().WithLocation(1).WithArguments("misspelled"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask V2_and_V3_CustomCollectionBehavior_DoesNotTrigger()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

			[CollectionDefinition("unused")]
			public class UnusedCollection { }
			""";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async ValueTask V2_and_V3_DisabledParallelization_Triggers()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			[assembly: CollectionBehavior(DisableTestParallelization = true)]

			[CollectionDefinition("unused")]
			public class {|#0:UnusedCollection|} { }
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("unused"),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Fact]
	public async ValueTask V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			[CollectionDefinition]
			public class UsedByTypeCollection { }

			[Collection(typeof(UsedByTypeCollection))]
			public class TestClass1 {
				[Fact]
				public void TestMethod() { }
			}

			[CollectionDefinition]
			public class UsedByGenericAttributeCollection { }

			[Collection<UsedByGenericAttributeCollection>]
			public class TestClass2 {
				[Fact]
				public void TestMethod() { }
			}

			[CollectionDefinition("named, used by type")]
			public class NamedUsedByTypeCollection { }

			[Collection(typeof(NamedUsedByTypeCollection))]
			public class TestClass3 {
				[Fact]
				public void TestMethod() { }
			}

			[CollectionDefinition]
			public class {|#0:UnusedCollection|} { }
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithArguments("UnusedCollection"),
		};

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp11, source, expected);
	}
}
