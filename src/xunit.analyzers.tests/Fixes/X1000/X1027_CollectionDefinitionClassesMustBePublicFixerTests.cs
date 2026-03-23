using System.Threading.Tasks;
using Xunit;
using Xunit.Analyzers.Fixes;
using Verify = CSharpVerifier<Xunit.Analyzers.CollectionDefinitionClassesMustBePublic>;

public class X1027_CollectionDefinitionClassesMustBePublicFixerTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var before = /* lang=c#-test */ """
			using Xunit;

			[CollectionDefinition("MyCollection1")]
			class [|ImplicitInternalCollectionClass|] { }

			[CollectionDefinition("MyCollection2")]
			internal class [|ExplicitInternalCollectionClass|] { }

			[CollectionDefinition("MyCollection3")]
			partial class [|PartialCollectionDefinitionClass|] { }

			partial class PartialCollectionDefinitionClass { }
			""";
		var after = /* lang=c#-test */ """
			using Xunit;

			[CollectionDefinition("MyCollection1")]
			public class ImplicitInternalCollectionClass { }

			[CollectionDefinition("MyCollection2")]
			public class ExplicitInternalCollectionClass { }

			[CollectionDefinition("MyCollection3")]
			public partial class PartialCollectionDefinitionClass { }

			partial class PartialCollectionDefinitionClass { }
			""";

		await Verify.VerifyCodeFixFixAll(before, after, CollectionDefinitionClassesMustBePublicFixer.Key_MakeCollectionDefinitionClassPublic);
	}
}
