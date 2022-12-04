using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
	public class EmptyCoreContext : ICoreContext
	{
		public INamedTypeSymbol? ClassDataAttributeType => null;

		public INamedTypeSymbol? CollectionDefinitionAttributeType => null;

		public INamedTypeSymbol? DataAttributeType => null;

		public INamedTypeSymbol? FactAttributeType => null;

		public INamedTypeSymbol? IClassFixtureType => null;

		public INamedTypeSymbol? ICollectionFixtureType => null;

		public INamedTypeSymbol? InlineDataAttributeType => null;

		public INamedTypeSymbol? MemberDataAttributeType => null;

		public INamedTypeSymbol? TheoryAttributeType => null;

		public bool TheorySupportsConversionFromStringToDateTimeOffsetAndGuid => false;

		public bool TheorySupportsDefaultParameterValues => false;

		public bool TheorySupportsParameterArrays => false;
	}
}
