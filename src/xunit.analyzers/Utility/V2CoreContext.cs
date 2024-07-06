using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V2CoreContext : ICoreContext
{
	internal static readonly Version Version_2_2_0 = new("2.2.0");
	internal static readonly Version Version_2_4_0 = new("2.4.0");

	readonly Lazy<INamedTypeSymbol?> lazyClassDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyCollectionAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyCollectionDefinitionAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyFactAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyIClassFixtureType;
	readonly Lazy<INamedTypeSymbol?> lazyICollectionFixtureType;
	readonly Lazy<INamedTypeSymbol?> lazyInlineDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyITestOutputHelperType;
	readonly Lazy<INamedTypeSymbol?> lazyMemberDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyTheoryAttributeType;

	V2CoreContext(
		Compilation compilation,
		Version version)
	{
		Version = version;

		lazyClassDataAttributeType = new(() => TypeSymbolFactory.ClassDataAttribute(compilation));
		lazyCollectionAttributeType = new(() => TypeSymbolFactory.CollectionAttribute(compilation));
		lazyCollectionDefinitionAttributeType = new(() => TypeSymbolFactory.CollectionDefinitionAttribute(compilation));
		lazyDataAttributeType = new(() => TypeSymbolFactory.DataAttribute_V2(compilation));
		lazyFactAttributeType = new(() => TypeSymbolFactory.FactAttribute(compilation));
		lazyIClassFixtureType = new(() => TypeSymbolFactory.IClassFixureOfT(compilation));
		lazyICollectionFixtureType = new(() => TypeSymbolFactory.ICollectionFixtureOfT(compilation));
		lazyInlineDataAttributeType = new(() => TypeSymbolFactory.InlineDataAttribute(compilation));
		lazyITestOutputHelperType = new(() => TypeSymbolFactory.ITestOutputHelper_V2(compilation));
		lazyMemberDataAttributeType = new(() => TypeSymbolFactory.MemberDataAttribute(compilation));
		lazyTheoryAttributeType = new(() => TypeSymbolFactory.TheoryAttribute(compilation));
	}

	/// <inheritdoc/>
	public INamedTypeSymbol? ClassDataAttributeType =>
		lazyClassDataAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? CollectionAttributeType =>
		lazyCollectionAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? CollectionDefinitionAttributeType =>
		lazyCollectionDefinitionAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? DataAttributeType =>
		lazyDataAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? FactAttributeType =>
		lazyFactAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? IClassFixtureType =>
		lazyIClassFixtureType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ICollectionFixtureType =>
		lazyICollectionFixtureType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? InlineDataAttributeType =>
		lazyInlineDataAttributeType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.ITestOutputHelper</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ITestOutputHelperType =>
		lazyITestOutputHelperType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? MemberDataAttributeType =>
		lazyMemberDataAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? TheoryAttributeType =>
		lazyTheoryAttributeType.Value;

	// See: https://github.com/xunit/xunit/pull/1546
	/// <inheritdoc/>
	public bool TheorySupportsConversionFromStringToDateTimeOffsetAndGuid =>
		Version >= Version_2_4_0;

	/// <inheritdoc/>
	public bool TheorySupportsDefaultParameterValues =>
		Version >= Version_2_2_0;

	/// <inheritdoc/>
	public bool TheorySupportsParameterArrays =>
		Version >= Version_2_2_0;

	/// <inheritdoc/>
	public Version Version { get; }

	public static V2CoreContext? Get(
		Compilation compilation,
		Version? versionOverride = null)
	{
		Guard.ArgumentNotNull(compilation);

		var version =
			versionOverride ??
			compilation
				.ReferencedAssemblyNames
				.FirstOrDefault(a => a.Name.Equals("xunit.core", StringComparison.OrdinalIgnoreCase))
				?.Version;

		return version is null ? null : new(compilation, version);
	}
}
