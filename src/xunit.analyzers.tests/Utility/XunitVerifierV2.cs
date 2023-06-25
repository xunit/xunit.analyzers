using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Testing.Verifiers;

public class XunitVerifierV2 : XunitVerifier
{
	public XunitVerifierV2() :
		base(ImmutableStack.Create("Testing against xUnit.net v2"))
	{ }
}
