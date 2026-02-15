using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

/// <summary>
/// Context for types that live in one of <c>xunit.v3.core</c> or <c>xunit.v3.core.aot</c>.
/// </summary>
public interface ICoreContextV3 : ICoreContext
{
	/// <summary>
	/// Gets a reference to type <c>AssemblyFixtureAttribute</c>, if available.
	/// </summary>
	INamedTypeSymbol? AssemblyFixtureAttributeType { get; }

	/// <summary>
	/// Gets a reference to type <c>ClassDataAttribute&lt;T&gt;</c>, if available.
	/// </summary>
	INamedTypeSymbol? ClassDataAttributeOfTType { get; }

	/// <summary>
	/// Gets a reference to type <c>CollectionAttribute&lt;T&gt;</c>, if available.
	/// </summary>
	INamedTypeSymbol? CollectionAttributeOfTType { get; }

	/// <summary>
	/// Gets a reference to type <c>IDataAttribute</c>, if available.
	/// </summary>
	INamedTypeSymbol? IDataAttributeType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITestContextAccessor</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestContextAccessorType { get; }

	/// <summary>
	/// Gets a reference to type <c>JsonTypeIDAttribute</c>, if available.
	/// </summary>
	INamedTypeSymbol? JsonTypeIDAttributeType { get; }
}
