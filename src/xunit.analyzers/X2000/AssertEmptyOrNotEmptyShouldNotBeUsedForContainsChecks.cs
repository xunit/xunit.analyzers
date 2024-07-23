using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecks : AssertUsageAnalyzerBase
{
	const string linqWhereMethod = "System.Linq.Enumerable.Where<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)";

	static readonly DiagnosticDescriptor[] targetDescriptors =
	[
		Descriptors.X2029_AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck,
		Descriptors.X2030_AssertNotEmptyShouldNotBeUsedForCollectionContainsCheck,
	];

	static readonly string[] targetMethods =
	[
		Constants.Asserts.Empty,
		Constants.Asserts.NotEmpty,
	];

	public AssertEmptyOrNotEmptyShouldNotBeUsedForContainsChecks() : 
		base(
			targetDescriptors,
			targetMethods
		)
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

		var descriptor = method.Name == Constants.Asserts.Empty
			? targetDescriptors[0]
			: targetDescriptors[1];

		context.ReportDiagnostic(
			Diagnostic.Create(
				descriptor,
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
