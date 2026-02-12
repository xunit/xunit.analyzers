using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.CollectionDefinitionClassesMustBePublic>;

public class CollectionDefinitionClassesMustBePublicFixerTests
{
	[Fact]
	public async Task FixAll_MakesAllClassesPublic()
	{
		var before = /* lang=c#-test */ """
			[Xunit.CollectionDefinition("MyCollection1")]
			class [|CollectionDefinitionClass1|] { }

			[Xunit.CollectionDefinition("MyCollection2")]
			internal class [|CollectionDefinitionClass2|] { }
			""";
		var after = /* lang=c#-test */ """
			[Xunit.CollectionDefinition("MyCollection1")]
			public class CollectionDefinitionClass1 { }

			[Xunit.CollectionDefinition("MyCollection2")]
			public class CollectionDefinitionClass2 { }
			""";

		await Verify.VerifyCodeFixFixAll(before, after, CollectionDefinitionClassesMustBePublicFixer.Key_MakeCollectionDefinitionClassPublic);
	}

	[Fact]
	public async Task ForPartialClassDeclarations_MakesSingleDeclarationPublic()
	{
		var before = /* lang=c#-test */ """
			[Xunit.CollectionDefinition("MyCollection")]
			partial class [|CollectionDefinitionClass|] { }

			partial class CollectionDefinitionClass { }
			""";
		var after = /* lang=c#-test */ """
			[Xunit.CollectionDefinition("MyCollection")]
			public partial class CollectionDefinitionClass { }

			partial class CollectionDefinitionClass { }
			""";

		await Verify.VerifyCodeFixFixAll(before, after, CollectionDefinitionClassesMustBePublicFixer.Key_MakeCollectionDefinitionClassPublic);
	}
}
