using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
	public class ExecutionContext
	{
		const string assemblyPrefix = "xunit.execution.";
		readonly Lazy<INamedTypeSymbol> lazyLongLivedMarshalByRefObjectType;

		public ExecutionContext(
			Compilation compilation,
			Version versionOverride = null)
		{
			var assembly = compilation.ReferencedAssemblyNames.FirstOrDefault(a => a.Name.StartsWith(assemblyPrefix, StringComparison.OrdinalIgnoreCase));

			Platform = assembly?.Name?.Substring(assemblyPrefix.Length);
			Version = versionOverride ?? assembly?.Version;

			lazyLongLivedMarshalByRefObjectType = new Lazy<INamedTypeSymbol>(() => compilation.GetTypeByMetadataName(Constants.Types.XunitLongLivedMarshalByRefObject));
		}

		public INamedTypeSymbol LongLivedMarshalByRefObjectType =>
			lazyLongLivedMarshalByRefObjectType?.Value;

		public string Platform { get; }

		public Version Version { get; }
	}
}
