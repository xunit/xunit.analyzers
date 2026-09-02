using System;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class RunnerCommonContextV3Base : IRunnerCommonContextV3
{
	readonly Lazy<INamedTypeSymbol?> lazyIConsoleResultWriterType;
	readonly Lazy<INamedTypeSymbol?> lazyIMicrosoftTestingPlatformResultWriterType;
	readonly Lazy<INamedTypeSymbol?> lazyIRunnerReporterType;
	readonly Lazy<INamedTypeSymbol?> lazyRegisterConsoleResultWriterAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyRegisterConsoleResultWriterAttributeOfTType;
	readonly Lazy<INamedTypeSymbol?> lazyRegisterMicrosoftTestingPlatformResultWriterAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyRegisterMicrosoftTestingPlatformResultWriterAttributeOfTType;
	readonly Lazy<INamedTypeSymbol?> lazyRegisterResultWriterAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyRegisterResultWriterAttributeOfTType;
	readonly Lazy<INamedTypeSymbol?> lazyRegisterRunnerReporterAttributeType;
	readonly Lazy<INamedTypeSymbol?> lazyRegisterRunnerReporterAttributeOfTType;

	protected RunnerCommonContextV3Base(
		Compilation compilation,
		Version version)
	{
		Version = version;

		lazyIConsoleResultWriterType = new(() => TypeSymbolFactory.IConsoleResultWriter_V3(compilation));
		lazyIMicrosoftTestingPlatformResultWriterType = new(() => TypeSymbolFactory.IMicrosoftTestingPlatformResultWriter_V3(compilation));
		lazyIRunnerReporterType = new(() => TypeSymbolFactory.IRunnerReporter_V3(compilation));
		lazyRegisterConsoleResultWriterAttributeType = new(() => TypeSymbolFactory.RegisterConsoleResultWriterAttribute_V3(compilation));
		lazyRegisterConsoleResultWriterAttributeOfTType = new(() => TypeSymbolFactory.RegisterConsoleResultWriterAttributeOfT_V3(compilation));
		lazyRegisterMicrosoftTestingPlatformResultWriterAttributeType = new(() => TypeSymbolFactory.RegisterMicrosoftTestingPlatformResultWriterAttribute_V3(compilation));
		lazyRegisterMicrosoftTestingPlatformResultWriterAttributeOfTType = new(() => TypeSymbolFactory.RegisterMicrosoftTestingPlatformResultWriterAttributeOfT_V3(compilation));
		lazyRegisterResultWriterAttributeType = new(() => TypeSymbolFactory.RegisterResultWriterAttribute_V3(compilation));
		lazyRegisterResultWriterAttributeOfTType = new(() => TypeSymbolFactory.RegisterResultWriterAttributeOfT_V3(compilation));
		lazyRegisterRunnerReporterAttributeType = new(() => TypeSymbolFactory.RegisterRunnerReporterAttribute_V3(compilation));
		lazyRegisterRunnerReporterAttributeOfTType = new(() => TypeSymbolFactory.RegisterRunnerReporterAttributeOfT_V3(compilation));
	}

	public INamedTypeSymbol? IConsoleResultWriterType =>
		lazyIConsoleResultWriterType.Value;

	public INamedTypeSymbol? IMicrosoftTestingPlatformResultWriterType =>
		lazyIMicrosoftTestingPlatformResultWriterType.Value;

	public INamedTypeSymbol? IRunnerReporterType =>
		lazyIRunnerReporterType.Value;

	public INamedTypeSymbol? RegisterConsoleResultWriterAttributeType =>
		lazyRegisterConsoleResultWriterAttributeType.Value;

	public INamedTypeSymbol? RegisterConsoleResultWriterAttributeOfTType =>
		lazyRegisterConsoleResultWriterAttributeOfTType.Value;

	public INamedTypeSymbol? RegisterMicrosoftTestingPlatformResultWriterAttributeType =>
		lazyRegisterMicrosoftTestingPlatformResultWriterAttributeType.Value;

	public INamedTypeSymbol? RegisterMicrosoftTestingPlatformResultWriterAttributeOfTType =>
		lazyRegisterMicrosoftTestingPlatformResultWriterAttributeOfTType.Value;

	public INamedTypeSymbol? RegisterResultWriterAttributeType =>
		lazyRegisterResultWriterAttributeType.Value;

	public INamedTypeSymbol? RegisterResultWriterAttributeOfTType =>
		lazyRegisterResultWriterAttributeOfTType.Value;

	public INamedTypeSymbol? RegisterRunnerReporterAttributeType =>
		lazyRegisterRunnerReporterAttributeType.Value;

	public INamedTypeSymbol? RegisterRunnerReporterAttributeOfTType =>
		lazyRegisterRunnerReporterAttributeOfTType.Value;

	public Version Version { get; }
}
