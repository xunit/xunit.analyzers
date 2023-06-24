using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Testing;

static class CodeAnalyzerHelper
{
	public static readonly ReferenceAssemblies CurrentXunitV2;

	public static readonly ReferenceAssemblies CurrentXunitV3;

	static CodeAnalyzerHelper()
	{
		CurrentXunitV2 = ReferenceAssemblies.Default.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("System.Collections.Immutable", "1.6.0"),
				new PackageIdentity("xunit.abstractions", "2.0.3"),
				new PackageIdentity("xunit.assert", "2.5.0-pre.38"),
				new PackageIdentity("xunit.core", "2.5.0-pre.38")
			)
		);

		CurrentXunitV3 = ReferenceAssemblies.Default.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("xunit.v3.core", "1.0.0")
			)
		);
	}
}
