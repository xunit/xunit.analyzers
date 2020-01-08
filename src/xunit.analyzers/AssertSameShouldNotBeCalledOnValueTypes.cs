using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertSameShouldNotBeCalledOnValueTypes : AssertUsageAnalyzerBase
	{
		internal static string MethodName = "MethodName";
		internal const string SameMethod = "Same";
		internal const string NotSameMethod = "NotSame";

		public AssertSameShouldNotBeCalledOnValueTypes()
			: base(Descriptors.X2005_AssertSameShouldNotBeCalledOnValueTypes, new[] { SameMethod, NotSameMethod })
		{ }

		protected override void Analyze(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
		{
			if (invocation.ArgumentList.Arguments.Count != 2)
				return;

			var firstArgumentType = context.SemanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression, context.CancellationToken).Type;
			var secondArgumentType = context.SemanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[1].Expression, context.CancellationToken).Type;
			if (firstArgumentType == null || secondArgumentType == null)
				return;

			if (firstArgumentType.IsReferenceType && secondArgumentType.IsReferenceType)
				return;

			var typeToDisplay = firstArgumentType.IsReferenceType ? secondArgumentType : firstArgumentType;

			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			builder[MethodName] = method.Name;
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2005_AssertSameShouldNotBeCalledOnValueTypes,
					invocation.GetLocation(),
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(
						method,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None)),
					SymbolDisplay.ToDisplayString(
						typeToDisplay,
						SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None))));
		}
	}
}
