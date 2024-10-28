using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V3CommonContext : ICommonContext
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
	readonly Lazy<INamedTypeSymbol?> lazyIXunitSerializerType;

	V3CommonContext(
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
		lazyIXunitSerializerType = new(() => TypeSymbolFactory.IXunitSerializer_V3(compilation));
	}

	/// <inheritdoc/>
	/// <remarks>This type lives in <c>xunit.v3.common</c>.</remarks>
	public INamedTypeSymbol? IMessageSinkType =>
		lazyIMessageSinkType.Value;

	/// <inheritdoc/>
	/// <remarks>This type lives in <c>xunit.v3.runner.common</c>.</remarks>
	public INamedTypeSymbol? ISourceInformationProviderType =>
		lazyISourceInformationProviderType.Value;

	/// <inheritdoc/>
	/// <remarks>This type lives in <c>xunit.v3.common</c>.</remarks>
	public INamedTypeSymbol? ITestAssemblyType =>
		lazyITestAssemblyType.Value;

	/// <inheritdoc/>
	/// <remarks>This type lives in <c>xunit.v3.common</c>.</remarks>
	public INamedTypeSymbol? ITestCaseType =>
		lazyITestCaseType.Value;

	/// <inheritdoc/>
	/// <remarks>This type lives in <c>xunit.v3.common</c>.</remarks>
	public INamedTypeSymbol? ITestClassType =>
		lazyITestClassType.Value;

	/// <inheritdoc/>
	/// <remarks>This type lives in <c>xunit.v3.common</c>.</remarks>
	public INamedTypeSymbol? ITestCollectionType =>
		lazyITestCollectionType.Value;

	/// <inheritdoc/>
	/// <remarks>This type lives in <c>xunit.v3.core</c>.</remarks>
	public INamedTypeSymbol? ITestFrameworkDiscovererType =>
		lazyITestFrameworkDiscovererType.Value;

	/// <inheritdoc/>
	/// <remarks>This type lives in <c>xunit.v3.core</c>.</remarks>
	public INamedTypeSymbol? ITestFrameworkExecutorType =>
		lazyITestFrameworkExecutorType.Value;

	/// <inheritdoc/>
	/// <remarks>This type lives in <c>xunit.v3.core</c>.</remarks>
	public INamedTypeSymbol? ITestFrameworkType =>
		lazyITestFrameworkType.Value;

	/// <inheritdoc/>
	/// <remarks>This type lives in <c>xunit.v3.common</c>.</remarks>
	public INamedTypeSymbol? ITestMethodType =>
		lazyITestMethodType.Value;

	/// <inheritdoc/>
	/// <remarks>This type lives in <c>xunit.v3.common</c>.</remarks>
	public INamedTypeSymbol? ITestType =>
		lazyITestType.Value;

	/// <inheritdoc/>
	/// <remarks>This type lives in <c>xunit.v3.common</c>.</remarks>
	public INamedTypeSymbol? IXunitSerializableType =>
		lazyIXunitSerializableType.Value;

	public INamedTypeSymbol? IXunitSerializerType =>
		lazyIXunitSerializerType.Value;

	/// <summary>
	/// Gets the version number of the <c>xunit.v3.common</c> assembly.
	/// </summary>
	public Version Version { get; }

	public static V3CommonContext? Get(
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
