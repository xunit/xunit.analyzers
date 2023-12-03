using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class EmptyCoreContext : ICoreContext
{
	EmptyCoreContext()
	{ }

	public INamedTypeSymbol? ClassDataAttributeType => null;

	public INamedTypeSymbol? CollectionAttributeType => null;

	public INamedTypeSymbol? CollectionDefinitionAttributeType => null;

	public INamedTypeSymbol? DataAttributeType => null;

	public INamedTypeSymbol? FactAttributeType => null;

	public INamedTypeSymbol? IClassFixtureType => null;

	public INamedTypeSymbol? ICollectionFixtureType => null;

	public INamedTypeSymbol? InlineDataAttributeType => null;

	public INamedTypeSymbol? ITestOutputHelperType => null;

	public static EmptyCoreContext Instance { get; } = new();

	public INamedTypeSymbol? MemberDataAttributeType => null;

	public INamedTypeSymbol? TheoryAttributeType => null;

	public bool TheorySupportsConversionFromStringToDateTimeOffsetAndGuid => false;

	public bool TheorySupportsDefaultParameterValues => false;

	public bool TheorySupportsParameterArrays => false;
}
