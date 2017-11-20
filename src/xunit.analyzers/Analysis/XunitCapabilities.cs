using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    public delegate XunitCapabilities XunitCapabilitiesFactory(Compilation compilation);

    public class XunitCapabilities
    {
        readonly Version xunitCoreVersion;

        // For mocking only
        protected XunitCapabilities()
        {
        }

        public XunitCapabilities(Version xunitCoreVersion)
        {
            this.xunitCoreVersion = xunitCoreVersion;
        }

        public virtual bool TheorySupportsParameterArrays => xunitCoreVersion >= new Version(2, 2, 0, 0);

        public virtual bool TheorySupportsDefaultParameterValues => xunitCoreVersion >= new Version(2, 2, 0, 0);

        public static XunitCapabilities Create(Compilation compilation)
        {
            var xunitVersion = compilation
                .ReferencedAssemblyNames
                .FirstOrDefault(a => a.Name.Equals("xunit.core", StringComparison.OrdinalIgnoreCase))
                ?.Version
                ?? new Version("2.0.0.0");

            return new XunitCapabilities(xunitVersion);
        }
    }
}
