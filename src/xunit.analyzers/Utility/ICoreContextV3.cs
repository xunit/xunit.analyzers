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
	/// Gets a reference to type <c>AssemblyFixtureAttribute&lt;T&gt;</c>, if available.
	/// </summary>
	INamedTypeSymbol? AssemblyFixtureAttributeOfTType { get; }

	/// <summary>
	/// Gets a reference to type <c>ClassDataAttribute&lt;T&gt;</c>, if available.
	/// </summary>
	INamedTypeSymbol? ClassDataAttributeOfTType { get; }

	/// <summary>
	/// Gets a reference to type <c>CollectionAttribute&lt;T&gt;</c>, if available.
	/// </summary>
	INamedTypeSymbol? CollectionAttributeOfTType { get; }

	/// <summary>
	/// Gets a reference to type <c>CollectionBehaviorAttribute&lt;T&gt;</c>, if available.
	/// </summary>
	INamedTypeSymbol? CollectionBehaviorAttributeOfTType { get; }

	/// <summary>
	/// Gets a reference to type <c>IDataAttribute</c>, if available.
	/// </summary>
	INamedTypeSymbol? IDataAttributeType { get; }

	/// <summary>
	/// Gets a reference to type <c>IFactAttribute</c>, if available.
	/// </summary>
	INamedTypeSymbol? IFactAttributeType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITestClassOrderer</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestClassOrdererType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITestContextAccessor</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestContextAccessorType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITestMethodOrderer</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestMethodOrdererType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITestPipelineStartup</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestPipelineStartupType { get; }

	/// <summary>
	/// Gets a reference to type <c>IXunitTestAssembly</c> (in reflection mode)
	/// or <c>ICodeGenTestAssemblyType</c> (in Native AOT mode), if available.
	/// </summary>
	INamedTypeSymbol? IXunitTestAssemblyType { get; }

	/// <summary>
	/// Gets a reference to type <c>JsonTypeIDAttribute</c>, if available.
	/// </summary>
	INamedTypeSymbol? JsonTypeIDAttributeType { get; }

	/// <summary>
	/// Gets a reference to type <c>TestCaseOrdererAttribute&lt;T&gt;</c>, if available.
	/// </summary>
	INamedTypeSymbol? TestCaseOrdererAttributeOfTType { get; }

	/// <summary>
	/// Gets a reference to type <c>TestClassOrdererAttribute</c>, if available.
	/// </summary>
	INamedTypeSymbol? TestClassOrdererAttributeType { get; }

	/// <summary>
	/// Gets a reference to type <c>TestClassOrdererAttribute&lt;T&gt;</c>, if available.
	/// </summary>
	INamedTypeSymbol? TestClassOrdererAttributeOfTType { get; }

	/// <summary>
	/// Gets a reference to type <c>TestCollectionOrdererAttribute&lt;T&gt;</c>, if available.
	/// </summary>
	INamedTypeSymbol? TestCollectionOrdererAttributeOfTType { get; }

	/// <summary>
	/// Gets a reference to type <c>TestFrameworkAttribute&lt;T&gt;</c>, if available.
	/// </summary>
	INamedTypeSymbol? TestFrameworkAttributeOfTType { get; }

	/// <summary>
	/// Gets a reference to type <c>TestMethodOrdererAttribute</c>, if available.
	/// </summary>
	INamedTypeSymbol? TestMethodOrdererAttributeType { get; }

	/// <summary>
	/// Gets a reference to type <c>TestMethodOrdererAttribute&lt;T&gt;</c>, if available.
	/// </summary>
	INamedTypeSymbol? TestMethodOrdererAttributeOfTType { get; }

	/// <summary>
	/// Gets a reference to type <c>TestPipelineStartupAttribute</c>, if available.
	/// </summary>
	INamedTypeSymbol? TestPipelineStartupAttributeType { get; }
}
