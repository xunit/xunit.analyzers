using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Testing;

static class CodeAnalyzerHelper
{
	public static readonly ReferenceAssemblies CurrentXunitV2;

	public static readonly ReferenceAssemblies CurrentXunitV3;

	// When changing any references here, make sure to update xunit.analzyers.tests.csproj.
	// We either need a direct reference (like xunit.core) or a package download (like everything else)
	// in order for this list to work most efficiently.
	static CodeAnalyzerHelper()
	{
		CurrentXunitV2 = ReferenceAssemblies.Default.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("System.Collections.Immutable", "1.6.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("xunit.abstractions", "2.0.3"),
				new PackageIdentity("xunit.assert", "2.5.0-pre.38"),
				new PackageIdentity("xunit.core", "2.5.0-pre.38")
			)
		);

		CurrentXunitV3 = ReferenceAssemblies.Default.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "7.0.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("System.Text.Json", "7.0.2"),
				new PackageIdentity("xunit.v3.assert", "0.1.1-pre.248"),
				new PackageIdentity("xunit.v3.common", "0.1.1-pre.248"),
				new PackageIdentity("xunit.v3.extensibility.core", "0.1.1-pre.248")
			)
		);
	}
}
