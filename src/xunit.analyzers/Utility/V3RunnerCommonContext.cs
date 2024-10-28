using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V3RunnerCommonContext
{
	readonly Lazy<INamedTypeSymbol?> lazyIRunnerReporterType;

	V3RunnerCommonContext(
		Compilation compilation,
		Version version)
	{
		Version = version;

		lazyIRunnerReporterType = new(() => TypeSymbolFactory.IRunnerReporter_V3(compilation));
	}

	public INamedTypeSymbol? IRunnerReporterType =>
		lazyIRunnerReporterType.Value;

	/// <inheritdoc/>
	public Version Version { get; }

	public static V3RunnerCommonContext? Get(
		Compilation compilation,
		Version? versionOverride = null)
	{
		Guard.ArgumentNotNull(compilation);

		var assembly =
			compilation
				.ReferencedAssemblyNames
				.FirstOrDefault(a => a.Name.Equals("xunit.v3.runner.common", StringComparison.OrdinalIgnoreCase));

		if (assembly is null)
			return null;

		var version = versionOverride ?? assembly.Version;

		return version is null ? null : new(compilation, version);
	}
}
