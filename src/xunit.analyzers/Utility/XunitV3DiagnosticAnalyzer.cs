using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

/// <summary>
/// Base class for diagnostic analyzers which support xUnit.net v3 only.
/// </summary>
public abstract class XunitV3DiagnosticAnalyzer : XunitDiagnosticAnalyzer
{
	public XunitV3DiagnosticAnalyzer(params DiagnosticDescriptor[] descriptors) :
		base(descriptors)
	{ }

	protected override bool ShouldAnalyze(XunitContext xunitContext) =>
		xunitContext.HasV3References;
}
