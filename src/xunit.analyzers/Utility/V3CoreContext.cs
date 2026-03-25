using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V3CoreContext : ICoreContextV3
{
	readonly Lazy<INamedTypeSymbol?> lazyAssemblyFixtureAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyClassDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyClassDataAttributeOfTType;
	readonly Lazy<INamedTypeSymbol?> lazyCollectionAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyCollectionAttributeOfTType;
	readonly Lazy<INamedTypeSymbol?> lazyCollectionDefinitionAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyCulturedFactAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyCulturedTheoryAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyFactAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyIClassFixtureType;
	readonly Lazy<INamedTypeSymbol?> lazyICollectionFixtureType;
	readonly Lazy<INamedTypeSymbol?> lazyIDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyIFactAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyInlineDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyITestContextAccessorType;
	readonly Lazy<INamedTypeSymbol?> lazyITestOutputHelperType;
	readonly Lazy<INamedTypeSymbol?> lazyJsonTypeIDAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyMemberDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyTheoryAttributeType;

	V3CoreContext(
		Compilation compilation,
		Version version)
	{
		Version = version;

		lazyAssemblyFixtureAttributeType = new(() => TypeSymbolFactory.AssemblyFixtureAttribute_V3(compilation));
		lazyClassDataAttributeType = new(() => TypeSymbolFactory.ClassDataAttribute(compilation));
		lazyClassDataAttributeOfTType = new(() => TypeSymbolFactory.ClassDataAttributeOfT_V3(compilation));
		lazyCollectionAttributeType = new(() => TypeSymbolFactory.CollectionAttribute(compilation));
		lazyCollectionAttributeOfTType = new(() => TypeSymbolFactory.CollectionAttributeOfT_V3(compilation));
		lazyCollectionDefinitionAttributeType = new(() => TypeSymbolFactory.CollectionDefinitionAttribute(compilation));
		lazyCulturedFactAttributeType = new(() => TypeSymbolFactory.CulturedFactAttribute_V3(compilation));
		lazyCulturedTheoryAttributeType = new(() => TypeSymbolFactory.CulturedTheoryAttribute_V3(compilation));
		lazyDataAttributeType = new(() => TypeSymbolFactory.DataAttribute_V3(compilation));
		lazyFactAttributeType = new(() => TypeSymbolFactory.FactAttribute(compilation));
		lazyIClassFixtureType = new(() => TypeSymbolFactory.IClassFixureOfT(compilation));
		lazyICollectionFixtureType = new(() => TypeSymbolFactory.ICollectionFixtureOfT(compilation));
		lazyIDataAttributeType = new(() => TypeSymbolFactory.IDataAttribute_V3(compilation));
		lazyIFactAttributeType = new(() => TypeSymbolFactory.IFactAttribute_V3(compilation));
		lazyInlineDataAttributeType = new(() => TypeSymbolFactory.InlineDataAttribute(compilation));
		lazyITestContextAccessorType = new(() => TypeSymbolFactory.ITestContextAccessor_V3(compilation));
		lazyITestOutputHelperType = new(() => TypeSymbolFactory.ITestOutputHelper_V3(compilation));
		lazyJsonTypeIDAttributeType = new(() => TypeSymbolFactory.JsonTypeIDAttribute_V3(compilation));
		lazyMemberDataAttributeType = new(() => TypeSymbolFactory.MemberDataAttribute(compilation));
		lazyTheoryAttributeType = new(() => TypeSymbolFactory.TheoryAttribute(compilation));
	}

	/// <inheritdoc/>
	public INamedTypeSymbol? AssemblyFixtureAttributeType =>
		lazyAssemblyFixtureAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ClassDataAttributeType =>
		lazyClassDataAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ClassDataAttributeOfTType =>
		lazyClassDataAttributeOfTType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? CollectionAttributeType =>
		lazyCollectionAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? CollectionAttributeOfTType =>
		lazyCollectionAttributeOfTType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? CollectionDefinitionAttributeType =>
		lazyCollectionDefinitionAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? CulturedFactAttributeType =>
		lazyCulturedFactAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? CulturedTheoryAttributeType =>
		lazyCulturedTheoryAttributeType.Value;

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
	public INamedTypeSymbol? IDataAttributeType =>
		lazyIDataAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? IFactAttributeType =>
		lazyIFactAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? InlineDataAttributeType =>
		lazyInlineDataAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ITestContextAccessorType =>
		lazyITestContextAccessorType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ITestOutputHelperType =>
		lazyITestOutputHelperType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? JsonTypeIDAttributeType =>
		lazyJsonTypeIDAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? MemberDataAttributeType =>
		lazyMemberDataAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? TheoryAttributeType =>
		lazyTheoryAttributeType.Value;

	/// <inheritdoc/>
	public bool TheorySupportsConversionFromStringToDateTimeOffsetAndGuid => true;

	/// <inheritdoc/>
	public bool TheorySupportsDefaultParameterValues => true;

	/// <inheritdoc/>
	public bool TheorySupportsParameterArrays => true;

	/// <inheritdoc/>
	public Version Version { get; }

	public static ICoreContextV3? Get(
		Compilation compilation,
		Version? versionOverride = null)
	{
		Guard.ArgumentNotNull(compilation);

		var version =
			versionOverride ??
			compilation
				.ReferencedAssemblyNames
				.FirstOrDefault(a => a.Name.Equals("xunit.v3.core", StringComparison.OrdinalIgnoreCase))
				?.Version;

		return version is null ? null : new V3CoreContext(compilation, version);
	}
}
