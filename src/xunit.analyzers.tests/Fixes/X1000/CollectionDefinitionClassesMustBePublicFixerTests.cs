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
		var before = $@"
[Xunit.CollectionDefinition(""MyCollection"")]
{modifier}class [|CollectionDefinitionClass|] {{ }}";

		var after = @"
[Xunit.CollectionDefinition(""MyCollection"")]
public class CollectionDefinitionClass { }";

		await Verify.VerifyCodeFix(before, after, CollectionDefinitionClassesMustBePublicFixer.Key_MakeCollectionDefinitionClassPublic);
	}

	[Theory]
	[InlineData("")]
	[InlineData("internal ")]
	public async Task ForPartialClassDeclarations_MakesSingleDeclarationPublic(string modifier)
	{
		var before = $@"
[Xunit.CollectionDefinition(""MyCollection"")]
{modifier}partial class [|CollectionDefinitionClass|] {{ }}

partial class CollectionDefinitionClass {{ }}";

		var after = @"
[Xunit.CollectionDefinition(""MyCollection"")]
public partial class CollectionDefinitionClass { }

partial class CollectionDefinitionClass { }";

		await Verify.VerifyCodeFix(before, after, CollectionDefinitionClassesMustBePublicFixer.Key_MakeCollectionDefinitionClassPublic);
	}
}
