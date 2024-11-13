using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit.Analyzers;

namespace Xunit.Suppressors;

/// <summary>
/// Base class for diagnostic suppressors which support xUnit.net v2 and v3.
/// </summary>
public abstract class XunitDiagnosticSuppressor(SuppressionDescriptor descriptor) :
	DiagnosticSuppressor
{
	protected SuppressionDescriptor Descriptor => SupportedSuppressions[0];

	/// <inheritdoc/>
#pragma warning disable IDE0305  // Cannot convert this due to Roslyn 3.11 vs. 4.11 dependencies
	public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions { get; } = new[] { descriptor }.ToImmutableArray();
#pragma warning restore IDE0305

	/// <summary>
	/// Override this factory method to influence the creation of <see cref="XunitContext"/>.
	/// Typically used by derived classes wanting to provide version overrides for specific
	/// references.
	/// </summary>
	/// <param name="compilation">The Roslyn compilation context</param>
	protected virtual XunitContext CreateXunitContext(Compilation compilation) =>
		new(compilation);

	/// <inheritdoc/>
	public sealed override void ReportSuppressions(SuppressionAnalysisContext context)
	{
		var xunitContext = CreateXunitContext(context.Compilation);

		if (ShouldAnalyze(xunitContext))
			foreach (var diagnostic in context.ReportedDiagnostics)
				if (ShouldSuppress(diagnostic, context, xunitContext))
					context.ReportSuppression(Suppression.Create(Descriptor, diagnostic));
	}

	/// <summary>
	/// Override this method to influence when we should consider diagnostic analysis. By
	/// default analyzes all assemblies that have a reference to xUnit.net v2 or v3.
	/// </summary>
	/// <param name="xunitContext">The xUnit.net context</param>
	/// <returns>Return <c>true</c> to analyze source; return <c>false</c> to skip analysis</returns>
	protected virtual bool ShouldAnalyze(XunitContext xunitContext) =>
		Guard.ArgumentNotNull(xunitContext).HasV2References || xunitContext.HasV3References;

	/// <summary>
	/// Analyzes the given diagnostic to determine if it should be suppressed.
	/// </summary>
	/// <param name="diagnostic">The diagnostic to analyze</param>
	/// <param name="context">The Roslyn supression analysis context</param>
	/// <param name="xunitContext">The xUnit.net context</param>
	protected abstract bool ShouldSuppress(
		Diagnostic diagnostic,
		SuppressionAnalysisContext context,
		XunitContext xunitContext);
}
