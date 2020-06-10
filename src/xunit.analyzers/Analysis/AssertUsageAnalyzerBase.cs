using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

				context.RegisterSyntaxNodeAction(context =>
				{
					var invocation = (InvocationExpressionSyntax)context.Node;

					var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
					if (symbolInfo.Symbol?.Kind != SymbolKind.Method)
						return;

					var methodSymbol = (IMethodSymbol)symbolInfo.Symbol;
					if (methodSymbol.MethodKind != MethodKind.Ordinary ||
							!Equals(methodSymbol.ContainingType, assertType) ||
							!methodNames.Contains(methodSymbol.Name))
						return;

					Analyze(context, invocation, methodSymbol);

				}, SyntaxKind.InvocationExpression);
			});
		}

		protected abstract void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method);
	}
}
