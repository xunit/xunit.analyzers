using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V3AssertContext : IAssertContext
{
	V3AssertContext(Version version)
	{
		Version = version;
	}

	/// <inheritdoc/>
	public bool SupportsAssertFail => true;

	/// <inheritdoc/>
	public Version Version { get; }

	public static V3AssertContext? Get(
		Compilation compilation,
		Version? versionOverride = null)
	{
		Guard.ArgumentNotNull(compilation);

		var version =
			versionOverride ??
			compilation
				.ReferencedAssemblyNames
				.FirstOrDefault(a => a.Name.Equals("xunit.v3.assert", StringComparison.OrdinalIgnoreCase) || a.Name.Equals("xunit.v3.assert.source", StringComparison.OrdinalIgnoreCase))
				?.Version;

		return version is null ? null : new(version);
	}
}
