using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InlineDataMustMatchTheoryParameters : XunitDiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		ImmutableArray.Create(
			Descriptors.X1009_InlineDataMustMatchTheoryParameters_TooFewValues,
			Descriptors.X1010_InlineDataMustMatchTheoryParameters_IncompatibleValueType,
			Descriptors.X1011_InlineDataMustMatchTheoryParameters_ExtraValue,
			Descriptors.X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter
		);

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		if (xunitContext.Core.TheoryAttributeType is null || xunitContext.Core.InlineDataAttributeType is null)
			return;

		var xunitSupportsParameterArrays = xunitContext.Core.TheorySupportsParameterArrays;
		var xunitSupportsDefaultParameterValues = xunitContext.Core.TheorySupportsDefaultParameterValues;
		var compilation = context.Compilation;
		INamedTypeSymbol? systemRuntimeInteropServicesOptionalAttribute = TypeSymbolFactory.OptionalAttribute(compilation);
		var objectArrayType = compilation.CreateArrayTypeSymbol(compilation.ObjectType);

		context.RegisterSymbolAction(context =>
		{
			if (context.Symbol is not IMethodSymbol method)
				return;

			var attributes = method.GetAttributes();
			if (!attributes.ContainsAttributeType(xunitContext.Core.TheoryAttributeType))
				return;

			foreach (var attribute in attributes)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, xunitContext.Core.InlineDataAttributeType))
					continue;

				// Check if the semantic model indicates there are no syntax/compilation errors
				if (attribute.ConstructorArguments.Length != 1 || !SymbolEqualityComparer.Default.Equals(objectArrayType, attribute.ConstructorArguments.FirstOrDefault().Type))
					continue;

				if (attribute.ApplicationSyntaxReference?.GetSyntax(context.CancellationToken) is not AttributeSyntax attributeSyntax)
					return;

				var arrayStyle = ParameterArrayStyleType.Initializer;
				var dataParameterExpressions = GetParameterExpressionsFromArrayArgument(attributeSyntax);
				if (dataParameterExpressions is null)
				{
					arrayStyle = ParameterArrayStyleType.Params;
					dataParameterExpressions =
						attributeSyntax
							.ArgumentList
							?.Arguments
							.Select(a => a.Expression)
							.ToList()
							?? new List<ExpressionSyntax>();
				}

				var dataArrayArgument = attribute.ConstructorArguments.Single();
				// Need to special case InlineData(null) as the compiler will treat the whole data array as being initialized to null
				var values = dataArrayArgument.IsNull ? ImmutableArray.Create(dataArrayArgument) : dataArrayArgument.Values;
				if (values.Length < method.Parameters.Count(p => RequiresMatchingValue(p, xunitSupportsParameterArrays, xunitSupportsDefaultParameterValues, systemRuntimeInteropServicesOptionalAttribute)))
				{
					var builder = ImmutableDictionary.CreateBuilder<string, string?>();
					builder[Constants.Properties.ParameterArrayStyle] = arrayStyle.ToString();

					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1009_InlineDataMustMatchTheoryParameters_TooFewValues,
							attributeSyntax.GetLocation(),
							builder.ToImmutable()
						)
					);
				}

				int valueIdx = 0, paramIdx = 0;
				for (; valueIdx < values.Length && paramIdx < method.Parameters.Length; valueIdx++)
				{
					var parameter = method.Parameters[paramIdx];

					// If the parameter type is object, everything is compatible
					if (SymbolEqualityComparer.Default.Equals(parameter.Type, compilation.ObjectType))
					{
						paramIdx++;
						continue;
					}

					// If this is a params array (and we're using a version of xUnit.net that supports params arrays),
					// get the element type so we can compare it appropriately.
					var paramsElementType =
						xunitSupportsParameterArrays && parameter.IsParams && parameter.Type is IArrayTypeSymbol arrayParam
							? arrayParam.ElementType
							: null;

					// For params array of object, just consume everything that's left
					if (paramsElementType != null && SymbolEqualityComparer.Default.Equals(paramsElementType, compilation.ObjectType))
					{
						valueIdx = values.Length;
						break;
					}

					var value = values[valueIdx];
					if (value.IsNull)
					{
						var isValueTypeParam =
							paramsElementType != null
								? paramsElementType.IsValueType && paramsElementType.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T
								: parameter.Type.IsValueType && parameter.Type.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T;

						if (isValueTypeParam)
						{
							var builder = ImmutableDictionary.CreateBuilder<string, string?>();
							builder[Constants.Properties.ParameterIndex] = paramIdx.ToString();
							builder[Constants.Properties.ParameterName] = parameter.Name;

							context.ReportDiagnostic(
								Diagnostic.Create(
									Descriptors.X1012_InlineDataMustMatchTheoryParameters_NullShouldNotBeUsedForIncompatibleParameter,
									dataParameterExpressions[valueIdx].GetLocation(),
									builder.ToImmutable(),
									parameter.Name,
									SymbolDisplay.ToDisplayString(paramsElementType ?? parameter.Type)
								)
							);
						}
					}
					else
					{
						if (value.Type is null)
							continue;

						var isCompatible = ConversionChecker.IsConvertible(compilation, value.Type, parameter.Type, xunitContext);
						if (!isCompatible && paramsElementType != null)
							isCompatible = ConversionChecker.IsConvertible(compilation, value.Type, paramsElementType, xunitContext);

						if (!isCompatible)
						{
							var builder = ImmutableDictionary.CreateBuilder<string, string?>();
							builder[Constants.Properties.ParameterIndex] = paramIdx.ToString();
							builder[Constants.Properties.ParameterName] = parameter.Name;

							context.ReportDiagnostic(
								Diagnostic.Create(
									Descriptors.X1010_InlineDataMustMatchTheoryParameters_IncompatibleValueType,
									dataParameterExpressions[valueIdx].GetLocation(),
									builder.ToImmutable(),
									parameter.Name,
									SymbolDisplay.ToDisplayString(paramsElementType ?? parameter.Type)
								)
							);
						}
					}

					if (!parameter.IsParams)
					{
						// Stop moving paramIdx forward if the argument is a parameter array, regardless of xunit's support for it
						paramIdx++;
					}
				}

				for (; valueIdx < values.Length; valueIdx++)
				{
					var builder = ImmutableDictionary.CreateBuilder<string, string?>();
					builder[Constants.Properties.ParameterIndex] = valueIdx.ToString();
					builder[Constants.Properties.ParameterSpecialType] = values[valueIdx].Type?.SpecialType.ToString() ?? string.Empty;

					context.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.X1011_InlineDataMustMatchTheoryParameters_ExtraValue,
							dataParameterExpressions[valueIdx].GetLocation(),
							builder.ToImmutable(),
							values[valueIdx].ToCSharpString()
						)
					);
				}
			}
		}, SymbolKind.Method);
	}

	static bool RequiresMatchingValue(
		IParameterSymbol parameter,
		bool supportsParamsArray,
		bool supportsDefaultValue,
		INamedTypeSymbol? optionalAttribute) =>
			!(parameter.HasExplicitDefaultValue && supportsDefaultValue)
			&& !(parameter.IsParams && supportsParamsArray)
			&& !parameter.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, optionalAttribute));

	static IList<ExpressionSyntax>? GetParameterExpressionsFromArrayArgument(AttributeSyntax attribute)
	{
		if (attribute.ArgumentList?.Arguments.Count != 1)
			return null;

		var argumentExpression = attribute.ArgumentList.Arguments.Single().Expression;

		var initializer = argumentExpression.Kind() switch
		{
			SyntaxKind.ArrayCreationExpression => ((ArrayCreationExpressionSyntax)argumentExpression).Initializer,
			SyntaxKind.ImplicitArrayCreationExpression => ((ImplicitArrayCreationExpressionSyntax)argumentExpression).Initializer,
			_ => null,
		};

		if (initializer is null)
			return null;

		return initializer.Expressions.ToList();
	}

	public enum ParameterArrayStyleType
	{
		/// <summary>
		/// E.g. InlineData(1, 2, 3)
		/// </summary>
		Params,

		/// <summary>
		/// E.g. InlineData(data: new object[] { 1, 2, 3 }) or InlineData(new object[] { 1, 2, 3 })
		/// </summary>
		Initializer,
	}

	static class ConversionChecker
	{
		public static bool IsConvertible(
			Compilation compilation,
			ITypeSymbol source,
			ITypeSymbol destination,
			XunitContext xunitContext)
		{
			if (destination.TypeKind == TypeKind.Array)
			{
				var destinationElementType = ((IArrayTypeSymbol)destination).ElementType;

				if (destinationElementType.TypeKind == TypeKind.TypeParameter)
					return IsConvertibleTypeParameter(source, (ITypeParameterSymbol)destinationElementType);
			}

			if (destination.TypeKind == TypeKind.TypeParameter)
				return IsConvertibleTypeParameter(source, (ITypeParameterSymbol)destination);

			var conversion = compilation.ClassifyConversion(source, destination);

			if (conversion.IsNumeric)
				return IsConvertibleNumeric(source, destination);

			if (destination.SpecialType == SpecialType.System_DateTime
				|| (xunitContext.Core.TheorySupportsConversionFromStringToDateTimeOffsetAndGuid == true && IsDateTimeOffsetOrGuid(destination)))
			{
				// Allow all conversions from strings. All parsing issues will be reported at runtime.
				return source.SpecialType == SpecialType.System_String;
			}

			// Rules of last resort
			return conversion.IsImplicit
				|| conversion.IsUnboxing
				|| (conversion.IsExplicit && conversion.IsUserDefined)
				|| (conversion.IsExplicit && conversion.IsNullable);
		}

		static bool IsConvertibleTypeParameter(
			ITypeSymbol source,
			ITypeParameterSymbol destination)
		{
			if (destination.HasValueTypeConstraint && !source.IsValueType)
				return false;
			if (destination.HasReferenceTypeConstraint && source.IsValueType)
				return false;

			return destination.ConstraintTypes.All(c => c.IsAssignableFrom(source));
		}

		static bool IsConvertibleNumeric(
			ITypeSymbol source,
			ITypeSymbol destination)
		{
			if (destination.SpecialType == SpecialType.System_Char
				&& (source.SpecialType == SpecialType.System_Double || source.SpecialType == SpecialType.System_Single))
			{
				// Conversions from float to char (though numeric) do not actually work at runtime, so report them
				return false;
			}

			return true; // Allow all numeric conversions. Narrowing conversion issues will be reported at runtime.
		}

		static bool IsDateTimeOffsetOrGuid(ITypeSymbol destination)
		{
			if (destination.ContainingNamespace?.Name != nameof(System))
				return false;

			return destination.MetadataName == nameof(DateTimeOffset) || destination.MetadataName == nameof(Guid);
		}
	}
}
