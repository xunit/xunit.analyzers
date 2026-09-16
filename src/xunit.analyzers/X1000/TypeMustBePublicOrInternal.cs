using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TypeMustBePublicOrInternal() :
	XunitV3AotDiagnosticAnalyzer(Descriptors.X1057_TypeMustBePublicOrInternal)
{
	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var beforeAfterAttributeType = xunitContext.Core.BeforeAfterTestAttributeType;
		var factAndTheoryAttributeTypes = xunitContext.Core.FactAndTheoryAttributeTypes;

		context.RegisterOperationAction(context =>
		{
			if (context.Operation is not IAttributeOperation { Operation: IObjectCreationOperation attributeCreation } attributeOperation)
				return;

			if (attributeCreation.Type is not INamedTypeSymbol attributeType)
				return;

			if (beforeAfterAttributeType.IsAssignableFrom(attributeType))
			{
				verifyTypeAccessibility(attributeType, attributeOperation.Syntax.GetLocation(), "Attribute");
				return;
			}

			if (factAndTheoryAttributeTypes.Contains(attributeType))
			{
				if (attributeCreation.Initializer is null)
					return;

				foreach (var initializerOperation in attributeCreation.Initializer.Initializers)
				{
					if (initializerOperation is not ISimpleAssignmentOperation { Target: IPropertyReferenceOperation propertyReference } assignment)
						continue;

					if (propertyReference.Property.Name != Constants.AttributeProperties.SkipExceptions)
						continue;

					var elements = assignment.Value.WalkDownImplicitConversions() switch
					{
						IArrayCreationOperation { Initializer: { } arrayInitializer } => arrayInitializer.ElementValues,
						ICollectionExpressionOperation collectionExpression => collectionExpression.Elements,
						_ => ImmutableArray<IOperation>.Empty,
					};

					foreach (var element in elements)
						if (element is ITypeOfOperation { TypeOperand: INamedTypeSymbol exceptionType } typeOfOperation)
							verifyTypeAccessibility(exceptionType, typeOfOperation.Syntax.GetLocation(), "Exception");
				}

				return;
			}

			void verifyTypeAccessibility(
				INamedTypeSymbol type,
				Location? location,
				string typeDescription)
			{
				var accessibility = type.DeclaredAccessibility;
				if (accessibility != Accessibility.Internal && accessibility != Accessibility.Public)
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1057_TypeMustBePublicOrInternal,
							location,
							typeDescription,
							type
						)
					);
			}
		}, OperationKind.Attribute);

		var fixtureTypes = new[] {
			xunitContext.Core.IClassFixtureType,
			xunitContext.Core.ICollectionFixtureType,
		}.WhereNotNull().ToImmutableHashSet(SymbolEqualityComparer.Default);

		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not INamedTypeSymbol typeSymbol)
				return;

			foreach (var @interface in typeSymbol.AllInterfaces)
				if (@interface.IsGenericType && fixtureTypes.Contains(@interface.OriginalDefinition))
					if (@interface.TypeArguments.Length == 1 && @interface.TypeArguments[0] is INamedTypeSymbol fixtureType)
						verifyTypeAccessibility(fixtureType, "Fixture");

			void verifyTypeAccessibility(
				INamedTypeSymbol type,
				string typeDescription)
			{
				var accessibility = type.DeclaredAccessibility;
				if (accessibility != Accessibility.Internal && accessibility != Accessibility.Public)
					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1057_TypeMustBePublicOrInternal,
							typeSymbol.Locations.FirstOrDefault(),
							typeDescription,
							type
						)
					);
			}
		}, SymbolKind.NamedType);
	}
}
