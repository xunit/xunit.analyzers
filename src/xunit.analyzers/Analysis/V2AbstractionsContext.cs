using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
	public class V2AbstractionsContext
	{
		readonly Lazy<INamedTypeSymbol?> lazyITestCaseType;
		readonly Lazy<INamedTypeSymbol?> lazyIXunitSerializableType;

		V2AbstractionsContext(
			Compilation compilation,
			Version version)
		{
			Version = version;

			lazyITestCaseType = new(() => compilation.GetTypeByMetadataName(Constants.Types.XunitAbstractionsITestCase));
			lazyIXunitSerializableType = new(() => compilation.GetTypeByMetadataName(Constants.Types.XunitAbstractionsIXunitSerializableType));
		}

		public INamedTypeSymbol? ITestCaseType =>
			lazyITestCaseType.Value;

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
}
