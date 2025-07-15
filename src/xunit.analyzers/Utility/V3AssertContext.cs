using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V3AssertContext : IAssertContext
{
	internal static readonly Version Version_0_6_0 = new("0.6.0");
	internal static readonly Version Version_3_0_1 = new("3.0.1");

	readonly Lazy<INamedTypeSymbol?> lazyAssertType;

	V3AssertContext(
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
	public bool SupportsAssertNullWithPointers =>
		Version >= Version_3_0_1;

	/// <inheritdoc/>
	public bool SupportsInexactTypeAssertions =>
		Version >= Version_0_6_0;

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

		return version is null ? null : new(compilation, version);
	}
}
