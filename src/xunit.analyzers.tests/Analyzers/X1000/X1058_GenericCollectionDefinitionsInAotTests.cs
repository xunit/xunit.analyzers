using System.Threading.Tasks;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.GenericCollectionDefinitionsInAot>;

public class X1058_GenericCollectionDefinitionsInAotTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class GenericNonCollection<T> { }

			[CollectionDefinition("collection")]
			public class NonGenericCollection { }

			[CollectionDefinition("collection")]
			public class {|#0:GenericCollection|}<T> { }
			""";

		await Verify.VerifyAnalyzerNonAot(source);
#if NETCOREAPP && ROSLYN_LATEST
		await Verify.VerifyAnalyzerV3Aot(source, Verify.Diagnostic().WithLocation(0));
#endif
	}
}
