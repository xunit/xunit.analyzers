using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CulturedTestCultureValidation() :
	XunitV3DiagnosticAnalyzer(
		Descriptors.X1060_CulturedTestMustHaveAtLeastOneCulture,
		Descriptors.X1070_CulturedTestCultureCannotBeNull,
		Descriptors.X1071_CulturedTestCultureShouldNotBeDuplicated)
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
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1060_CulturedTestMustHaveAtLeastOneCulture,
						attributeOperation.Syntax.GetLocation()
					)
				);
				return;
			}

			var elements = cultures switch
			{
				IArrayCreationOperation { Initializer: not null } arrayCreation => arrayCreation.Initializer.ElementValues,
				ICollectionExpressionOperation collectionExpression => collectionExpression.Elements,
				_ => [],
			};

			var seenCultures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (var element in elements)
			{
				var value = element;
				while (value is IConversionOperation { IsImplicit: true } conversion)
					value = conversion.Operand;

				if (!value.ConstantValue.HasValue)
					continue;

				if (value.ConstantValue.Value is not string culture)
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1070_CulturedTestCultureCannotBeNull,
							element.Syntax.GetLocation()
						)
					);
				else if (!seenCultures.Add(culture))
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1071_CulturedTestCultureShouldNotBeDuplicated,
							element.Syntax.GetLocation(),
							culture
						)
					);
			}
		}, OperationKind.Attribute);
	}
}
