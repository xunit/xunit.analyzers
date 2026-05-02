using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

/// <summary>
/// Base class for diagnostic analyzers which support xUnit.net v3 (in reflection mode) only.
/// </summary>
public abstract class XunitV3NonAotDiagnosticAnalyzer(params DiagnosticDescriptor[] descriptors) :
	XunitDiagnosticAnalyzer(descriptors)
{
	protected override bool ShouldAnalyze(XunitContext xunitContext) =>
		Guard.ArgumentNotNull(xunitContext).HasV3NonAotReferences;
}
