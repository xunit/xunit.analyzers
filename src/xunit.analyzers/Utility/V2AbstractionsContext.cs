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

	public INamedTypeSymbol? IAssemblyInfoType =>
		lazyIAssemblyInfoType.Value;

	public INamedTypeSymbol? IAttributeInfoType =>
		lazyIAttributeInfoType.Value;

	public INamedTypeSymbol? IMessageSinkMessageType =>
		lazyIMessageSinkMessageType.Value;

	public INamedTypeSymbol? IMessageSinkType =>
		lazyIMessageSinkType.Value;

	public INamedTypeSymbol? IMethodInfoType =>
		lazyIMethodInfoType.Value;

	public INamedTypeSymbol? IParameterInfoType =>
		lazyIParameterInfoType.Value;

	public INamedTypeSymbol? ISourceInformationProviderType =>
		lazyISourceInformationProviderType.Value;

	public INamedTypeSymbol? ISourceInformationType =>
		lazyISourceInformationType.Value;

	public INamedTypeSymbol? ITestAssemblyType =>
		lazyITestAssemblyType.Value;

	public INamedTypeSymbol? ITestCaseType =>
		lazyITestCaseType.Value;

	public INamedTypeSymbol? ITestClassType =>
		lazyITestClassType.Value;

	public INamedTypeSymbol? ITestCollectionType =>
		lazyITestCollectionType.Value;

	public INamedTypeSymbol? ITestFrameworkDiscovererType =>
		lazyITestFrameworkDiscovererType.Value;

	public INamedTypeSymbol? ITestFrameworkExecutorType =>
		lazyITestFrameworkExecutorType.Value;

	public INamedTypeSymbol? ITestFrameworkType =>
		lazyITestFrameworkType.Value;

	public INamedTypeSymbol? ITestMethodType =>
		lazyITestMethodType.Value;

	public INamedTypeSymbol? ITestType =>
		lazyITestType.Value;

	public INamedTypeSymbol? ITypeInfoType =>
		lazyITypeInfoType.Value;

	public INamedTypeSymbol? IXunitSerializableType =>
		lazyIXunitSerializableType.Value;

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
