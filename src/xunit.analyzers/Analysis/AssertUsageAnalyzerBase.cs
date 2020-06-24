using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	public abstract class AssertUsageAnalyzerBase : DiagnosticAnalyzer
	{
		readonly HashSet<string> methodNames;

		protected AssertUsageAnalyzerBase(DiagnosticDescriptor descriptor, IEnumerable<string> methods)
			: this(new[] { descriptor }, methods)
		{ }

		protected AssertUsageAnalyzerBase(IEnumerable<DiagnosticDescriptor> descriptors, IEnumerable<string> methods)
		{
			SupportedDiagnostics = ImmutableArray.CreateRange(descriptors);
			methodNames = new HashSet<string>(methods, StringComparer.Ordinal);
		}

		public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

		public sealed override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(context =>
			{
				var assertType = context.Compilation.GetTypeByMetadataName(Constants.Types.XunitAssert);
				if (assertType == null)
					return;

				context.RegisterOperationAction(context =>
				{
					var invocationOperation = (IInvocationOperation)context.Operation;
					if (!(invocationOperation.Syntax is InvocationExpressionSyntax invocation))
						return;

					var methodSymbol = invocationOperation.TargetMethod;
					if (methodSymbol.MethodKind != MethodKind.Ordinary ||
							!Equals(methodSymbol.ContainingType, assertType) ||
							!methodNames.Contains(methodSymbol.Name))
						return;

					Analyze(context, invocationOperation, invocation, methodSymbol);
				}, OperationKind.Invocation);
			});
		}

		protected abstract void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, InvocationExpressionSyntax invocation, IMethodSymbol method);
	}
}
