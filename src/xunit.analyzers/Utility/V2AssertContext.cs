using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V2AssertContext : IAssertContext
{
	internal static readonly Version Version_2_5_0 = new("2.5.0");

	V2AssertContext(Version version)
	{
		Version = version;
	}

	/// <inheritdoc/>
	public bool SupportsAssertFail =>
		Version >= Version_2_5_0;

	/// <inheritdoc/>
	public Version Version { get; }

	public static V2AssertContext? Get(
		Compilation compilation,
		Version? versionOverride = null)
	{
		var version =
			versionOverride ??
			compilation
				.ReferencedAssemblyNames
				.FirstOrDefault(a => a.Name.Equals("xunit.assert", StringComparison.OrdinalIgnoreCase) || a.Name.Equals("xunit.assert.source", StringComparison.OrdinalIgnoreCase))
				?.Version;

		return version is null ? null : new(version);
	}
}
