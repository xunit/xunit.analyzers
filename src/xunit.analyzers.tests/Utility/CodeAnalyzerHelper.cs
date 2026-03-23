using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Testing;

#if !NET472
using System.IO;
#endif

static class CodeAnalyzerHelper
{
	public static readonly ReferenceAssemblies CurrentXunitV2;
	public static readonly ReferenceAssemblies CurrentXunitV2RunnerUtility;
	public static readonly ReferenceAssemblies CurrentXunitV3;
	public static readonly ReferenceAssemblies CurrentXunitV3RunnerUtility;

#if NETCOREAPP && ROSLYN_LATEST
	public static readonly ReferenceAssemblies CurrentXunitV3Aot;
	public static readonly ReferenceAssemblies CurrentXunitV3RunnerUtilityAot;
#endif

	// When changing any references here, make sure to update xunit.analyzers.tests.csproj.
	// We either need a direct reference (like xunit.core) or a package download (like everything else)
	// in order for this list to work most efficiently.
	static CodeAnalyzerHelper()
	{
#if NET472
		var defaultAssemblies = ReferenceAssemblies.NetFramework.Net472.Default;
#else
		// Can't use ReferenceAssemblies.Net.Net80 because it's too new for Microsoft.CodeAnalysis 3.11.0
		var defaultAssemblies =
			new ReferenceAssemblies(
				"net8.0",
				new PackageIdentity("Microsoft.NETCore.App.Ref", "8.0.25"),
				Path.Combine("ref", "net8.0")
			);

		var defaultNet90Assemblies =
			new ReferenceAssemblies(
				"net9.0",
				new PackageIdentity("Microsoft.NETCore.App.Ref", "9.0.14"),
				Path.Combine("ref", "net9.0")
			);
#endif

		CurrentXunitV2 = defaultAssemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "6.0.0"),
				new PackageIdentity("Microsoft.Extensions.Primitives", "6.0.0"),
				new PackageIdentity("System.Collections.Immutable", "1.6.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("xunit.abstractions", "2.0.3"),
				new PackageIdentity("xunit.assert", "2.9.4-pre.6"),
				new PackageIdentity("xunit.core", "2.9.4-pre.6")
			)
		);

		CurrentXunitV2RunnerUtility = defaultAssemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "6.0.0"),
				new PackageIdentity("Microsoft.Extensions.Primitives", "6.0.0"),
				new PackageIdentity("System.Collections.Immutable", "1.6.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("xunit.abstractions", "2.0.3"),
				new PackageIdentity("xunit.runner.utility", "2.9.4-pre.6")
			)
		);

		CurrentXunitV3 = defaultAssemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "6.0.0"),
				new PackageIdentity("Microsoft.Extensions.Primitives", "6.0.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("xunit.v3.assert", "4.0.0-pre.25"),
				new PackageIdentity("xunit.v3.common", "4.0.0-pre.25"),
				new PackageIdentity("xunit.v3.extensibility.core", "4.0.0-pre.25"),
				new PackageIdentity("xunit.v3.runner.common", "4.0.0-pre.25")
			)
		);

		CurrentXunitV3RunnerUtility = defaultAssemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "6.0.0"),
				new PackageIdentity("Microsoft.Extensions.Primitives", "6.0.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("xunit.v3.common", "4.0.0-pre.25"),
				new PackageIdentity("xunit.v3.runner.utility", "4.0.0-pre.25")
			)
		);

#if NETCOREAPP && ROSLYN_LATEST

		CurrentXunitV3Aot = defaultNet90Assemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "6.0.0"),
				new PackageIdentity("Microsoft.Extensions.Primitives", "6.0.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("xunit.v3.assert.aot", "4.0.0-pre.25"),
				new PackageIdentity("xunit.v3.common.aot", "4.0.0-pre.25"),
				new PackageIdentity("xunit.v3.extensibility.core.aot", "4.0.0-pre.25"),
				new PackageIdentity("xunit.v3.runner.common.aot", "4.0.0-pre.25")
			)
		);

		CurrentXunitV3RunnerUtilityAot = defaultNet90Assemblies.AddPackages(
			ImmutableArray.Create(
				new PackageIdentity("Microsoft.Bcl.AsyncInterfaces", "6.0.0"),
				new PackageIdentity("Microsoft.Extensions.Primitives", "6.0.0"),
				new PackageIdentity("System.Threading.Tasks.Extensions", "4.5.4"),
				new PackageIdentity("xunit.v3.common.aot", "4.0.0-pre.25"),
				new PackageIdentity("xunit.v3.runner.utility.aot", "4.0.0-pre.25")
			)
		);

#endif  // NETCOREAPP && ROSLYN_LATEST
	}
}
