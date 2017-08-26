using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public abstract class XunitDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        readonly XunitCapabilitiesFactory capabilitiesFactory;

        protected XunitDiagnosticAnalyzer() : this(null) { }

        protected XunitDiagnosticAnalyzer(XunitCapabilities capabilities = null) {
            if (capabilities == null)
                capabilitiesFactory = XunitCapabilities.Create;
            else
                capabilitiesFactory = c => capabilities;
        }

        public class XunitContext
        {
            internal XunitContext(Compilation compilation, XunitCapabilitiesFactory capabilitiesFactory)
            {
                Capabilities = capabilitiesFactory(compilation);
                Compilation = compilation;
            }

            public XunitCapabilities Capabilities { get; }
            public Compilation Compilation { get; }

            public INamedTypeSymbol FactAttributeType => Compilation.GetTypeByMetadataName(Constants.Types.XunitFactAttribute);
            public INamedTypeSymbol TheoryAttributeType => Compilation.GetTypeByMetadataName(Constants.Types.XunitTheoryAttribute);
            public INamedTypeSymbol DataAttributeType => Compilation.GetTypeByMetadataName(Constants.Types.XunitSdkDataAttribute);
            public INamedTypeSymbol InlineDataAttributeType => Compilation.GetTypeByMetadataName(Constants.Types.XunitInlineDataAttribute);
            public INamedTypeSymbol ClassDataAttributeType => Compilation.GetTypeByMetadataName(Constants.Types.XunitClassDataAttribute);
            public INamedTypeSymbol MemberDataAttributeType => Compilation.GetTypeByMetadataName(Constants.Types.XunitMemberDataAttribute);
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var xunitContext = new XunitContext(compilationStartContext.Compilation, capabilitiesFactory);
                if (xunitContext.FactAttributeType != null && ShouldAnalzye(xunitContext))
                    AnalyzeCompilation(compilationStartContext, xunitContext);
            });
        }

        protected virtual bool ShouldAnalzye(XunitContext xunitContext) => true;

        internal abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext);
    }
}
