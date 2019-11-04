using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Testing;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace Xunit.Analyzers
{
	internal static class CodeAnalyzerHelper
	{
		internal static ReferenceAssemblies CurrentXunit { get; }

		static CodeAnalyzerHelper()
		{
			CurrentXunit = ReferenceAssemblies.Default.AddPackages(ImmutableArray.Create(
				new PackageIdentity("System.Collections.Immutable", NuGetVersion.Parse("1.6.0")),
				new PackageIdentity("xunit.assert", NuGetVersion.Parse("2.4.1")),
				new PackageIdentity("xunit.core", NuGetVersion.Parse("2.4.1"))));
		}
	}
}
