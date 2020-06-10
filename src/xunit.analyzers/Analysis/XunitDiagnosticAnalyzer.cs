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
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(context =>
			{
				var xunitContext = new XunitContext(context.Compilation, versionOverride);
				if (ShouldAnalyze(xunitContext))
					AnalyzeCompilation(context, xunitContext);
			});
		}

		protected virtual bool ShouldAnalyze(XunitContext xunitContext)
			=> xunitContext.HasCoreReference;

		internal abstract void AnalyzeCompilation(CompilationStartAnalysisContext context, XunitContext xunitContext);
	}
}
