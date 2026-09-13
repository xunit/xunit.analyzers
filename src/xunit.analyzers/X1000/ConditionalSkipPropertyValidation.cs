using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConditionalSkipPropertyValidation() :
	XunitV3DiagnosticAnalyzer(
		Descriptors.X1054_ConditionalSkipPropertiesMustBePublicStaticBoolean,
		Descriptors.X1055_CannotSetBothSkipUnlessAndSkipWhen)
{
	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var dataAttributes = xunitContext.V3Core.DataAttributeTypes_V3;
		var factAndTheoryAttributeTypes = xunitContext.Core.FactAndTheoryAttributeTypes;
		var booleanType = TypeSymbolFactory.Boolean(context.Compilation);

		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IAttributeOperation { Operation: IObjectCreationOperation { Initializer: { } initializer } attributeCreation })
				return;

			if (attributeCreation.Type is not INamedTypeSymbol attributeType)
				return;

			if (dataAttributes.Contains(attributeType.IsGenericType ? attributeType.OriginalDefinition : attributeType) || factAndTheoryAttributeTypes.Contains(attributeType))
			{
				var skipType = context.ContainingSymbol?.ContainingType;
				var skipUnless = default(string);
				var skipUnlessLocation = default(Location);
				var skipWhen = default(string);
				var skipWhenLocation = default(Location);

				foreach (var initializerOperation in initializer.Initializers)
				{
					if (initializerOperation is not ISimpleAssignmentOperation { Target: IPropertyReferenceOperation propertyReference } assignment)
						continue;

					switch (propertyReference.Property.Name)
					{
						case Constants.AttributeProperties.SkipType:
							skipType = (assignment.Value as ITypeOfOperation)?.TypeOperand as INamedTypeSymbol;
							break;

						case Constants.AttributeProperties.SkipUnless:
							skipUnless = toName(assignment.Value);
							skipUnlessLocation = assignment.Syntax.GetLocation();
							break;

						case Constants.AttributeProperties.SkipWhen:
							skipWhen = toName(assignment.Value);
							skipWhenLocation = assignment.Syntax.GetLocation();
							break;
					}
				}

				if (skipType is null)
					return;

				verifySkipProperty(skipType, skipUnless, skipUnlessLocation);
				verifySkipProperty(skipType, skipWhen, skipWhenLocation);

				if (skipUnless is not null && skipWhen is not null)
				{
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1055_CannotSetBothSkipUnlessAndSkipWhen,
							skipUnlessLocation
						)
					);
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1055_CannotSetBothSkipUnlessAndSkipWhen,
							skipWhenLocation
						)
					);
				}
			}

			static string? toName(IOperation operation) =>
				operation.ConstantValue is { HasValue: true, Value: string name } ? name : null;

			void verifySkipProperty(
				INamedTypeSymbol skipType,
				string? propertyName,
				Location? location)
			{
				if (propertyName is null)
					return;

				var currentSymbol = skipType;

				while (currentSymbol is not null)
				{
					var property =
						currentSymbol
							.GetMembers(propertyName)
							.OfType<IPropertySymbol>()
							.FirstOrDefault();

					if (property is not null)
					{
						if (property.DeclaredAccessibility == Accessibility.Public
								&& property.IsStatic
								&& SymbolEqualityComparer.Default.Equals(property.Type, booleanType))
							return;

						break;
					}

					currentSymbol = currentSymbol.BaseType;
				}

				context.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.X1054_ConditionalSkipPropertiesMustBePublicStaticBoolean,
						location,
						skipType,
						propertyName
					)
				);
			}
		}, OperationKind.Attribute);
	}
}
