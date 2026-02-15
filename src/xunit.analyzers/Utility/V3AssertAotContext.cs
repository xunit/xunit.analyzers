using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V3AssertAotContext : IAssertContextV3
{
	readonly Lazy<INamedTypeSymbol?> lazyAssertType;

	V3AssertAotContext(
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
	public bool SupportsAssertFail => true;

	/// <inheritdoc/>
	public bool SupportsAssertNullWithPointers => true;

	/// <inheritdoc/>
	public bool SupportsInexactTypeAssertions => true;

	/// <inheritdoc/>
	public Version Version { get; }

	public static IAssertContextV3? Get(
		Compilation compilation,
		Version? versionOverride = null)
	{
		Guard.ArgumentNotNull(compilation);

		var version =
			versionOverride ??
			compilation
				.ReferencedAssemblyNames
				.FirstOrDefault(a => a.Name.Equals("xunit.v3.assert.aot", StringComparison.OrdinalIgnoreCase))
				?.Version;

		return version is null ? null : new V3AssertAotContext(compilation, version);
	}
}
