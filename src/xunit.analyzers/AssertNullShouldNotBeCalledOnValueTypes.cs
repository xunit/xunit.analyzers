using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AssertNullShouldNotBeCalledOnValueTypes : AssertUsageAnalyzerBase
	{
		private const string NullMethod = "Null";
		private const string NotNullMethod = "NotNull";

		public AssertNullShouldNotBeCalledOnValueTypes()
			: base(Descriptors.X2002_AssertNullShouldNotBeCalledOnValueTypes, new[] { NullMethod, NotNullMethod })
		{ }

		protected override void Analyze(OperationAnalysisContext context, IInvocationOperation invocationOperation, InvocationExpressionSyntax invocation, IMethodSymbol method)
		{
			if (invocationOperation.Arguments.Length != 1)
				return;

			var argumentValue = invocationOperation.Arguments[0].Value.WalkDownImplicitConversions();
			var argumentType = argumentValue.Type;
			if (argumentType == null || IsArgumentTypeRecognizedAsReferenceType(argumentType))
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2002_AssertNullShouldNotBeCalledOnValueTypes,
					invocationOperation.Syntax.GetLocation(),
					GetDisplayString(method),
					GetDisplayString(argumentType)));
		}

		private static bool IsArgumentTypeRecognizedAsReferenceType(ITypeSymbol argumentType)
		{
			var isNullableOfT = argumentType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
			var isUnconstrainedGenericType = !argumentType.IsReferenceType && !argumentType.IsValueType;

			return argumentType.IsReferenceType || isNullableOfT || isUnconstrainedGenericType;
		}

		private static string GetDisplayString(ISymbol method)
		{
			var displayFormat = SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None);

			return SymbolDisplay.ToDisplayString(method, displayFormat);
		}
	}
}
