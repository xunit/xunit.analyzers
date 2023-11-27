using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V3CoreContext : ICoreContext
{
	readonly Lazy<INamedTypeSymbol?> lazyClassDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyCollectionAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyCollectionDefinitionAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyFactAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyIClassFixtureType;
	readonly Lazy<INamedTypeSymbol?> lazyICollectionFixtureType;
	readonly Lazy<INamedTypeSymbol?> lazyInlineDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyMemberDataAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyTheoryAttributeType;

	V3CoreContext(
		Compilation compilation,
		Version version)
	{
		Version = version;

		lazyClassDataAttributeType = new(() => TypeSymbolFactory.ClassDataAttribute(compilation));
		lazyCollectionAttributeType = new(() => TypeSymbolFactory.CollectionAttribute(compilation));
		lazyCollectionDefinitionAttributeType = new(() => TypeSymbolFactory.CollectionDefinitionAttribute(compilation));
		lazyDataAttributeType = new(() => TypeSymbolFactory.DataAttribute(compilation));
		lazyFactAttributeType = new(() => TypeSymbolFactory.FactAttribute(compilation));
		lazyIClassFixtureType = new(() => TypeSymbolFactory.IClassFixureOfT(compilation));
		lazyICollectionFixtureType = new(() => TypeSymbolFactory.ICollectionFixtureOfT(compilation));
		lazyInlineDataAttributeType = new(() => TypeSymbolFactory.InlineDataAttribute(compilation));
		lazyMemberDataAttributeType = new(() => TypeSymbolFactory.MemberDataAttribute(compilation));
		lazyTheoryAttributeType = new(() => TypeSymbolFactory.TheoryAttribute(compilation));
	}

	public INamedTypeSymbol? ClassDataAttributeType =>
		lazyClassDataAttributeType.Value;

	public INamedTypeSymbol? CollectionAttributeType =>
		lazyCollectionAttributeType.Value;

	public INamedTypeSymbol? CollectionDefinitionAttributeType =>
		lazyCollectionDefinitionAttributeType.Value;

	public INamedTypeSymbol? DataAttributeType =>
		lazyDataAttributeType.Value;

	public INamedTypeSymbol? FactAttributeType =>
		lazyFactAttributeType.Value;

	public INamedTypeSymbol? IClassFixtureType =>
		lazyIClassFixtureType.Value;

	public INamedTypeSymbol? ICollectionFixtureType =>
		lazyICollectionFixtureType.Value;

	public INamedTypeSymbol? InlineDataAttributeType =>
		lazyInlineDataAttributeType.Value;

	public INamedTypeSymbol? MemberDataAttributeType =>
		lazyMemberDataAttributeType.Value;

	public INamedTypeSymbol? TheoryAttributeType =>
		lazyTheoryAttributeType.Value;

	public bool TheorySupportsConversionFromStringToDateTimeOffsetAndGuid => true;

	public bool TheorySupportsDefaultParameterValues => true;

	public bool TheorySupportsParameterArrays => true;

	public Version Version { get; set; }

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
