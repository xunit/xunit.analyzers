using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotUseAssertEmptyWithProblematicTypes : AssertUsageAnalyzerBase
{
	static readonly string[] targetMethods =
	[
		Constants.Asserts.Empty,
		Constants.Asserts.NotEmpty,
	];

	public DoNotUseAssertEmptyWithProblematicTypes() :
		base(Descriptors.X2028_DoNotUseAssertEmptyWithProblematicTypes, targetMethods)
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

		var semanticModel = context.Operation.SemanticModel;
		if (semanticModel is null)
			return;

		var arguments = invocationOperation.Arguments;
		if (arguments.Length != 1)
			return;

		if (method.Parameters.Length != 1)
			return;

		if (semanticModel.GetTypeInfo(arguments[0].Value.Syntax).Type is not INamedTypeSymbol sourceType)
			return;

		var stringValuesType = TypeSymbolFactory.StringValues(context.Compilation);
		if (stringValuesType is not null && SymbolEqualityComparer.Default.Equals(sourceType, stringValuesType))
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X2028_DoNotUseAssertEmptyWithProblematicTypes,
					invocationOperation.Syntax.GetLocation(),
					method.Name,
					sourceType.ToMinimalDisplayString(semanticModel, 0),
					"it is implicitly cast to a string, not a collection"
				)
			);

		if (sourceType.IsGenericType)
		{
			var arraySegmentType = TypeSymbolFactory.ArraySegmentOfT(context.Compilation)?.ConstructUnboundGenericType();
			if (arraySegmentType is not null && SymbolEqualityComparer.Default.Equals(sourceType.ConstructUnboundGenericType(), arraySegmentType))
				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X2028_DoNotUseAssertEmptyWithProblematicTypes,
						invocationOperation.Syntax.GetLocation(),
						method.Name,
						sourceType.ToMinimalDisplayString(semanticModel, 0),
						"its implementation of GetEnumerator() can throw"
					)
				);
		}
	}
}
