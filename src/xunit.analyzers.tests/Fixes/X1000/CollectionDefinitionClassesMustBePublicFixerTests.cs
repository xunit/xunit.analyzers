using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.CollectionDefinitionClassesMustBePublic>;

public class CollectionDefinitionClassesMustBePublicFixerTests
{
	[Theory]
	[InlineData("")]
	[InlineData("internal ")]
	public async Task MakesClassPublic(string modifier)
	{
		var before = string.Format(/* lang=c#-test */ """
			[Xunit.CollectionDefinition("MyCollection")]
			{0}class [|CollectionDefinitionClass|] {{ }}
			""", modifier);
		var after = /* lang=c#-test */ """
			[Xunit.CollectionDefinition("MyCollection")]
			public class CollectionDefinitionClass { }
			""";

		await Verify.VerifyCodeFix(before, after, CollectionDefinitionClassesMustBePublicFixer.Key_MakeCollectionDefinitionClassPublic);
	}

	[Theory]
	[InlineData("")]
	[InlineData("internal ")]
	public async Task ForPartialClassDeclarations_MakesSingleDeclarationPublic(string modifier)
	{
		var before = string.Format(/* lang=c#-test */ """
			[Xunit.CollectionDefinition("MyCollection")]
			{0}partial class [|CollectionDefinitionClass|] {{ }}

			partial class CollectionDefinitionClass {{ }}
			""", modifier);
		var after = /* lang=c#-test */ """
			[Xunit.CollectionDefinition("MyCollection")]
			public partial class CollectionDefinitionClass { }

			partial class CollectionDefinitionClass { }
			""";

		await Verify.VerifyCodeFix(before, after, CollectionDefinitionClassesMustBePublicFixer.Key_MakeCollectionDefinitionClassPublic);
	}
}
