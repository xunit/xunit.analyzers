using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheck : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	{
		Constants.Asserts.Throws,
		Constants.Asserts.ThrowsAny,
	};

	public AssertThrowsShouldNotBeUsedForAsyncThrowsCheck()
		: base(new[] { Descriptors.X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck }, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		if (invocationOperation.Arguments.Length < 1 || invocationOperation.Arguments.Length > 2)
			return;

		var throwExpressionSymbol = GetThrowExpressionSymbol(invocationOperation);
		if (!ThrowExpressionReturnsTask(throwExpressionSymbol, context))
			return;

		var replacement = method.Name + "Async";

		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		builder[Constants.Properties.MethodName] = method.Name;
		builder[Constants.Properties.Replacement] = replacement;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck,
				invocationOperation.Syntax.GetLocation(),
				builder.ToImmutable(),
				SymbolDisplay.ToDisplayString(
					method,
					SymbolDisplayFormat
						.CSharpShortErrorMessageFormat
						.WithParameterOptions(SymbolDisplayParameterOptions.None)
						.WithGenericsOptions(SymbolDisplayGenericsOptions.None)
				),
				replacement
			)
		);
	}

	static ISymbol? GetThrowExpressionSymbol(IInvocationOperation invocationOperation)
	{
		var argument = invocationOperation.Arguments.LastOrDefault()?.Value;

		if (argument is IDelegateCreationOperation delegateCreation)
		{
			if (delegateCreation.Target is IAnonymousFunctionOperation anonymousFunction)
			{
				IOperation? symbolOperation = null;
				if (anonymousFunction.Body.Operations.Length == 2
					&& anonymousFunction.Body.Operations[0] is IExpressionStatementOperation expressionStatement
					&& anonymousFunction.Body.Operations[1] is IReturnOperation { ReturnedValue: null })
				{
					symbolOperation = expressionStatement.Operation;
				}
				else if (anonymousFunction.Body.Operations.Length == 1
					&& anonymousFunction.Body.Operations[0] is IReturnOperation { ReturnedValue: { } returnedValue })
				{
					symbolOperation = returnedValue.WalkDownImplicitConversions();
				}

				if (symbolOperation is IAwaitOperation awaitOperation)
					symbolOperation = awaitOperation.Operation.WalkDownImplicitConversions();
				if (symbolOperation is IInvocationOperation symbolInvoke)
					return symbolInvoke.TargetMethod;
				else if (symbolOperation is ILiteralOperation)
					return null;
			}
			else if (delegateCreation.Target is IMethodReferenceOperation methodReference)
				return methodReference.Method;
		}

		return null;
	}

	static bool ThrowExpressionReturnsTask(
		ISymbol? symbol,
		OperationAnalysisContext context)
	{
		if (symbol?.Kind != SymbolKind.Method)
			return false;

		var taskType = context.Compilation.GetTypeByMetadataName(Constants.Types.SystemThreadingTasksTask);
		if (symbol is not IMethodSymbol methodSymbol)
			return false;

		var returnType = methodSymbol.ReturnType;
		if (taskType.IsAssignableFrom(returnType))
			return true;

		var configuredTaskAwaitableType = context.Compilation.GetTypeByMetadataName(Constants.Types.SystemRuntimeCompilerServicesConfiguredTaskAwaitable);
		if (configuredTaskAwaitableType.IsAssignableFrom(returnType))
			return true;

		return false;
	}
}
