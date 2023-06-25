using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Testing.Verifiers;

public class XunitVerifierV3 : XunitVerifier
{
	public XunitVerifierV3() :
		base(ImmutableStack.Create("Testing against xUnit.net v3"))
	{ }
}
