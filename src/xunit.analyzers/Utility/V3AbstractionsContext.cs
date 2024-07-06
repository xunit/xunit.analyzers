using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

// Technically xunit.abstractions doesn't exist in v3; all the co-existent/updated types are present in xunit.v3.common, instead.

public class V3AbstractionsContext : IAbstractionsContext
{
	readonly Lazy<INamedTypeSymbol?> lazyIMessageSinkType;
	readonly Lazy<INamedTypeSymbol?> lazyISourceInformationProviderType;
	readonly Lazy<INamedTypeSymbol?> lazyITestAssemblyType;
	readonly Lazy<INamedTypeSymbol?> lazyITestCaseType;
	readonly Lazy<INamedTypeSymbol?> lazyITestClassType;
	readonly Lazy<INamedTypeSymbol?> lazyITestCollectionType;
	readonly Lazy<INamedTypeSymbol?> lazyITestFrameworkDiscovererType;
	readonly Lazy<INamedTypeSymbol?> lazyITestFrameworkExecutorType;
	readonly Lazy<INamedTypeSymbol?> lazyITestFrameworkType;
	readonly Lazy<INamedTypeSymbol?> lazyITestMethodType;
	readonly Lazy<INamedTypeSymbol?> lazyITestType;
	readonly Lazy<INamedTypeSymbol?> lazyIXunitSerializableType;

	V3AbstractionsContext(
		Compilation compilation,
		Version version)
	{
		Version = version;

		lazyIMessageSinkType = new(() => TypeSymbolFactory.IMessageSink_V3(compilation));
		lazyISourceInformationProviderType = new(() => TypeSymbolFactory.ISourceInformationProvider_V3(compilation));
		lazyITestAssemblyType = new(() => TypeSymbolFactory.ITestAssembly_V3(compilation));
		lazyITestCaseType = new(() => TypeSymbolFactory.ITestCase_V3(compilation));
		lazyITestClassType = new(() => TypeSymbolFactory.ITestClass_V3(compilation));
		lazyITestCollectionType = new(() => TypeSymbolFactory.ITestCollection_V3(compilation));
		lazyITestFrameworkDiscovererType = new(() => TypeSymbolFactory.ITestFrameworkDiscoverer_V3(compilation));
		lazyITestFrameworkExecutorType = new(() => TypeSymbolFactory.ITestFrameworkExecutor_V3(compilation));
		lazyITestFrameworkType = new(() => TypeSymbolFactory.ITestFramework_V3(compilation));
		lazyITestMethodType = new(() => TypeSymbolFactory.ITestMethod_V3(compilation));
		lazyITestType = new(() => TypeSymbolFactory.ITest_V3(compilation));
		lazyIXunitSerializableType = new(() => TypeSymbolFactory.IXunitSerializable_V3(compilation));
	}

	/// <inheritdoc/>
	public INamedTypeSymbol? IMessageSinkType =>
		lazyIMessageSinkType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ISourceInformationProviderType =>
		lazyISourceInformationProviderType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ITestAssemblyType =>
		lazyITestAssemblyType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ITestCaseType =>
		lazyITestCaseType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ITestClassType =>
		lazyITestClassType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ITestCollectionType =>
		lazyITestCollectionType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ITestFrameworkDiscovererType =>
		lazyITestFrameworkDiscovererType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ITestFrameworkExecutorType =>
		lazyITestFrameworkExecutorType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ITestFrameworkType =>
		lazyITestFrameworkType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ITestMethodType =>
		lazyITestMethodType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? ITestType =>
		lazyITestType.Value;

	/// <inheritdoc/>
	public INamedTypeSymbol? IXunitSerializableType =>
		lazyIXunitSerializableType.Value;

	/// <summary>
	/// Gets the version number of the <c>xunit.v3.common</c> assembly.
	/// </summary>
	public Version Version { get; }

	public static V3AbstractionsContext? Get(
		Compilation compilation,
		Version? versionOverride = null)
	{
		Guard.ArgumentNotNull(compilation);

		var version =
			versionOverride ??
			compilation
				.ReferencedAssemblyNames
				.FirstOrDefault(a => a.Name.Equals("xunit.v3.common", StringComparison.OrdinalIgnoreCase))
				?.Version;

		return version is null ? null : new(compilation, version);
	}
}
