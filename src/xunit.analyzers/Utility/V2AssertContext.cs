using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V2AssertContext : IAssertContext
{
	internal static readonly Version Version_2_5_0 = new("2.5.0");

	readonly Lazy<INamedTypeSymbol?> lazyAssertType;

	V2AssertContext(
		Compilation compilation,
		Version version)
	{
		Version = version;

		lazyAssertType = new(() => TypeSymbolFactory.Assert(compilation));
	}

	/// <inheritdoc/>
	public INamedTypeSymbol? AssertType =>
		lazyAssertType.Value;

	/// <inheritdoc/>
	public bool SupportsAssertFail =>
		Version >= Version_2_5_0;

	/// <inheritdoc/>
	public Version Version { get; }

	public static V2AssertContext? Get(
		Compilation compilation,
		Version? versionOverride = null)
	{
		Guard.ArgumentNotNull(compilation);

		var version =
			versionOverride ??
			compilation
				.ReferencedAssemblyNames
				.FirstOrDefault(a => a.Name.Equals("xunit.assert", StringComparison.OrdinalIgnoreCase) || a.Name.Equals("xunit.assert.source", StringComparison.OrdinalIgnoreCase))
				?.Version;

		return version is null ? null : new(compilation, version);
	}
}
