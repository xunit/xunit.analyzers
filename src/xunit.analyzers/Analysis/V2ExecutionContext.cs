using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
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

			lazyLongLivedMarshalByRefObjectType = new(() => compilation.GetTypeByMetadataName(Constants.Types.XunitLongLivedMarshalByRefObject));
		}

		public INamedTypeSymbol? LongLivedMarshalByRefObjectType =>
			lazyLongLivedMarshalByRefObjectType.Value;

		public string Platform { get; }

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
}
