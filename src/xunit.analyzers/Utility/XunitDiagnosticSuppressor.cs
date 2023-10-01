using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers.Utility;

public abstract class XunitDiagnosticSuppressor : DiagnosticSuppressor
{
	protected XunitDiagnosticSuppressor(params SuppressionDescriptor[] descriptors) =>
		SupportedSuppressions = descriptors.ToImmutableArray();

	public sealed override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; }

	public sealed override void ReportSuppressions(SuppressionAnalysisContext context)
	{
		var xunitContext = CreateXunitContext(context.Compilation);

		if (ShouldAnalyze(xunitContext))
		{
			ReportSuppressions(context, xunitContext);
		}
	}

	public abstract void ReportSuppressions(SuppressionAnalysisContext context, XunitContext xunitContext);

	/// <summary>
	/// Override this factory method to influence the creation of <see cref="XunitContext"/>.
	/// Typically used by derived classes wanting to provide version overrides for specific
	/// references.
	/// </summary>
	/// <param name="compilation">The Roslyn compilation context</param>
	protected virtual XunitContext CreateXunitContext(Compilation compilation) =>
		new(compilation);

	/// <summary>
	/// Override this method to influence when we should consider diagnostic analysis. By
	/// default analyzes all assemblies that have a reference to xUnit.net v2 or v3.
	/// </summary>
	/// <param name="xunitContext">The xUnit.net context</param>
	/// <returns>Return <c>true</c> to analyze source; return <c>false</c> to skip analysis</returns>
	protected virtual bool ShouldAnalyze(XunitContext xunitContext) =>
		xunitContext.HasV2References || xunitContext.HasV3References;

}
