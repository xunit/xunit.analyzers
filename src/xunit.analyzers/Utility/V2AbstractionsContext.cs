using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V2AbstractionsContext
{
	readonly Lazy<INamedTypeSymbol?> lazyIAssemblyInfoType;
	readonly Lazy<INamedTypeSymbol?> lazyIAttributeInfoType;
	readonly Lazy<INamedTypeSymbol?> lazyIMessageSinkMessageType;
	readonly Lazy<INamedTypeSymbol?> lazyIMessageSinkType;
	readonly Lazy<INamedTypeSymbol?> lazyIMethodInfoType;
	readonly Lazy<INamedTypeSymbol?> lazyIParameterInfoType;
	readonly Lazy<INamedTypeSymbol?> lazyISourceInformationProviderType;
	readonly Lazy<INamedTypeSymbol?> lazyISourceInformationType;
	readonly Lazy<INamedTypeSymbol?> lazyITestAssemblyType;
	readonly Lazy<INamedTypeSymbol?> lazyITestCaseType;
	readonly Lazy<INamedTypeSymbol?> lazyITestClassType;
	readonly Lazy<INamedTypeSymbol?> lazyITestCollectionType;
	readonly Lazy<INamedTypeSymbol?> lazyITestFrameworkDiscovererType;
	readonly Lazy<INamedTypeSymbol?> lazyITestFrameworkExecutorType;
	readonly Lazy<INamedTypeSymbol?> lazyITestFrameworkType;
	readonly Lazy<INamedTypeSymbol?> lazyITestMethodType;
	readonly Lazy<INamedTypeSymbol?> lazyITestType;
	readonly Lazy<INamedTypeSymbol?> lazyITypeInfoType;
	readonly Lazy<INamedTypeSymbol?> lazyIXunitSerializableType;

	V2AbstractionsContext(
		Compilation compilation,
		Version version)
	{
		Version = version;

		lazyIAssemblyInfoType = new(() => TypeSymbolFactory.IAssemblyInfo_V2(compilation));
		lazyIAttributeInfoType = new(() => TypeSymbolFactory.IAttributeInfo_V2(compilation));
		lazyIMessageSinkMessageType = new(() => TypeSymbolFactory.IMessageSinkMessage_V2(compilation));
		lazyIMessageSinkType = new(() => TypeSymbolFactory.IMessageSink_V2(compilation));
		lazyIMethodInfoType = new(() => TypeSymbolFactory.IMethodInfo_V2(compilation));
		lazyIParameterInfoType = new(() => TypeSymbolFactory.IParameterInfo_V2(compilation));
		lazyISourceInformationProviderType = new(() => TypeSymbolFactory.ISourceInformationProvider_V2(compilation));
		lazyISourceInformationType = new(() => TypeSymbolFactory.ISourceInformation_V2(compilation));
		lazyITestAssemblyType = new(() => TypeSymbolFactory.ITestAssembly_V2(compilation));
		lazyITestCaseType = new(() => TypeSymbolFactory.ITestCase_V2(compilation));
		lazyITestClassType = new(() => TypeSymbolFactory.ITestClass_V2(compilation));
		lazyITestCollectionType = new(() => TypeSymbolFactory.ITestCollection_V2(compilation));
		lazyITestFrameworkDiscovererType = new(() => TypeSymbolFactory.ITestFrameworkDiscoverer_V2(compilation));
		lazyITestFrameworkExecutorType = new(() => TypeSymbolFactory.ITestFrameworkExecutor_V2(compilation));
		lazyITestFrameworkType = new(() => TypeSymbolFactory.ITestFramework_V2(compilation));
		lazyITestMethodType = new(() => TypeSymbolFactory.ITestMethod_V2(compilation));
		lazyITestType = new(() => TypeSymbolFactory.ITest_V2(compilation));
		lazyITypeInfoType = new(() => TypeSymbolFactory.ITypeInfo_V2(compilation));
		lazyIXunitSerializableType = new(() => TypeSymbolFactory.IXunitSerializable_V2(compilation));
	}

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.IAssemblyInfo</c>, if available.
	/// </summary>
	public INamedTypeSymbol? IAssemblyInfoType =>
		lazyIAssemblyInfoType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.IAttributeInfo</c>, if available.
	/// </summary>
	public INamedTypeSymbol? IAttributeInfoType =>
		lazyIAttributeInfoType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.IMessageSinkMessage</c>, if available.
	/// </summary>
	public INamedTypeSymbol? IMessageSinkMessageType =>
		lazyIMessageSinkMessageType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.IMessageSink</c>, if available.
	/// </summary>
	public INamedTypeSymbol? IMessageSinkType =>
		lazyIMessageSinkType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.IMethodInfo</c>, if available.
	/// </summary>
	public INamedTypeSymbol? IMethodInfoType =>
		lazyIMethodInfoType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.IParameterInfo</c>, if available.
	/// </summary>
	public INamedTypeSymbol? IParameterInfoType =>
		lazyIParameterInfoType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.ISourceInformationProvider</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ISourceInformationProviderType =>
		lazyISourceInformationProviderType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.ISourceInformation</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ISourceInformationType =>
		lazyISourceInformationType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.ITestAssembly</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ITestAssemblyType =>
		lazyITestAssemblyType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.ITestCase</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ITestCaseType =>
		lazyITestCaseType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.ITestClass</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ITestClassType =>
		lazyITestClassType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.ITestCollection</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ITestCollectionType =>
		lazyITestCollectionType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.ITestFrameworkDiscoverer</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ITestFrameworkDiscovererType =>
		lazyITestFrameworkDiscovererType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.ITestFrameworkExecutor</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ITestFrameworkExecutorType =>
		lazyITestFrameworkExecutorType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.ITestFramework</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ITestFrameworkType =>
		lazyITestFrameworkType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.ITestMethod</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ITestMethodType =>
		lazyITestMethodType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.ITest</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ITestType =>
		lazyITestType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.ITypeInfo</c>, if available.
	/// </summary>
	public INamedTypeSymbol? ITypeInfoType =>
		lazyITypeInfoType.Value;

	/// <summary>
	/// Gets a reference to type <c>Xunit.Abstractions.IXunitSerializable</c>, if available.
	/// </summary>
	public INamedTypeSymbol? IXunitSerializableType =>
		lazyIXunitSerializableType.Value;

	/// <summary>
	/// Gets the version number of the abstractions assembly.
	/// </summary>
	public Version Version { get; }

	public static V2AbstractionsContext? Get(
		Compilation compilation,
		Version? versionOverride = null)
	{
		var version =
			versionOverride ??
			compilation
				.ReferencedAssemblyNames
				.FirstOrDefault(a => a.Name.Equals("xunit.abstractions", StringComparison.OrdinalIgnoreCase))
				?.Version;

		return version is null ? null : new(compilation, version);
	}
}
