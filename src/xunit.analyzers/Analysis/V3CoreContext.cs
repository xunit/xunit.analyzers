using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
	public class V3CoreContext
	{
		V3CoreContext(
			Compilation compilation,
			Version version)
		{
			Version = version;
		}

		public Version Version { get; set; }

		public static V3CoreContext? Get(
			Compilation compilation,
			Version? versionOverride = null)
		{
			var version =
				versionOverride ??
				compilation
					.ReferencedAssemblyNames
					.FirstOrDefault(a => a.Name.Equals("xunit.v3.core", StringComparison.OrdinalIgnoreCase))
					?.Version;

			return version is null ? null : new(compilation, version);
		}
	}
}
