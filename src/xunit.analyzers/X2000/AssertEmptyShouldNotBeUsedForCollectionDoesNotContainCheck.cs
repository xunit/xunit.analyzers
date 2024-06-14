using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using Xunit.Analyzers;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck : AssertUsageAnalyzerBase
{
	const string linqWhereMethod = "System.Linq.Enumerable.Where<TSource>(System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource, bool>)";

	static readonly string[] targetMethods =
	{
		Constants.Asserts.Empty,
	};

	public AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck()
		: base(Descriptors.X2029_AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck, targetMethods)
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
				Descriptors.X2029_AssertEmptyShouldNotBeUsedForCollectionDoesNotContainCheck,
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
