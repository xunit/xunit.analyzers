using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V3RunnerUtilityAotContext : IRunnerUtilityContextV3
{
	readonly Lazy<INamedTypeSymbol?> lazyLongLivedMarshalByRefObjectType;

	V3RunnerUtilityAotContext(
		Compilation compilation,
		Version version)
	{
		Version = version;

		lazyLongLivedMarshalByRefObjectType = new(() => TypeSymbolFactory.LongLivedMarshalByRefObject_RunnerUtility(compilation));
	}

	/// <inheritdoc/>
	public INamedTypeSymbol? LongLivedMarshalByRefObjectType =>
		lazyLongLivedMarshalByRefObjectType.Value;

	/// <inheritdoc/>
	public string Platform => "aot";

	/// <inheritdoc/>
	public Version Version { get; }

	public static IRunnerUtilityContextV3? Get(
		Compilation compilation,
		Version? versionOverride = null)
	{
		Guard.ArgumentNotNull(compilation);

		var assembly =
			compilation
				.ReferencedAssemblyNames
				.FirstOrDefault(a => a.Name.Equals("xunit.v3.runner.utility.aot", StringComparison.OrdinalIgnoreCase));

		if (assembly is null)
			return null;

		var version = versionOverride ?? assembly.Version;

		return version is null ? null : new V3RunnerUtilityAotContext(compilation, version);
	}
}
