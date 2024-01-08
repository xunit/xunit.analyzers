using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis.Testing;

static class CodeAnalyzerHelper
{
	public static readonly ReferenceAssemblies CurrentXunitV2;

	public static readonly ReferenceAssemblies CurrentXunitV2RunnerUtility;

	public static readonly ReferenceAssemblies CurrentXunitV3;

	public static readonly ReferenceAssemblies CurrentXunitV3RunnerUtility;

	// When changing any references here, make sure to update xunit.analyzers.tests.csproj.
	// We either need a direct reference (like xunit.core) or a package download (like everything else)
	// in order for this list to work most efficiently.
	static CodeAnalyzerHelper()
	{
#if NET472
		var defaultAssemblies = ReferenceAssemblies.NetFramework.Net472.Default;
#else
		// Can't use ReferenceAssemblies.Net.Net80 because it's too new for Microsoft.CodeAnalysis 4.2.0
		var defaultAssemblies =
			new ReferenceAssemblies(
				"net8.0",
				new PackageIdentity("Microsoft.NETCore.App.Ref", "8.0.0"),
				Path.Combine("ref", "net8.0")
			);
#endif

		CurrentXunitV2 = defaultAssemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("Microsoft.Extensions.Primitives", "8.0.0"),
				new PackageIdentity("System.Collections.Immutable", "1.6.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("xunit.abstractions", "2.0.3"),
				new PackageIdentity("xunit.assert", "2.6.5"),
				new PackageIdentity("xunit.core", "2.6.5")
			)
		);

		CurrentXunitV2RunnerUtility = defaultAssemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("Microsoft.Extensions.Primitives", "8.0.0"),
				new PackageIdentity("System.Collections.Immutable", "1.6.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("xunit.abstractions", "2.0.3"),
				new PackageIdentity("xunit.runner.utility", "2.6.5")
			)
		);

		CurrentXunitV3 = defaultAssemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "8.0.0"),
				new PackageIdentity("Microsoft.Extensions.Primitives", "8.0.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("System.Text.Json", "8.0.0"),
				new PackageIdentity("xunit.v3.assert", "0.1.1-pre.342"),
				new PackageIdentity("xunit.v3.common", "0.1.1-pre.342"),
				new PackageIdentity("xunit.v3.extensibility.core", "0.1.1-pre.342")
			)
		);

		CurrentXunitV3RunnerUtility = defaultAssemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "8.0.0"),
				new PackageIdentity("Microsoft.Extensions.Primitives", "8.0.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("System.Text.Json", "8.0.0"),
				new PackageIdentity("xunit.v3.common", "0.1.1-pre.342"),
				new PackageIdentity("xunit.v3.runner.utility", "0.1.1-pre.342")
			)
		);
	}
}
