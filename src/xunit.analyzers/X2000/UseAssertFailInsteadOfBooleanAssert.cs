using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseAssertFailInsteadOfBooleanAssert : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	{
		Constants.Asserts.False,
		Constants.Asserts.True,
	};
	static readonly Dictionary<string, bool> targetValues = new()
	{
		{ Constants.Asserts.False, true },
		{ Constants.Asserts.True, false },
	};

	public UseAssertFailInsteadOfBooleanAssert()
		: base(Descriptors.X2020_UseAssertFailInsteadOfBooleanAssert, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		XunitContext xunitContext,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		var arguments = invocationOperation.Arguments;
		if (arguments.Length != 2)
			return;

		if (!targetValues.TryGetValue(method.Name, out var targetValue))
			return;

		if (arguments.FirstOrDefault(arg => arg.Parameter?.Ordinal == 0)?.Value is not ILiteralOperation literalFirstArgument)
			return;
		if (!literalFirstArgument.ConstantValue.HasValue)
			return;
		if (!Equals(literalFirstArgument.ConstantValue.Value, targetValue))
			return;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2020_UseAssertFailInsteadOfBooleanAssert,
				invocationOperation.Syntax.GetLocation(),
				method.Name,
				targetValue ? "true" : "false"
			)
		);
	}

	protected override bool ShouldAnalyze(XunitContext xunitContext) =>
		xunitContext.Assert.SupportsAssertFail;
}
