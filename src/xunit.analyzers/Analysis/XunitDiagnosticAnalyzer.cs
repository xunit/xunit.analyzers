using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
	/// <summary>
	/// Base class for diagnostic analyzers which support xUnit.net v2 and v3.
	/// </summary>
	public abstract class XunitDiagnosticAnalyzer : DiagnosticAnalyzer
	{
		/// <summary>
		/// Analyzes compilation to discover diagnostics.
		/// </summary>
		/// <param name="context">The Roslyn diagnostic context</param>
		/// <param name="xunitContext">The xUnit.net context</param>
		public abstract void AnalyzeCompilation(
			CompilationStartAnalysisContext context,
			XunitContext xunitContext);

		/// <summary>
		/// Override this factory method to influence the creation of <see cref="XunitContext"/>.
		/// Typically used by derived classes wanting to provide version overrides for specific
		/// references.
		/// </summary>
		/// <param name="compilation">The Roslyn compilation context</param>
		protected virtual XunitContext CreateXunitContext(Compilation compilation) =>
			new(compilation);

		/// <inheritdoc/>
		public sealed override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(context =>
			{
				var xunitContext = CreateXunitContext(context.Compilation);
				if (ShouldAnalyze(xunitContext))
					AnalyzeCompilation(context, xunitContext);
			});
		}

		/// <summary>
		/// Override this method to influence when we should consider diagnostic analysis. By
		/// default analyzes all assemblies that have a reference to xUnit.net v2 or v3.
		/// </summary>
		/// <param name="xunitContext">The xUnit.net context</param>
		/// <returns>Return <c>true</c> to analyze source; return <c>false</c> to skip analysis</returns>
		protected virtual bool ShouldAnalyze(XunitContext xunitContext) =>
			xunitContext.HasV2References || xunitContext.HasV3References;
	}
}
