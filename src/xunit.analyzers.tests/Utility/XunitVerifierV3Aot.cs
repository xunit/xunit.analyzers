using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Testing.Verifiers;

public class XunitVerifierV3Aot : XunitVerifier
{
	public XunitVerifierV3Aot() :
		base(ImmutableStack.Create("Testing against xUnit.net v3 [Native AOT]"))
	{ }
}
