using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public class V2ExecutionContext
{
	const string assemblyPrefix = "xunit.execution.";
	readonly Lazy<INamedTypeSymbol?> lazyLongLivedMarshalByRefObjectType;

	V2ExecutionContext(
		Compilation compilation,
		string platform,
		Version version)
	{
		Platform = platform;
		Version = version;

		lazyLongLivedMarshalByRefObjectType = new(() => TypeSymbolFactory.LongLivedMarshalByRefObject_ExecutionV2(compilation));
	}

	/// <summary>
	/// Gets a reference to type <c>Xunit.LongLivedMarshalByRefObject</c>, if available.
	/// </summary>
	public INamedTypeSymbol? LongLivedMarshalByRefObjectType =>
		lazyLongLivedMarshalByRefObjectType.Value;

	/// <summary>
	/// Gets a description of the target platform for the execution library (i.e., "desktop"). This is
	/// typically extracted from the assembly name (i.e., "xunit.execution.desktop").
	/// </summary>
	public string Platform { get; }

	/// <summary>
	/// Gets the version number of the execution assembly.
	/// </summary>
	public Version Version { get; }

	public static V2ExecutionContext? Get(
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
