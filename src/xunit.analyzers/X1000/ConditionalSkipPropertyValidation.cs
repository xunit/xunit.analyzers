using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

		context.RegisterSyntaxNodeAction(context =>
		{
			if (context.Node is not AttributeSyntax attributeSyntax || attributeSyntax.ArgumentList is null)
				return;

			if (context.SemanticModel.GetTypeInfo(attributeSyntax, context.CancellationToken).Type is not INamedTypeSymbol attributeType)
				return;

			if (dataAttributes.Contains(attributeType.IsGenericType ? attributeType.OriginalDefinition : attributeType) || factAndTheoryAttributeTypes.Contains(attributeType))
			{
				var skipType = context.ContainingSymbol?.ContainingType;
				var skipUnless = default(string);
				var skipUnlessLocation = default(Location);
				var skipWhen = default(string);
				var skipWhenLocation = default(Location);

				foreach (var argument in attributeSyntax.ArgumentList.Arguments)
					switch (argument.NameEquals?.Name.ToString())
					{
						case Constants.AttributeProperties.SkipType:
							skipType = toType(argument.Expression);
							break;

						case Constants.AttributeProperties.SkipUnless:
							skipUnless = toName(argument.Expression);
							skipUnlessLocation = argument.GetLocation();
							break;

						case Constants.AttributeProperties.SkipWhen:
							skipWhen = toName(argument.Expression);
							skipWhenLocation = argument.GetLocation();
							break;
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

			static string? toName(ExpressionSyntax expression)
			{
				if (expression is LiteralExpressionSyntax literal)
					return literal.Token.Value as string;
				// TODO: Is there a more canonically correct way to get just the name besides a .Split?
				if (expression is InvocationExpressionSyntax invocation && invocation.Expression is IdentifierNameSyntax)
					return invocation.ArgumentList?.Arguments.FirstOrDefault()?.ToString().Split('.').LastOrDefault();

				return null;
			}

			INamedTypeSymbol? toType(ExpressionSyntax expression)
			{
				if (expression is not TypeOfExpressionSyntax typeOf)
					return null;

				return context.SemanticModel.GetTypeInfo(typeOf.Type).Type as INamedTypeSymbol;
			}

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
							.GetMembers()
							.OfType<IPropertySymbol>()
							.FirstOrDefault(symbol => symbol.Name == propertyName);

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
		}, SyntaxKind.Attribute);
	}
}
