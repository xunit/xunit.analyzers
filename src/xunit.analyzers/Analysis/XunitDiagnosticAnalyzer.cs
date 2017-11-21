using System;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public abstract class XunitDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        readonly Version versionOverride;

        public XunitDiagnosticAnalyzer() { }

        /// <summary>For testing purposes only.</summary>
        public XunitDiagnosticAnalyzer(Version versionOverride)
        {
            this.versionOverride = versionOverride;
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var xunitContext = new XunitContext(compilationStartContext.Compilation, versionOverride);
                if (ShouldAnalyze(xunitContext))
                    AnalyzeCompilation(compilationStartContext, xunitContext);
            });
        }

        protected virtual bool ShouldAnalyze(XunitContext xunitContext)
            => xunitContext.HasCoreReference;

        internal abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext);
    }
}
