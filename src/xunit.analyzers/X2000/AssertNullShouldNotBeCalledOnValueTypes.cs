using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AssertNullShouldNotBeCalledOnValueTypes : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	[
		Constants.Asserts.NotNull,
		Constants.Asserts.Null,
	];

	public AssertNullShouldNotBeCalledOnValueTypes()
		: base(Descriptors.X2002_AssertNullShouldNotBeCalledOnValueTypes, targetMethods)
	{ }

	protected override void AnalyzeInvocation(
		OperationAnalysisContext context,
		XunitContext xunitContext,
		IInvocationOperation invocationOperation,
		IMethodSymbol method)
	{
		Guard.ArgumentNotNull(xunitContext);
		Guard.ArgumentNotNull(invocationOperation);
		Guard.ArgumentNotNull(method);

		if (invocationOperation.Arguments.Length != 1)
			return;

		var argumentValue = invocationOperation.Arguments[0].Value.WalkDownImplicitConversions();
		var argumentType = argumentValue.Type;
		if (argumentType is null || IsArgumentTypeRecognizedAsReferenceType(argumentType))
			return;

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X2002_AssertNullShouldNotBeCalledOnValueTypes,
				invocationOperation.Syntax.GetLocation(),
				GetDisplayString(method),
				GetDisplayString(argumentType)
			)
		);
	}

	static bool IsArgumentTypeRecognizedAsReferenceType(ITypeSymbol argumentType)
	{
		var isNullableOfT = argumentType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
		var isUnconstrainedGenericType = !argumentType.IsReferenceType && !argumentType.IsValueType;

		return argumentType.IsReferenceType || isNullableOfT || isUnconstrainedGenericType;
	}

	static string GetDisplayString(ISymbol method)
	{
		var displayFormat = SymbolDisplayFormat.CSharpShortErrorMessageFormat.WithParameterOptions(SymbolDisplayParameterOptions.None);

		return SymbolDisplay.ToDisplayString(method, displayFormat);
	}
}
