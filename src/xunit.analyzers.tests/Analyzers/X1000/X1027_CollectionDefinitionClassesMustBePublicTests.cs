using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.CollectionDefinitionClassesMustBePublic>;

public class X1027_CollectionDefinitionClassesMustBePublicTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			[CollectionDefinition("Public")]
			public class PublicCollectionDefinitionClass { }

			[CollectionDefinition("Private")]
			class [|PrivateCollectionDefinitionClass|] { }

			[CollectionDefinition("Internal")]
			internal class [|InternalCollectionDefinitionClass|] { }

			// Public partials

			[CollectionDefinition("PublicPartials")]
			public partial class CollectionDefinitionClass { }
			partial class CollectionDefinitionClass { }

			// Non-public partials

			[CollectionDefinition("PrivatePartials")]
			partial class {|#0:CollectionDefinitionClass1|} { }
			partial class {|#1:CollectionDefinitionClass1|} { }

			[CollectionDefinition("PrivateInternalPartials")]
			partial class {|#10:CollectionDefinitionClass2|} { }
			internal partial class {|#11:CollectionDefinitionClass2|} { }

			[CollectionDefinition("InternalPartials")]
			internal partial class {|#20:CollectionDefinitionClass3|} { }
			internal partial class {|#21:CollectionDefinitionClass3|} { }
			""";
		var expected = new[] {
			Verify.Diagnostic().WithLocation(0).WithLocation(1),
			Verify.Diagnostic().WithLocation(10).WithLocation(11),
			Verify.Diagnostic().WithLocation(20).WithLocation(21),
		};

		await Verify.VerifyAnalyzer(source, expected);
	}
}
