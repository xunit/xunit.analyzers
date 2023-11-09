using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AsyncAssertsShouldBeAwaited : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	{
		Constants.Asserts.PropertyChangedAsync,
		Constants.Asserts.RaisesAnyAsync,
		Constants.Asserts.RaisesAsync,
		Constants.Asserts.ThrowsAnyAsync,
		Constants.Asserts.ThrowsAsync,
	};

	public AsyncAssertsShouldBeAwaited() :
		base(Descriptors.X2021_AsyncAssertionsShouldBeAwaited, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		XunitContext xunitContext,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		var taskType = TypeSymbolFactory.Task(context.Compilation);
		var taskOfTType = TypeSymbolFactory.TaskOfT(context.Compilation)?.ConstructUnboundGenericType();

		for (IOperation? current = invocationOperation; current is not null; current = current?.Parent)
		{
			// Stop looking when we've hit the enclosing block
			if (current is IBlockOperation)
				return;

			// Only interested in something that results in an expression to a named type
			if (current is not IExpressionStatementOperation expression || expression.Operation.Type is not INamedTypeSymbol namedReturnType)
				continue;

			if (namedReturnType.IsGenericType)
			{
				// Does it return Task<T>?
				if (!SymbolEqualityComparer.Default.Equals(namedReturnType.ConstructUnboundGenericType(), taskOfTType))
					continue;
			}
			else
			{
				// Does it return Task?
				if (!SymbolEqualityComparer.Default.Equals(namedReturnType, taskType))
					continue;
			}

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2021_AsyncAssertionsShouldBeAwaited,
					invocationOperation.Syntax.GetLocation(),
					method.Name
				)
			);
		}
	}
}
