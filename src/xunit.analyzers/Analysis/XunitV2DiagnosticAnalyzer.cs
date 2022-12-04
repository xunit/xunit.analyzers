namespace Xunit.Analyzers
{
	/// <summary>
	/// Base class for diagnostic analyzers which support xUnit.net v2 only.
	/// </summary>
	public abstract class XunitV2DiagnosticAnalyzer : XunitDiagnosticAnalyzer
	{
		protected override bool ShouldAnalyze(XunitContext xunitContext) =>
			xunitContext.HasV2References;
	}
}
