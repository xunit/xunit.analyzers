using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V3RunnerUtilityContext : IRunnerUtilityContext
{
	const string assemblyPrefix = "xunit.v3.runner.utility.";
	readonly Lazy<INamedTypeSymbol?> lazyLongLivedMarshalByRefObjectType;

	V3RunnerUtilityContext(
		Compilation compilation,
		string platform,
		Version version)
	{
		Platform = platform;
		Version = version;

		lazyLongLivedMarshalByRefObjectType = new(() => TypeSymbolFactory.LongLivedMarshalByRefObject_RunnerUtility(compilation));
	}

	/// <inheritdoc/>
	public INamedTypeSymbol? LongLivedMarshalByRefObjectType =>
		lazyLongLivedMarshalByRefObjectType.Value;

	/// <inheritdoc/>
	public string Platform { get; }

	/// <inheritdoc/>
	public Version Version { get; }

	public static V3RunnerUtilityContext? Get(
		Compilation compilation,
		Version? versionOverride = null)
	{
		Guard.ArgumentNotNull(compilation);

		var assembly =
			compilation
				.ReferencedAssemblyNames
				.FirstOrDefault(a => a.Name.StartsWith(assemblyPrefix, StringComparison.OrdinalIgnoreCase));

		if (assembly is null)
			return null;

		var version = versionOverride ?? assembly.Version;
		var platform = assembly.Name.Substring(assemblyPrefix.Length);

		return version is null ? null : new(compilation, platform, version);
	}
}
