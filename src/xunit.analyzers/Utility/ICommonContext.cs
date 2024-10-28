using System;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

/// <summary>
/// Context for types that that originated in <c>xunit.abstractions</c> in v2, and moved in v3
/// to one of <c>xunit.v3.common</c>, <c>xunit.v3.core</c>, or <c>xunit.v3.runner.common</c>.
/// </summary>
public interface ICommonContext
{
	/// <summary>
	/// Gets a reference to type <c>IMessageSink</c>, if available.
	/// </summary>
	INamedTypeSymbol? IMessageSinkType { get; }

	/// <summary>
	/// Gets a reference to type <c>ISourceInformationProvider</c>, if available.
	/// </summary>
	INamedTypeSymbol? ISourceInformationProviderType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITestAssembly</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestAssemblyType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITestCase</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestCaseType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITestClass</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestClassType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITestCollection</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestCollectionType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITestFrameworkDiscoverer</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestFrameworkDiscovererType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITestFrameworkExecutor</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestFrameworkExecutorType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITestFramework</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestFrameworkType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITestMethod</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestMethodType { get; }

	/// <summary>
	/// Gets a reference to type <c>ITest</c>, if available.
	/// </summary>
	INamedTypeSymbol? ITestType { get; }

	/// <summary>
	/// Gets a reference to type <c>IXunitSerializable</c>, if available.
	/// </summary>
	INamedTypeSymbol? IXunitSerializableType { get; }

	/// <summary>
	/// Gets the version number of the <c>xunit.abstractions</c> or <c>xunit.v3.common</c> assembly.
	/// </summary>
	public Version Version { get; }
}
