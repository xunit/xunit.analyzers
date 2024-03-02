using Microsoft.CodeAnalysis;
using Xunit.Analyzers;

namespace Xunit.Suppressors;

/// <summary>
/// Base class for diagnostic suppressors which support xUnit.net v3 only.
/// </summary>
public abstract class XunitV3DiagnosticSuppressor : XunitDiagnosticSuppressor
{
	protected XunitV3DiagnosticSuppressor(SuppressionDescriptor descriptor) :
		base(descriptor)
	{ }

	protected override bool ShouldAnalyze(XunitContext xunitContext) =>
		Guard.ArgumentNotNull(xunitContext).HasV3References;
}
