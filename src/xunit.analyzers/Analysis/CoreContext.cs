using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    public class CoreContext
    {
        static readonly Version Version_2_2_0 = new Version("2.2.0");
        static readonly Version Version_2_4_0 = new Version("2.4.0");

        readonly Lazy<INamedTypeSymbol> lazyClassDataAttributeType;
        readonly Lazy<INamedTypeSymbol> lazyDataAttributeType;
        readonly Lazy<INamedTypeSymbol> lazyFactAttributeType;
        readonly Lazy<INamedTypeSymbol> lazyInlineDataAttributeType;
        readonly Lazy<INamedTypeSymbol> lazyMemberDataAttributeType;
        readonly Lazy<INamedTypeSymbol> lazyTheoryAttributeType;

        public CoreContext(Compilation compilation, Version versionOverride = null)
        {
            Version = versionOverride ?? compilation.ReferencedAssemblyNames
                                                    .FirstOrDefault(a => a.Name.Equals("xunit.core", StringComparison.OrdinalIgnoreCase))
                                                   ?.Version;

            lazyClassDataAttributeType = new Lazy<INamedTypeSymbol>(() => compilation.GetTypeByMetadataName(Constants.Types.XunitClassDataAttribute));
            lazyDataAttributeType = new Lazy<INamedTypeSymbol>(() => compilation.GetTypeByMetadataName(Constants.Types.XunitSdkDataAttribute));
            lazyFactAttributeType = new Lazy<INamedTypeSymbol>(() => compilation.GetTypeByMetadataName(Constants.Types.XunitFactAttribute));
            lazyInlineDataAttributeType = new Lazy<INamedTypeSymbol>(() => compilation.GetTypeByMetadataName(Constants.Types.XunitInlineDataAttribute));
            lazyMemberDataAttributeType = new Lazy<INamedTypeSymbol>(() => compilation.GetTypeByMetadataName(Constants.Types.XunitMemberDataAttribute));
            lazyTheoryAttributeType = new Lazy<INamedTypeSymbol>(() => compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute));
        }

        public INamedTypeSymbol ClassDataAttributeType
            => lazyClassDataAttributeType?.Value;

        public INamedTypeSymbol DataAttributeType
            => lazyDataAttributeType?.Value;

        public INamedTypeSymbol FactAttributeType
            => lazyFactAttributeType?.Value;

        public INamedTypeSymbol InlineDataAttributeType
            => lazyInlineDataAttributeType?.Value;

        public INamedTypeSymbol MemberDataAttributeType
            => lazyMemberDataAttributeType?.Value;

        public INamedTypeSymbol TheoryAttributeType
            => lazyTheoryAttributeType?.Value;

        public virtual bool TheorySupportsParameterArrays
            => Version >= Version_2_2_0;

        public virtual bool TheorySupportsDefaultParameterValues 
            => Version >= Version_2_2_0;



        /// <summary>
        /// See: https://github.com/xunit/xunit/pull/1546
        /// </summary>
        public virtual bool TheorySupportsConversionFromStringToDateTimeOffsetAndGuid
            => Version >= Version_2_4_0;

        public Version Version { get; set; }
    }
}
