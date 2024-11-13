using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

public abstract class AssertUsageAnalyzerBase(
	DiagnosticDescriptor[] descriptors,
	IEnumerable<string> methods) :
		XunitDiagnosticAnalyzer(descriptors)
{
	readonly HashSet<string> targetMethods = new(methods, StringComparer.Ordinal);

	protected AssertUsageAnalyzerBase(
		DiagnosticDescriptor descriptor,
		IEnumerable<string> methods)
			: this([descriptor], methods)
	{ }

	public sealed override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var assertType = TypeSymbolFactory.Assert(context.Compilation);
		if (assertType is null)
			return;

		context.RegisterOperationAction(context =>
		{
			if (context.Operation is IInvocationOperation invocationOperation)
			{
				var methodSymbol = invocationOperation.TargetMethod;
				if (methodSymbol.MethodKind != MethodKind.Ordinary || !SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, assertType) || !targetMethods.Contains(methodSymbol.Name))
					return;

				AnalyzeInvocation(context, xunitContext, invocationOperation, methodSymbol);
			}
		}, OperationKind.Invocation);
	}

	protected abstract void AnalyzeInvocation(
		OperationAnalysisContext context,
		XunitContext xunitContext,
		IInvocationOperation invocationOperation,
		IMethodSymbol method);
}
