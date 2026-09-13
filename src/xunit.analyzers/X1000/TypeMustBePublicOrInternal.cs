using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

		context.RegisterSyntaxNodeAction(context =>
		{
			if (context.Node is not AttributeSyntax attributeSyntax)
				return;

			if (context.SemanticModel.GetTypeInfo(attributeSyntax, context.CancellationToken).Type is not INamedTypeSymbol attributeType)
				return;

			if (beforeAfterAttributeType.IsAssignableFrom(attributeType))
			{
				verifyTypeAccessibility(attributeType, attributeSyntax.GetLocation(), "Attribute");
				return;
			}

			if (factAndTheoryAttributeTypes.Contains(attributeType))
			{
				var skipExceptions = attributeSyntax.ArgumentList?.Arguments.FirstOrDefault(a => a.NameEquals?.Name.ToString() == Constants.AttributeProperties.SkipExceptions);
				if (skipExceptions is null)
					return;

				if (skipExceptions.Expression is ArrayCreationExpressionSyntax arraySyntax && arraySyntax.Initializer is not null)
					foreach (var typeOfExpression in arraySyntax.Initializer.Expressions.OfType<TypeOfExpressionSyntax>())
						if (context.SemanticModel.GetTypeInfo(typeOfExpression.Type, context.CancellationToken).Type is INamedTypeSymbol exceptionType)
							verifyTypeAccessibility(exceptionType, typeOfExpression.GetLocation(), "Exception");

				if (skipExceptions.Expression is ImplicitArrayCreationExpressionSyntax implicitArraySyntax)
					foreach (var typeOfExpression in implicitArraySyntax.Initializer.Expressions.OfType<TypeOfExpressionSyntax>())
						if (context.SemanticModel.GetTypeInfo(typeOfExpression.Type, context.CancellationToken).Type is INamedTypeSymbol exceptionType)
							verifyTypeAccessibility(exceptionType, typeOfExpression.GetLocation(), "Exception");

				if (skipExceptions.Expression is CollectionExpressionSyntax collectionSyntax)
					foreach (var expressionElement in collectionSyntax.Elements.OfType<ExpressionElementSyntax>())
						if (expressionElement.Expression is TypeOfExpressionSyntax typeOfExpression)
							if (context.SemanticModel.GetTypeInfo(typeOfExpression.Type, context.CancellationToken).Type is INamedTypeSymbol exceptionType)
								verifyTypeAccessibility(exceptionType, typeOfExpression.GetLocation(), "Exception");

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
		}, SyntaxKind.Attribute);

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
