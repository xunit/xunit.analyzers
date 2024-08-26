using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertSingleShouldUseTwoArgumentCall : AssertUsageAnalyzerBase
{
	const string linqWhereMethod = "System.Linq.Enumerable.Where<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)";

	static readonly string[] targetMethods =
	[
		Constants.Asserts.Single,
	];

	public AssertSingleShouldUseTwoArgumentCall() :
		base(Descriptors.X2031_AssertSingleShouldUseTwoArgumentCall, targetMethods)
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

		var arguments = invocationOperation.Arguments;
		if (arguments.Length != 1)
			return;

		var argument = arguments[0];
		var value = argument.Value;
		if (value is IConversionOperation conversion)
			value = conversion.Operand;

		if (value is not IInvocationOperation innerInvocation)
			return;

		var originalMethod = SymbolDisplay.ToDisplayString(innerInvocation.TargetMethod.OriginalDefinition);
		if (originalMethod != linqWhereMethod)
			return;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2031_AssertSingleShouldUseTwoArgumentCall,
				invocationOperation.Syntax.GetLocation(),
				SymbolDisplay.ToDisplayString(
					method,
					SymbolDisplayFormat
						.CSharpShortErrorMessageFormat
						.WithParameterOptions(SymbolDisplayParameterOptions.None)
						.WithGenericsOptions(SymbolDisplayGenericsOptions.None)
				)
			)
		);
	}
}
