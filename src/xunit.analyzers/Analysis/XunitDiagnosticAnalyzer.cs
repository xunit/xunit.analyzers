using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public abstract class XunitDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        readonly XunitCapabilitiesFactory capabilitiesFactory;

        protected XunitDiagnosticAnalyzer() : this(null) { }

        protected XunitDiagnosticAnalyzer(XunitCapabilities capabilities = null)
        {
            if (capabilities == null)
                capabilitiesFactory = XunitCapabilities.Create;
            else
                capabilitiesFactory = c => capabilities;
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var xunitContext = new XunitContext(compilationStartContext.Compilation, capabilitiesFactory);
                if (ShouldAnalyze(xunitContext))
                    AnalyzeCompilation(compilationStartContext, xunitContext);
            });
        }

        protected virtual bool ShouldAnalyze(XunitContext xunitContext)
            => xunitContext.HasCoreReference;

        internal abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext);
    }
}
