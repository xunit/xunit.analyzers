using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotUseBlockingTaskOperations : XunitDiagnosticAnalyzer
{
	static readonly string[] blockingAwaiterMethods =
	[
		// We will "steal" the name from TaskAwaiter, but we look for it by pattern: if the type is
		// assignable from ICriticalNotifyCompletion and it contains a method with this name. We
		// also explicitly look for IValueTaskSource and IValueTaskSource<T>.
		nameof(IValueTaskSource.GetResult),
	];
	static readonly string[] blockingTaskMethods =
	[
		// These are only on Task, and not on ValueTask
		nameof(Task.Wait),
		nameof(Task.WaitAny),
		nameof(Task.WaitAll),
	];
	static readonly string[] blockingTaskProperties =
	[
		// These are on both Task<T> and ValueTask<T>
		nameof(Task<int>.Result),
	];
	static readonly string[] whenAll =
	[
		nameof(Task.WhenAll),
	];
	static readonly string[] whenAny =
	[
		nameof(Task.WhenAny),
	];

	public DoNotUseBlockingTaskOperations() :
		base(Descriptors.X1031_DoNotUseBlockingTaskOperations)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		if (xunitContext.Core.FactAttributeType is null || xunitContext.Core.TheoryAttributeType is null)
			return;

		var iCriticalNotifyCompletionType = TypeSymbolFactory.ICriticalNotifyCompletion(context.Compilation);
		var iValueTaskSourceType = TypeSymbolFactory.IValueTaskSource(context.Compilation);
		var iValueTaskSourceOfTType = TypeSymbolFactory.IValueTaskSourceOfT(context.Compilation);
		var taskType = TypeSymbolFactory.Task(context.Compilation);
		var taskOfTType = TypeSymbolFactory.TaskOfT(context.Compilation);
		var valueTaskOfTType = TypeSymbolFactory.ValueTaskOfT(context.Compilation);

		if (taskType is null)
			return;

		// Need to dynamically look for ICriticalNotifyCompletion, for use with blockingAwaiterMethods

		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IInvocationOperation invocation)
				return;

			var foundSymbol =
				FindSymbol(invocation.TargetMethod, invocation, taskType, blockingTaskMethods, xunitContext, out var foundSymbolName) ||
				FindSymbol(invocation.TargetMethod, invocation, iCriticalNotifyCompletionType, blockingAwaiterMethods, xunitContext, out foundSymbolName) ||
				FindSymbol(invocation.TargetMethod, invocation, iValueTaskSourceType, blockingAwaiterMethods, xunitContext, out foundSymbolName) ||
				FindSymbol(invocation.TargetMethod, invocation, iValueTaskSourceOfTType, blockingAwaiterMethods, xunitContext, out foundSymbolName);

			if (!foundSymbol)
				return;

			// Ignore anything inside a lambda expression or a local function
			for (var current = context.Operation; current is not null; current = current.Parent)
				if (current is IAnonymousFunctionOperation or ILocalFunctionOperation)
					return;

			var symbolsForSearch = default(IEnumerable<ILocalSymbol>);
			switch (foundSymbolName)
			{
				case nameof(IValueTaskSource.GetResult):
					{
						// Only handles 'task.GetAwaiter().GetResult()', which gives us direct access to the task
						// TODO: How hard would it be to trace back something like:
						//    var awaiter = task.GetAwaiter();
						//    var result = awaiter.GetResult();
						if (invocation.Instance is IInvocationOperation getAwaiterOperation &&
								getAwaiterOperation.TargetMethod.Name == nameof(Task.GetAwaiter) &&
								getAwaiterOperation.Instance is ILocalReferenceOperation localReferenceOperation)
							symbolsForSearch = [localReferenceOperation.Local];
						break;
					}

				case nameof(Task.Wait):
					{
						if (invocation.Instance is ILocalReferenceOperation localReferenceOperation)
							symbolsForSearch = [localReferenceOperation.Local];
						break;
					}

				case nameof(Task.WaitAll):
				case nameof(Task.WaitAny):
					{
						if (invocation.Arguments.Length == 1 &&
								invocation.Arguments[0].Value is IArrayCreationOperation arrayCreationOperation &&
								arrayCreationOperation.Initializer is not null)
							symbolsForSearch = arrayCreationOperation.Initializer.ElementValues.OfType<ILocalReferenceOperation>().Select(l => l.Local);
						break;
					}
			}

			if (symbolsForSearch is not null)
				if (TaskIsKnownToBeCompleted(invocation, symbolsForSearch, taskType, xunitContext))
					return;

			// Should have two child nodes: "(some other code).(target method)" and the arguments
			var invocationChildren = invocation.Syntax.ChildNodes().ToList();
			if (invocationChildren.Count != 2)
				return;

			// First child node should be split into two nodes: "(some other code)" and "(target method)"
			var methodCallChildren = invocationChildren[0].ChildNodes().ToList();
			if (methodCallChildren.Count != 2)
				return;

			// Construct a location that covers the target method and arguments
			var length = methodCallChildren[1].Span.Length + invocationChildren[1].Span.Length;
			var textSpan = new TextSpan(methodCallChildren[1].SpanStart, length);
			var location = Location.Create(invocation.Syntax.SyntaxTree, textSpan);
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.X1031_DoNotUseBlockingTaskOperations, location));
		}, OperationKind.Invocation);

		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IPropertyReferenceOperation reference)
				return;

			var foundSymbol =
				FindSymbol(reference.Property, reference, taskOfTType, blockingTaskProperties, xunitContext, out var foundSymbolName) ||
				FindSymbol(reference.Property, reference, valueTaskOfTType, blockingTaskProperties, xunitContext, out foundSymbolName);

			if (!foundSymbol)
				return;

			if (foundSymbolName == nameof(Task<int>.Result) &&
					reference.Instance is ILocalReferenceOperation localReferenceOperation &&
					TaskIsKnownToBeCompleted(reference, [localReferenceOperation.Local], taskType, xunitContext))
				return;

			// Should have two child nodes: "(some other code)" and "(property name)"
			var propertyChildren = reference.Syntax.ChildNodes().ToList();
			if (propertyChildren.Count != 2)
				return;

			var location = propertyChildren[1].GetLocation();
			context.ReportDiagnostic(Diagnostic.Create(Descriptors.X1031_DoNotUseBlockingTaskOperations, location));
		}, OperationKind.PropertyReference);
	}

	static bool FindSymbol(
		ISymbol symbol,
		IOperation operation,
		INamedTypeSymbol? targetType,
		string[] targetNames,
		XunitContext xunitContext,
		[NotNullWhen(true)]
		out string? foundSymbolName)
	{
		foundSymbolName = default;

		if (targetType is null)
			return false;

		var containingType = symbol.ContainingType;
		if (containingType.IsGenericType && targetType.IsGenericType)
		{
			containingType = containingType.ConstructUnboundGenericType();
			targetType = targetType.ConstructUnboundGenericType();
		}

		if (!targetType.IsAssignableFrom(containingType))
			return false;

		if (!targetNames.Contains(symbol.Name))
			return false;

		// Only trigger when you're inside a test method
		var foundSymbol = operation.IsInTestMethod(xunitContext);
		if (foundSymbol)
			foundSymbolName = symbol.Name;

		return foundSymbol;
	}

	static bool TaskIsKnownToBeCompleted(
		IOperation? operation,
		IEnumerable<ILocalSymbol> symbols,
		INamedTypeSymbol taskType,
		XunitContext xunitContext)
	{
		var ourOperations = new List<IOperation>();
		var unfoundSymbols = new HashSet<ILocalSymbol>(symbols, SymbolEqualityComparer.Default);

		bool validateSafeTasks(IOperation op)
		{
#if ROSLYN_LATEST
			foreach (var childOperation in op.ChildOperations)
#else
			foreach (var childOperation in op.Children)
#endif
			{
				// Stop looking once we've found the operation that is ours, since any
				// code after that operation isn't something we should consider
				if (ourOperations.Contains(childOperation))
					break;

				// Could be marked as safe because of "Task.WhenAll(..., symbol, ...)"
				if (childOperation is IInvocationOperation childInvocationOperation)
					ValidateTasksInWhenAll(childInvocationOperation, unfoundSymbols, taskType, xunitContext);

				// Could be marked as safe because of "var symbol = await WhenAny(...)"
				if (childOperation is IVariableDeclaratorOperation variableDeclaratorOperation)
					ValidateTaskFromWhenAny(variableDeclaratorOperation, unfoundSymbols, taskType, xunitContext);

				// If we've run out of symbols to validate, we're done
				if (unfoundSymbols.Count == 0)
					return true;

#if ROSLYN_LATEST
				if (childOperation.ChildOperations.Any(c => validateSafeTasks(c)))
#else
				if (childOperation.Children.Any(c => validateSafeTasks(c)))
#endif
					return true;
			}

			return false;
		}

		for (; operation is not null && unfoundSymbols.Count != 0; operation = operation.Parent)
		{
			if (operation is IBlockOperation blockOperation)
				if (validateSafeTasks(blockOperation))
					return true;

			ourOperations.Add(operation);
		}

		return false;
	}

	static void ValidateTaskFromWhenAny(
		IVariableDeclaratorOperation operation,
		HashSet<ILocalSymbol> unfoundSymbols,
		INamedTypeSymbol taskType,
		XunitContext xunitContext)
	{
		if (!unfoundSymbols.Contains(operation.Symbol))
			return;

#if ROSLYN_LATEST
		if (operation.ChildOperations.FirstOrDefault() is not IVariableInitializerOperation variableInitializerOperation)
#else
		if (operation.Children.FirstOrDefault() is not IVariableInitializerOperation variableInitializerOperation)
#endif
			return;

#if ROSLYN_LATEST
		if (variableInitializerOperation.Value.ChildOperations.FirstOrDefault() is not IInvocationOperation variableInitializerInvocationOperation)
#else
		if (variableInitializerOperation.Value.Children.FirstOrDefault() is not IInvocationOperation variableInitializerInvocationOperation)
#endif
			return;

		if (!FindSymbol(variableInitializerInvocationOperation.TargetMethod, variableInitializerInvocationOperation, taskType, whenAny, xunitContext, out var _))
			return;

		unfoundSymbols.Remove(operation.Symbol);
	}

	static void ValidateTasksInWhenAll(
		IInvocationOperation operation,
		HashSet<ILocalSymbol> unfoundSymbols,
		INamedTypeSymbol taskType,
		XunitContext xunitContext)
	{
		if (!FindSymbol(operation.TargetMethod, operation, taskType, whenAll, xunitContext, out var _))
			return;

		var argument = operation.Arguments.FirstOrDefault();
		if (argument is null)
			return;
		if (argument.Value is not IArrayCreationOperation arrayCreation)
			return;
		if (arrayCreation.Initializer is null)
			return;

		foreach (var arrayElement in arrayCreation.Initializer.ElementValues.OfType<ILocalReferenceOperation>())
			unfoundSymbols.Remove(arrayElement.Local);
	}
}
