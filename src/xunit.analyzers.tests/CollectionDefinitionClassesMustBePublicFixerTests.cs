using Verify = Xunit.Analyzers.CSharpVerifier<Xunit.Analyzers.CollectionDefinitionClassesMustBePublic>;

namespace Xunit.Analyzers
{
	public class CollectionDefinitionClassesMustBePublicFixerTests
	{
		[Theory]
		[InlineData("")]
		[InlineData("internal ")]
		public async void MakesClassPublic(string nonPublicAccessModifier)
		{
			var source = $@"
[Xunit.CollectionDefinition(""MyCollection"")]
{nonPublicAccessModifier}class [|CollectionDefinitionClass|] {{ }}";
			var fixedSource = @"
[Xunit.CollectionDefinition(""MyCollection"")]
public class CollectionDefinitionClass { }";

			await Verify.VerifyCodeFixAsync(source, fixedSource);
		}

		[Theory]
		[InlineData("")]
		[InlineData("internal ")]
		public async void ForPartialClassDeclarations_MakesSingleDeclarationPublic(string nonPublicAccessModifier)
		{
			var source = $@"
[Xunit.CollectionDefinition(""MyCollection"")]
{nonPublicAccessModifier}partial class [|CollectionDefinitionClass|] {{ }}
partial class CollectionDefinitionClass {{ }}";
			var fixedSource = @"
[Xunit.CollectionDefinition(""MyCollection"")]
public partial class CollectionDefinitionClass { }
partial class CollectionDefinitionClass { }";

			await Verify.VerifyCodeFixAsync(source, fixedSource);
		}
	}
}
