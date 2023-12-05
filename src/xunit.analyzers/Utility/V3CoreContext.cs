using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V3CoreContext : ICoreContext
{
	readonly Lazy<INamedTypeSymbol?> lazyAssemblyFixtureAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyClassDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyCollectionAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyCollectionDefinitionAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyFactAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyIClassFixtureType;
	readonly Lazy<INamedTypeSymbol?> lazyICollectionFixtureType;
	readonly Lazy<INamedTypeSymbol?> lazyInlineDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyITestContextAccessorType;
	readonly Lazy<INamedTypeSymbol?> lazyITestOutputHelperType;
	readonly Lazy<INamedTypeSymbol?> lazyMemberDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyTheoryAttributeType;

	V3CoreContext(
		Compilation compilation,
		Version version)
	{
		Version = version;

		lazyAssemblyFixtureAttributeType = new(() => TypeSymbolFactory.AssemblyFixtureAttribute_V3(compilation));
		lazyClassDataAttributeType = new(() => TypeSymbolFactory.ClassDataAttribute(compilation));
		lazyCollectionAttributeType = new(() => TypeSymbolFactory.CollectionAttribute(compilation));
		lazyCollectionDefinitionAttributeType = new(() => TypeSymbolFactory.CollectionDefinitionAttribute(compilation));
		lazyDataAttributeType = new(() => TypeSymbolFactory.DataAttribute(compilation));
		lazyFactAttributeType = new(() => TypeSymbolFactory.FactAttribute(compilation));
		lazyIClassFixtureType = new(() => TypeSymbolFactory.IClassFixureOfT(compilation));
		lazyICollectionFixtureType = new(() => TypeSymbolFactory.ICollectionFixtureOfT(compilation));
		lazyInlineDataAttributeType = new(() => TypeSymbolFactory.InlineDataAttribute(compilation));
		lazyITestContextAccessorType = new(() => TypeSymbolFactory.ITestContextAccessor_V3(compilation));
		lazyITestOutputHelperType = new(() => TypeSymbolFactory.ITestOutputHelper_V3(compilation));
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

	/// <inheritdoc/>
	public INamedTypeSymbol? ITestContextAccessorType =>
		lazyITestContextAccessorType.Value;

	// TODO: This will need to be updated when v3 names are finalized
	/// <summary>
	/// Gets a reference to type <c>Xunit.v3._ITestOutputHelper</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ITestOutputHelperType =>
		lazyITestOutputHelperType.Value;

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
