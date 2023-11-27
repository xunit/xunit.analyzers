using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public interface ICoreContext
{
	INamedTypeSymbol? ClassDataAttributeType { get; }

	INamedTypeSymbol? CollectionAttributeType { get; }

	INamedTypeSymbol? CollectionDefinitionAttributeType { get; }

	INamedTypeSymbol? DataAttributeType { get; }

	INamedTypeSymbol? FactAttributeType { get; }

	INamedTypeSymbol? IClassFixtureType { get; }

	INamedTypeSymbol? ICollectionFixtureType { get; }

	INamedTypeSymbol? InlineDataAttributeType { get; }

	INamedTypeSymbol? MemberDataAttributeType { get; }

	INamedTypeSymbol? TheoryAttributeType { get; }

	bool TheorySupportsConversionFromStringToDateTimeOffsetAndGuid { get; }

	bool TheorySupportsDefaultParameterValues { get; }

	bool TheorySupportsParameterArrays { get; }
}
