using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CulturedTestMustHaveAtLeastOneCulture() :
	XunitV3DiagnosticAnalyzer(Descriptors.X1060_CulturedTestMustHaveAtLeastOneCulture)
{
	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var culturedAttributeTypes = xunitContext.V3Core.CulturedTestAttributeTypes;
		if (culturedAttributeTypes.Count == 0)
			return;

		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IAttributeOperation { Operation: IObjectCreationOperation attributeCreation } attributeOperation
					|| attributeCreation.Type is not INamedTypeSymbol attributeType
					|| !culturedAttributeTypes.Contains(attributeType)
					|| attributeCreation.Arguments.IsEmpty)
				return;

			var cultures = attributeCreation.Arguments[0].Value;
			while (cultures is IConversionOperation { IsImplicit: true } conversion)
				cultures = conversion.Operand;

			var isEmpty = cultures switch
			{
				IArrayCreationOperation arrayCreation =>
					arrayCreation.DimensionSizes.Length == 1 && arrayCreation.DimensionSizes[0].ConstantValue is { HasValue: true, Value: 0 },
				ICollectionExpressionOperation collectionExpression =>
					collectionExpression.Elements.IsEmpty,
				_ => false,
			};

			if (isEmpty)
				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1060_CulturedTestMustHaveAtLeastOneCulture,
						attributeOperation.Syntax.GetLocation()
					)
				);
		}, OperationKind.Attribute);
	}
}
