using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers
{
    public class XunitContext
    {
        readonly Lazy<AbstractionsContext> abstractions;
        readonly Lazy<CoreContext> core;
        readonly Lazy<ExecutionContext> execution;
        readonly Lazy<bool> hasAbstractionsReference;
        readonly Lazy<bool> hasCoreReference;
        readonly Lazy<bool> hasExecutionReference;

        internal XunitContext(Compilation compilation, XunitCapabilitiesFactory capabilitiesFactory)
        {
            Capabilities = capabilitiesFactory(compilation);
            Compilation = compilation;

            hasAbstractionsReference = new Lazy<bool>(() => Compilation.ReferencedAssemblyNames.FirstOrDefault(x => x.Name == "xunit.abstractions") != null);
            hasCoreReference = new Lazy<bool>(() => Compilation.ReferencedAssemblyNames.FirstOrDefault(x => x.Name == "xunit.core") != null);
            hasExecutionReference = new Lazy<bool>(() => Compilation.ReferencedAssemblyNames.FirstOrDefault(x => x.Name.StartsWith("xunit.execution.")) != null);

            abstractions = new Lazy<AbstractionsContext>(() => new AbstractionsContext(compilation));
            core = new Lazy<CoreContext>(() => new CoreContext(compilation));
            execution = new Lazy<ExecutionContext>(() => new ExecutionContext(compilation));
        }

        public AbstractionsContext Abstractions => abstractions.Value;
        public XunitCapabilities Capabilities { get; }
        public Compilation Compilation { get; }
        public CoreContext Core => core.Value;
        public ExecutionContext Execution => execution.Value;

        public bool HasAbstractionsReference => hasAbstractionsReference.Value;
        public bool HasCoreReference => hasCoreReference.Value;
        public bool HasExecutionReference => hasExecutionReference.Value;
    }
}
