using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
	public class CoreV3Context
	{
		public CoreV3Context(
			Compilation compilation,
			Version versionOverride = null)
		{
			Version =
				versionOverride ??
				compilation
					.ReferencedAssemblyNames
					.FirstOrDefault(a => a.Name.Equals("xunit.v3.core", StringComparison.OrdinalIgnoreCase))
					?.Version;
		}

		public Version Version { get; set; }
	}
}
