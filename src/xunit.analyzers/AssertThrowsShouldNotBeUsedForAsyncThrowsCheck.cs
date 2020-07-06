﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertThrowsShouldNotBeUsedForAsyncThrowsCheck : AssertUsageAnalyzerBase
	{
		internal const string MethodName = "MethodName";

		private static readonly HashSet<string> ObsoleteMethods = new HashSet<string>
		{
			"Xunit.Assert.Throws<System.NotImplementedException>(System.Func<System.Threading.Tasks.Task>)",
			"Xunit.Assert.Throws<System.ArgumentException>(string, System.Func<System.Threading.Tasks.Task>)"
		};

		public AssertThrowsShouldNotBeUsedForAsyncThrowsCheck()
			: base(new[] { Descriptors.X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck, Descriptors.X2019_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck }, new[] { "Throws", "ThrowsAny" })
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, IMethodSymbol method)
		{
			if (invocationOperation.Arguments.Length < 1 || invocationOperation.Arguments.Length > 2)
				return;

			var throwExpressionSymbol = GetThrowExpressionSymbol(context, invocationOperation);
			if (!ThrowExpressionReturnsTask(throwExpressionSymbol, context))
				return;

			var descriptor = ObsoleteMethods.Contains(SymbolDisplay.ToDisplayString(method))
				? Descriptors.X2019_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck
				: Descriptors.X2014_AssertThrowsShouldNotBeUsedForAsyncThrowsCheck;

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[MethodName] = method.Name;
			context.ReportDiagnostic(
				Diagnostic.Create(
				descriptor,
				invocationOperation.Syntax.GetLocation(),
				builder.ToImmutable(),
				SymbolDisplay.ToDisplayString(
					method,
					SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None).WithGenericsOptions(SymbolDisplayGenericsOptions.None))));
		}

		private static ISymbol GetThrowExpressionSymbol(OperationAnalysisContext context, IInvocationOperation invocationOperation)
		{
			var argument = invocationOperation.Arguments.Last().Value;
			if (argument is IDelegateCreationOperation delegateCreation)
			{
				if (delegateCreation.Target is IAnonymousFunctionOperation anonymousFunction)
				{
					IOperation symbolOperation = null;
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
					{
						symbolOperation = awaitOperation.Operation.WalkDownImplicitConversions();
					}

					if (symbolOperation is IInvocationOperation symbolInvoke)
					{
						return symbolInvoke.TargetMethod;
					}
					else if (symbolOperation is ILiteralOperation)
					{
						return null;
					}
				}
				else if (delegateCreation.Target is IMethodReferenceOperation methodReference)
				{
					return methodReference.Method;
				}
			}

#if DEBUG
			// Fall back to SemanticModel analysis. This should never return a result different from the above.
			var invocation = (InvocationExpressionSyntax)invocationOperation.Syntax;
			var argumentExpression = invocation.ArgumentList.Arguments.Last().Expression;
			var semanticModel = context.Compilation.GetSemanticModel(context.Operation.Syntax.SyntaxTree);

			if (!(argumentExpression is LambdaExpressionSyntax lambdaExpression))
				return semanticModel.GetSymbolInfo(argumentExpression).Symbol;

			if (!(lambdaExpression.Body is AwaitExpressionSyntax awaitExpression))
				return semanticModel.GetSymbolInfo(lambdaExpression.Body).Symbol;

			return semanticModel.GetSymbolInfo(awaitExpression.Expression).Symbol;
#else
			return null;
#endif
		}

		private static bool ThrowExpressionReturnsTask(ISymbol symbol, OperationAnalysisContext context)
		{
			if (symbol?.Kind != SymbolKind.Method)
				return false;

			var taskType = context.Compilation.GetTypeByMetadataName(Constants.Types.SystemThreadingTasksTask);
			var returnType = ((IMethodSymbol)symbol).ReturnType;
			if (taskType.IsAssignableFrom(returnType))
				return true;

			var configuredTaskAwaitableType = context.Compilation.GetTypeByMetadataName(Constants.Types.SystemRuntimeCompilerServicesConfiguredTaskAwaitable);
			if (configuredTaskAwaitableType.IsAssignableFrom(returnType))
				return true;

			return false;
		}
	}
}
