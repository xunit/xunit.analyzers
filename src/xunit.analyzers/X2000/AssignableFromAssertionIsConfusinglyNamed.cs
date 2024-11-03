using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssignableFromAssertionIsConfusinglyNamed : AssertUsageAnalyzerBase
{
	public static readonly Dictionary<string, string> ReplacementMethods = new()
	{
		{ Constants.Asserts.IsAssignableFrom, Constants.Asserts.IsType },
		{ Constants.Asserts.IsNotAssignableFrom, Constants.Asserts.IsNotType },
	};

	public AssignableFromAssertionIsConfusinglyNamed() :
		base(Descriptors.X2032_AssignableFromAssertionIsConfusinglyNamed, ReplacementMethods.Keys)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		XunitContext xunitContext,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		Guard.ArgumentNotNull(xunitContext);
		Guard.ArgumentNotNull(invocationOperation);
		Guard.ArgumentNotNull(method);

		if (!xunitContext.Assert.SupportsInexactTypeAssertions)
			return;

		if (!ReplacementMethods.TryGetValue(invocationOperation.TargetMethod.Name, out var replacement))
			return;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2032_AssignableFromAssertionIsConfusinglyNamed,
				invocationOperation.Syntax.GetLocation(),
				invocationOperation.TargetMethod.Name,
				replacement
			)
		);
	}
}
