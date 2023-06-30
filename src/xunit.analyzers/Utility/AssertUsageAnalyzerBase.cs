using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

public abstract class AssertUsageAnalyzerBase : XunitDiagnosticAnalyzer
{
	readonly HashSet<string> targetMethods;

	protected AssertUsageAnalyzerBase(
		DiagnosticDescriptor descriptor,
		IEnumerable<string> methods)
			: this(new[] { descriptor }, methods)
	{ }

	protected AssertUsageAnalyzerBase(
		DiagnosticDescriptor[] descriptors,
		IEnumerable<string> methods) :
			base(descriptors) =>
				targetMethods = new HashSet<string>(methods, StringComparer.Ordinal);

	public sealed override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
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
