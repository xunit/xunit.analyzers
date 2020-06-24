using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

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

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, InvocationExpressionSyntax invocation, IMethodSymbol method)
		{
			if (invocationOperation.Arguments.Length != 2)
				return;

			var firstArgumentType = invocationOperation.Arguments[0].Value.WalkDownImplicitConversions()?.Type;
			var secondArgumentType = invocationOperation.Arguments[1].Value.WalkDownImplicitConversions()?.Type;
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
					invocationOperation.Syntax.GetLocation(),
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
