using System;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public abstract class CoreContextV3Base(
	Compilation compilation,
	Version version) :
		CoreContextBase(compilation, version), ICoreContextV3
{
	readonly Lazy<INamedTypeSymbol?> lazyAssemblyFixtureAttributeType = new(() => TypeSymbolFactory.AssemblyFixtureAttribute_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyAssemblyFixtureAttributeOfTType = new(() => TypeSymbolFactory.AssemblyFixtureAttributeOfT_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyBeforeAfterTestAttributeType = new(() => TypeSymbolFactory.BeforeAfterTestAttribute_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyClassDataAttributeOfTType = new(() => TypeSymbolFactory.ClassDataAttributeOfT_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyCollectionAttributeOfTType = new(() => TypeSymbolFactory.CollectionAttributeOfT_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyCollectionBehaviorAttributeOfTType = new(() => TypeSymbolFactory.CollectionBehaviorAttributeOfT_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyCulturedFactAttributeType = new(() => TypeSymbolFactory.CulturedFactAttribute_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyCulturedTheoryAttributeType = new(() => TypeSymbolFactory.CulturedTheoryAttribute_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyDataAttributeType = new(() => TypeSymbolFactory.DataAttribute_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyITestCaseOrdererType = new(() => TypeSymbolFactory.ITestCaseOrderer_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyITestClassOrdererType = new(() => TypeSymbolFactory.ITestClassOrderer_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyITestCollectionOrdererType = new(() => TypeSymbolFactory.ITestCollectionOrderer_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyITestContextAccessorType = new(() => TypeSymbolFactory.ITestContextAccessor_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyITestFrameworkType = new(() => TypeSymbolFactory.ITestFramework_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyITestMethodOrdererType = new(() => TypeSymbolFactory.ITestMethodOrderer_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyITestOutputHelperType = new(() => TypeSymbolFactory.ITestOutputHelper_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyITestPipelineStartupType = new(() => TypeSymbolFactory.ITestPipelineStartup_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyJsonTypeIDAttributeType = new(() => TypeSymbolFactory.JsonTypeIDAttribute_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyTestCaseOrdererAttributeOfTType = new(() => TypeSymbolFactory.TestCaseOrdererAttributeOfT_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyTestClassOrdererAttributeType = new(() => TypeSymbolFactory.TestClassOrdererAttribute_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyTestClassOrdererAttributeOfTType = new(() => TypeSymbolFactory.TestClassOrdererAttributeOfT_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyTestCollectionOrdererAttributeOfTType = new(() => TypeSymbolFactory.TestCollectionOrdererAttributeOfT_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyTestFrameworkAttributeOfTType = new(() => TypeSymbolFactory.TestFrameworkAttributeOfT_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyTestMethodOrdererAttributeType = new(() => TypeSymbolFactory.TestMethodOrdererAttribute_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyTestMethodOrdererAttributeOfTType = new(() => TypeSymbolFactory.TestMethodOrdererAttributeOfT_V3(compilation));
	readonly Lazy<INamedTypeSymbol?> lazyTestPipelineStartupAttributeType = new(() => TypeSymbolFactory.TestPipelineStartupAttribute_V3(compilation));

	public INamedTypeSymbol? AssemblyFixtureAttributeType =>
		lazyAssemblyFixtureAttributeType.Value;

	public INamedTypeSymbol? AssemblyFixtureAttributeOfTType =>
		lazyAssemblyFixtureAttributeOfTType.Value;

	public override INamedTypeSymbol? BeforeAfterTestAttributeType =>
		lazyBeforeAfterTestAttributeType.Value;

	public INamedTypeSymbol? ClassDataAttributeOfTType =>
		lazyClassDataAttributeOfTType.Value;

	public INamedTypeSymbol? CollectionAttributeOfTType =>
		lazyCollectionAttributeOfTType.Value;

	public INamedTypeSymbol? CollectionBehaviorAttributeOfTType =>
		lazyCollectionBehaviorAttributeOfTType.Value;

	public override INamedTypeSymbol? CulturedFactAttributeType =>
		lazyCulturedFactAttributeType.Value;

	public override INamedTypeSymbol? CulturedTheoryAttributeType =>
		lazyCulturedTheoryAttributeType.Value;

	public override INamedTypeSymbol? DataAttributeType =>
		lazyDataAttributeType.Value;

	public abstract INamedTypeSymbol? IDataAttributeType { get; }

	public abstract INamedTypeSymbol? IFactAttributeType { get; }

	public override INamedTypeSymbol? ITestCaseOrdererType =>
		lazyITestCaseOrdererType.Value;

	public INamedTypeSymbol? ITestClassOrdererType =>
		lazyITestClassOrdererType.Value;

	public override INamedTypeSymbol? ITestCollectionOrdererType =>
		lazyITestCollectionOrdererType.Value;

	public INamedTypeSymbol? ITestContextAccessorType =>
		lazyITestContextAccessorType.Value;

	public override INamedTypeSymbol? ITestFrameworkType =>
		lazyITestFrameworkType.Value;

	public override INamedTypeSymbol? ITestOutputHelperType =>
		lazyITestOutputHelperType.Value;

	public INamedTypeSymbol? ITestMethodOrdererType =>
		lazyITestMethodOrdererType.Value;

	public INamedTypeSymbol? ITestPipelineStartupType =>
		lazyITestPipelineStartupType.Value;

	public abstract INamedTypeSymbol? IXunitTestAssemblyType { get; }

	public INamedTypeSymbol? JsonTypeIDAttributeType =>
		lazyJsonTypeIDAttributeType.Value;

	public INamedTypeSymbol? TestCaseOrdererAttributeOfTType =>
		lazyTestCaseOrdererAttributeOfTType.Value;

	public INamedTypeSymbol? TestClassOrdererAttributeType =>
		lazyTestClassOrdererAttributeType.Value;

	public INamedTypeSymbol? TestClassOrdererAttributeOfTType =>
		lazyTestClassOrdererAttributeOfTType.Value;

	public INamedTypeSymbol? TestCollectionOrdererAttributeOfTType =>
		lazyTestCollectionOrdererAttributeOfTType.Value;

	public INamedTypeSymbol? TestFrameworkAttributeOfTType =>
		lazyTestFrameworkAttributeOfTType.Value;

	public INamedTypeSymbol? TestMethodOrdererAttributeType =>
		lazyTestMethodOrdererAttributeType.Value;

	public INamedTypeSymbol? TestMethodOrdererAttributeOfTType =>
		lazyTestMethodOrdererAttributeOfTType.Value;

	public INamedTypeSymbol? TestPipelineStartupAttributeType =>
		lazyTestPipelineStartupAttributeType.Value;

	public override bool TheorySupportsConversionFromStringToDateTimeOffsetAndGuid => true;

	public override bool TheorySupportsDefaultParameterValues => true;

	public override bool TheorySupportsParameterArrays => true;
}
