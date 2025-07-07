using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V3CoreContext : ICoreContext
{
	readonly Lazy<INamedTypeSymbol?> lazyAssemblyFixtureAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyClassDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyClassDataAttributeOfTType;
	readonly Lazy<INamedTypeSymbol?> lazyCollectionAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyCollectionAttributeOfTType;
	readonly Lazy<INamedTypeSymbol?> lazyCollectionDefinitionAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyFactAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyIClassFixtureType;
	readonly Lazy<INamedTypeSymbol?> lazyICollectionFixtureType;
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
		lazyDataAttributeType = new(() => TypeSymbolFactory.DataAttribute_V3(compilation));
		lazyFactAttributeType = new(() => TypeSymbolFactory.FactAttribute(compilation));
		lazyIClassFixtureType = new(() => TypeSymbolFactory.IClassFixureOfT(compilation));
		lazyICollectionFixtureType = new(() => TypeSymbolFactory.ICollectionFixtureOfT(compilation));
		lazyInlineDataAttributeType = new(() => TypeSymbolFactory.InlineDataAttribute(compilation));
		lazyITestContextAccessorType = new(() => TypeSymbolFactory.ITestContextAccessor_V3(compilation));
		lazyITestOutputHelperType = new(() => TypeSymbolFactory.ITestOutputHelper_V3(compilation));
		lazyJsonTypeIDAttributeType = new(() => TypeSymbolFactory.JsonTypeIDAttribute_V3(compilation));
		lazyMemberDataAttributeType = new(() => TypeSymbolFactory.MemberDataAttribute(compilation));
		lazyTheoryAttributeType = new(() => TypeSymbolFactory.TheoryAttribute(compilation));
	}

	/// <summary>
	/// Gets a reference to type <c>AssemblyFixtureAttribute</c>, if available.
	/// </summary>
	public INamedTypeSymbol? AssemblyFixtureAttributeType =>
		lazyAssemblyFixtureAttributeType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ClassDataAttributeType =>
		lazyClassDataAttributeType.Value;

	/// <summary>
	/// Gets a reference to type <c>ClassDataAttribute&lt;T&gt;</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ClassDataAttributeOfTType =>
		lazyClassDataAttributeOfTType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? CollectionAttributeType =>
		lazyCollectionAttributeType.Value;

	/// <summary>
	/// Gets a reference to type <c>CollectionAttribute&lt;T&gt;</c>, if available.
	/// </summary>
	public INamedTypeSymbol? CollectionAttributeOfTType =>
		lazyCollectionAttributeOfTType.Value;

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
	/// Gets a reference to type <c>ITestContextAccessor</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ITestContextAccessorType =>
		lazyITestContextAccessorType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ITestOutputHelperType =>
		lazyITestOutputHelperType.Value;

	/// <summary>
	/// Gets a reference to type <c>JsonTypeIDAttribute</c>, if available.
	/// </summary>
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

	public static V3CoreContext? Get(
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

		return version is null ? null : new(compilation, version);
	}
}
