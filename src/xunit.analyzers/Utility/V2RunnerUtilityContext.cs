using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V2RunnerUtilityContext : IRunnerUtilityContext
{
	const string assemblyPrefix = "xunit.runner.utility.";
	readonly Lazy<INamedTypeSymbol?> lazyLongLivedMarshalByRefObjectType;

	V2RunnerUtilityContext(
		Compilation compilation,
		string platform,
		Version version)
	{
		Platform = platform;
		Version = version;

		lazyLongLivedMarshalByRefObjectType = new(() => TypeSymbolFactory.LongLivedMarshalByRefObject_RunnerUtilityV2(compilation));
	}

	public INamedTypeSymbol? LongLivedMarshalByRefObjectType =>
		lazyLongLivedMarshalByRefObjectType.Value;

	public string Platform { get; }

	public Version Version { get; set; }

	public static V2RunnerUtilityContext? Get(
		Compilation compilation,
		Version? versionOverride = null)
	{
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
