using System;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class EmptyCommonContext : ICommonContext
{
	EmptyCommonContext()
	{ }

	public static EmptyCommonContext Instance { get; } = new();

	public INamedTypeSymbol? IMessageSinkType => null;

	public INamedTypeSymbol? ISourceInformationProviderType => null;

	public INamedTypeSymbol? ITestAssemblyType => null;

	public INamedTypeSymbol? ITestCaseType => null;

	public INamedTypeSymbol? ITestClassType => null;

	public INamedTypeSymbol? ITestCollectionType => null;

	public INamedTypeSymbol? ITestFrameworkDiscovererType => null;

	public INamedTypeSymbol? ITestFrameworkExecutorType => null;

	public INamedTypeSymbol? ITestFrameworkType => null;

	public INamedTypeSymbol? ITestMethodType => null;

	public INamedTypeSymbol? ITestType => null;

	public INamedTypeSymbol? IXunitSerializableType => null;

	public Version Version => new();
}
