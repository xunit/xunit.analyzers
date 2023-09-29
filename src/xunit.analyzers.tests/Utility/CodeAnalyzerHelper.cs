using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Testing;

static class CodeAnalyzerHelper
{
	public static readonly ReferenceAssemblies CurrentXunitV2;

	public static readonly ReferenceAssemblies CurrentXunitV2RunnerUtility;

	public static readonly ReferenceAssemblies CurrentXunitV3;

	public static readonly ReferenceAssemblies CurrentXunitV3RunnerUtility;

	// When changing any references here, make sure to update xunit.analzyers.tests.csproj.
	// We either need a direct reference (like xunit.core) or a package download (like everything else)
	// in order for this list to work most efficiently.
	static CodeAnalyzerHelper()
	{
#if NET472
		var defaultAssemblies = ReferenceAssemblies.NetFramework.Net472.Default;
#else
		var defaultAssemblies = ReferenceAssemblies.Net.Net60;
#endif

		CurrentXunitV2 = defaultAssemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("System.Collections.Immutable", "1.6.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("xunit.abstractions", "2.0.3"),
				new PackageIdentity("xunit.assert", "2.5.1"),
				new PackageIdentity("xunit.core", "2.5.1")
			)
		);

		CurrentXunitV2RunnerUtility = defaultAssemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("System.Collections.Immutable", "1.6.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("xunit.abstractions", "2.0.3"),
				new PackageIdentity("xunit.runner.utility", "2.5.1")
			)
		);

		CurrentXunitV3 = defaultAssemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "6.0.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("System.Text.Json", "6.0.0"),
				new PackageIdentity("xunit.v3.assert", "0.1.1-pre.279"),
				new PackageIdentity("xunit.v3.common", "0.1.1-pre.279"),
				new PackageIdentity("xunit.v3.extensibility.core", "0.1.1-pre.279")
			)
		);

		CurrentXunitV3RunnerUtility = defaultAssemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "6.0.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("System.Text.Json", "6.0.0"),
				new PackageIdentity("xunit.v3.common", "0.1.1-pre.279"),
				new PackageIdentity("xunit.v3.runner.utility", "0.1.1-pre.279")
			)
		);
	}
}
