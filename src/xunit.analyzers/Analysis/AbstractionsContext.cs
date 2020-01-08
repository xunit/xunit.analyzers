using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
	public class AbstractionsContext
	{
		readonly Lazy<INamedTypeSymbol> lazyITestCaseType;
		readonly Lazy<INamedTypeSymbol> lazyIXunitSerializableType;

		public AbstractionsContext(Compilation compilation, Version versionOverride)
		{
			Version =
				versionOverride ??
				compilation.ReferencedAssemblyNames
					.FirstOrDefault(a => a.Name.Equals("xunit.abstractions", StringComparison.OrdinalIgnoreCase))
					?.Version;

			lazyITestCaseType = new Lazy<INamedTypeSymbol>(() => compilation.GetTypeByMetadataName(Constants.Types.XunitAbstractionsITestCase));
			lazyIXunitSerializableType = new Lazy<INamedTypeSymbol>(() => compilation.GetTypeByMetadataName(Constants.Types.XunitAbstractionsIXunitSerializableType));
		}

		public INamedTypeSymbol ITestCaseType
			=> lazyITestCaseType?.Value;

		public INamedTypeSymbol IXunitSerializableType
			=> lazyIXunitSerializableType?.Value;

		public Version Version { get; }
	}
}
