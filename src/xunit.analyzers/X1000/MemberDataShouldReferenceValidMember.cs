using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MemberDataShouldReferenceValidMember : XunitDiagnosticAnalyzer
{
	public MemberDataShouldReferenceValidMember() :
		base(
			Descriptors.X1014_MemberDataShouldUseNameOfOperator,
			Descriptors.X1015_MemberDataMustReferenceExistingMember,
			Descriptors.X1016_MemberDataMustReferencePublicMember,
			Descriptors.X1017_MemberDataMustReferenceStaticMember,
			Descriptors.X1018_MemberDataMustReferenceValidMemberKind,
			Descriptors.X1019_MemberDataMustReferenceMemberOfValidType,
			Descriptors.X1020_MemberDataPropertyMustHaveGetter,
			Descriptors.X1021_MemberDataNonMethodShouldNotHaveParameters,
			Descriptors.X1034_MemberDataArgumentsMustMatchMethodParameters_NullShouldNotBeUsedForIncompatibleParameter,
			Descriptors.X1035_MemberDataArgumentsMustMatchMethodParameters_IncompatibleValueType,
			Descriptors.X1036_MemberDataArgumentsMustMatchMethodParameters_ExtraValue,
			Descriptors.X1037_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_TooFewTypeParameters,
			Descriptors.X1038_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_ExtraTypeParameters,
			Descriptors.X1039_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes,
			Descriptors.X1040_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability
		)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		if (xunitContext.Core.MemberDataAttributeType is null)
			return;

		var xunitSupportsParameterArrays = xunitContext.Core.TheorySupportsParameterArrays;
		var compilation = context.Compilation;

		Dictionary<int, INamedTypeSymbol> theoryDataTypes = new();
		for (int i = 1; i <= 10; i++)
		{
			var symbol = TypeSymbolFactory.TheoryDataN(compilation, i);
			if (symbol is not null)
				theoryDataTypes.Add(i, symbol);
		}

		var supportsNameofOperator =
			compilation is CSharpCompilation cSharpCompilation
			&& cSharpCompilation.LanguageVersion >= LanguageVersion.CSharp6;

		context.RegisterSyntaxNodeAction(context =>
		{
			if (context.Node is not MethodDeclarationSyntax testMethod)
				return;

			var attributeLists = testMethod.AttributeLists;

			foreach (var attributeSyntax in attributeLists.WhereNotNull().SelectMany(attList => attList.Attributes))
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				var memberNameArgument = attributeSyntax.ArgumentList?.Arguments.FirstOrDefault();
				if (memberNameArgument is null)
					continue;

				var semanticModel = context.SemanticModel;
				if (!SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeInfo(attributeSyntax, context.CancellationToken).Type, xunitContext.Core.MemberDataAttributeType))
					continue;

				if (attributeSyntax.ArgumentList is null)
					continue;

				var propertyAttributeParameters =
					attributeSyntax
						.ArgumentList
						.Arguments
						.Count(a => !string.IsNullOrEmpty(a.NameEquals?.Name.Identifier.ValueText));

				var paramsCount = attributeSyntax.ArgumentList.Arguments.Count - 1 - propertyAttributeParameters;

				var constantValue = semanticModel.GetConstantValue(memberNameArgument.Expression, context.CancellationToken);
				if (constantValue.Value is not string memberName)
					continue;

				var memberTypeArgument = attributeSyntax.ArgumentList.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.ValueText == Constants.AttributeProperties.MemberType);
				var memberTypeSymbol = default(ITypeSymbol);
				if (memberTypeArgument?.Expression is TypeOfExpressionSyntax typeofExpression)
				{
					var typeSyntax = typeofExpression.Type;
					memberTypeSymbol = semanticModel.GetTypeInfo(typeSyntax, context.CancellationToken).Type;
				}

				(var testClassTypeSymbol, var declaredMemberTypeSymbol) = GetClassTypesForAttribute(
					attributeSyntax.ArgumentList, semanticModel, context.CancellationToken);
				if (declaredMemberTypeSymbol is null || testClassTypeSymbol is null)
					continue;

				var memberSymbol = FindMemberSymbol(memberName, declaredMemberTypeSymbol, paramsCount);

				if (memberSymbol is null)
					ReportMissingMember(context, attributeSyntax, memberName, declaredMemberTypeSymbol);
				else if (memberSymbol.Kind != SymbolKind.Field && memberSymbol.Kind != SymbolKind.Property && memberSymbol.Kind != SymbolKind.Method)
					ReportIncorrectMemberType(context, attributeSyntax);
				else
				{
					if (supportsNameofOperator && memberNameArgument.Expression.IsKind(SyntaxKind.StringLiteralExpression))
						ReportUseNameof(context, memberNameArgument, memberName, testClassTypeSymbol, memberSymbol);

					var memberProperties = new Dictionary<string, string?>
					{
						{ Constants.AttributeProperties.DeclaringType, declaredMemberTypeSymbol.ToDisplayString() },
						{ Constants.AttributeProperties.MemberName, memberName }
					}.ToImmutableDictionary();

					if (memberSymbol.DeclaredAccessibility != Accessibility.Public)
						ReportNonPublicAccessibility(context, attributeSyntax, memberProperties);

					if (!memberSymbol.IsStatic)
						ReportNonStatic(context, attributeSyntax, memberProperties);

					var memberType = memberSymbol switch
					{
						IPropertySymbol prop => prop.Type,
						IFieldSymbol field => field.Type,
						IMethodSymbol method => method.ReturnType,
						_ => null,
					};

					if (memberType is not null)
					{
						var iEnumerableOfObjectArrayType = TypeSymbolFactory.IEnumerableOfObjectArray(compilation);
						var iEnumerableOfTheoryDataRowType = TypeSymbolFactory.IEnumerableOfITheoryDataRow(compilation);
						var valid = iEnumerableOfObjectArrayType.IsAssignableFrom(memberType);

						if (!valid && xunitContext.HasV3References)
						{
							if (iEnumerableOfTheoryDataRowType is not null)
								valid = iEnumerableOfTheoryDataRowType.IsAssignableFrom(memberType);
						}

						if (!valid)
							ReportIncorrectReturnType(context, iEnumerableOfObjectArrayType, iEnumerableOfTheoryDataRowType, attributeSyntax, memberProperties, memberType);
					}

					if (memberSymbol.Kind == SymbolKind.Property && memberSymbol.DeclaredAccessibility == Accessibility.Public)
						if (memberSymbol is IPropertySymbol propertySymbol)
						{
							var getMethod = propertySymbol.GetMethod;
							if (getMethod is null || getMethod.DeclaredAccessibility != Accessibility.Public)
								ReportNonPublicPropertyGetter(context, attributeSyntax);
						}

					var extraArguments = attributeSyntax.ArgumentList.Arguments.Skip(1).TakeWhile(a => a.NameEquals is null).ToList();
					if (memberSymbol.Kind == SymbolKind.Property || memberSymbol.Kind == SymbolKind.Field)
						if (extraArguments.Any())
							ReportIllegalNonMethodArguments(context, attributeSyntax, extraArguments);

					if (memberSymbol.Kind == SymbolKind.Method)
					{
						// First check: arguments have types that match method parameters
						var argumentSyntaxList = GetParameterExpressionsFromArrayArgument(extraArguments);
						if (argumentSyntaxList is null)
							continue;

						var dataMethodSymbol = (IMethodSymbol)memberSymbol;
						var dataMethodParameterSymbols = dataMethodSymbol.Parameters;

						int valueIdx = 0, paramIdx = 0;
						for (; valueIdx < argumentSyntaxList.Count && paramIdx < dataMethodParameterSymbols.Length; valueIdx++)
						{
							var parameter = dataMethodParameterSymbols[paramIdx];
							var value = semanticModel.GetConstantValue(argumentSyntaxList[valueIdx], context.CancellationToken);

							// If the parameter type is object, everything is compatible, though we still need to check for nullability
							if (SymbolEqualityComparer.Default.Equals(parameter.Type, compilation.ObjectType)
								&& (!value.HasValue || parameter.Type.NullableAnnotation != NullableAnnotation.NotAnnotated))
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
							if (paramsElementType != null
								&& SymbolEqualityComparer.Default.Equals(paramsElementType, compilation.ObjectType)
								&& paramsElementType.NullableAnnotation != NullableAnnotation.NotAnnotated)
							{
								valueIdx = extraArguments.Count;
								break;
							}

							if (!value.HasValue || value.Value is null)
							{
								var isValueTypeParam =
									paramsElementType != null
										? paramsElementType.IsValueType && paramsElementType.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T
										: parameter.Type.IsValueType && parameter.Type.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T;

								var isNonNullableReferenceTypeParam =
									paramsElementType != null
										? paramsElementType.IsReferenceType && paramsElementType.NullableAnnotation == NullableAnnotation.NotAnnotated
										: parameter.Type.IsReferenceType && parameter.Type.NullableAnnotation == NullableAnnotation.NotAnnotated;

								if (isValueTypeParam || isNonNullableReferenceTypeParam)
								{
									var builder = ImmutableDictionary.CreateBuilder<string, string?>();
									builder[Constants.Properties.ParameterIndex] = paramIdx.ToString();
									builder[Constants.Properties.ParameterName] = parameter.Name;
									builder[Constants.Properties.MemberName] = memberName;

									ReportMemberMethodParameterNullability(context, argumentSyntaxList[valueIdx], parameter, paramsElementType, builder);
								}
							}
							else
							{
								var valueType = compilation.GetTypeByMetadataName(value.Value!.GetType().FullName ?? "System.Object");
								if (valueType is null)
									continue;

								var isCompatible = ConversionChecker.IsConvertible(compilation, valueType, parameter.Type, xunitContext);
								if (!isCompatible && paramsElementType != null)
									isCompatible = ConversionChecker.IsConvertible(compilation, valueType, paramsElementType, xunitContext);

								if (!isCompatible)
								{
									var builder = ImmutableDictionary.CreateBuilder<string, string?>();
									builder[Constants.Properties.ParameterIndex] = paramIdx.ToString();
									builder[Constants.Properties.ParameterName] = parameter.Name;
									builder[Constants.Properties.MemberName] = memberName;

									ReportMemberMethodParametersDoNotMatchArgumentTypes(context, argumentSyntaxList[valueIdx], parameter, paramsElementType, builder);
								}
							}

							if (!parameter.IsParams)
							{
								// Stop moving paramIdx forward if the argument is a parameter array, regardless of xunit's support for it
								paramIdx++;
							}
						}

						for (; valueIdx < argumentSyntaxList.Count; valueIdx++)
						{
							var value = semanticModel.GetConstantValue(argumentSyntaxList[valueIdx], context.CancellationToken);
							var valueTypeName = value.Value?.GetType().FullName;
							var valueType = compilation.GetTypeByMetadataName(valueTypeName ?? "System.Object");
							var builder = ImmutableDictionary.CreateBuilder<string, string?>();

							builder[Constants.Properties.ParameterIndex] = valueIdx.ToString();
							builder[Constants.Properties.ParameterSpecialType] = valueType?.SpecialType.ToString() ?? string.Empty;
							builder[Constants.Properties.MemberName] = memberName;

							ReportTooManyArgumentsProvided(context, argumentSyntaxList[valueIdx], value.Value, builder);
						}

						// Second check: method return type, if TheoryData<>, satisfies test method parameters' types and nullability
						if (memberType is not INamedTypeSymbol methodType || !methodType.IsGenericType)
							continue;

						var methodTypeArguments = methodType.TypeArguments;
						if (!SymbolEqualityComparer.Default.Equals(theoryDataTypes[methodTypeArguments.Length], methodType.OriginalDefinition))
							continue;

						var testMethodSymbol = semanticModel.GetDeclaredSymbol(testMethod, context.CancellationToken);
						if (testMethodSymbol is null)
							continue;

						var testMethodParameterSymbols = testMethodSymbol.Parameters;
						var testMethodParameterSyntaxes = testMethod.ParameterList.Parameters;

						if (testMethodParameterSymbols.Length > methodTypeArguments.Length
							&& testMethodParameterSymbols.Skip(methodTypeArguments.Length).Any(p => !p.IsOptional && !p.IsParams))
						{
							var builder = ImmutableDictionary.CreateBuilder<string, string?>();
							builder[Constants.Properties.MemberName] = memberName;

							ReportMemberMethodTheoryDataTooFewTypeArguments(context, attributeSyntax.GetLocation(), builder);
							continue;
						}

						if (testMethodParameterSymbols.Length < methodTypeArguments.Length
							&& !testMethodParameterSymbols.Last().IsParams)
						{
							var builder = ImmutableDictionary.CreateBuilder<string, string?>();
							builder[Constants.Properties.MemberName] = memberName;

							ReportMemberMethodTheoryDataExtraTypeArguments(context, attributeSyntax.GetLocation(), builder);
							continue;
						}

						int typeArgumentIdx = 0, parameterTypeIdx = 0;
						for (; typeArgumentIdx < methodTypeArguments.Length && parameterTypeIdx < testMethodParameterSymbols.Length; typeArgumentIdx++)
						{
							var parameterSyntax = testMethodParameterSyntaxes[parameterTypeIdx];
							if (parameterSyntax.Type is null)
								continue;

							var parameter = testMethodParameterSymbols[parameterTypeIdx];
							if (parameter.Type is null)
								continue;

							var parameterType =
								parameter.IsParams && parameter.Type is IArrayTypeSymbol paramsArraySymbol
									? paramsArraySymbol.ElementType
									: parameter.Type;

							var typeArgument = methodTypeArguments[typeArgumentIdx];
							if (typeArgument is null)
								continue;

							if (!parameterType.IsAssignableFrom(typeArgument))
							{
								var builder = ImmutableDictionary.CreateBuilder<string, string?>();
								builder[Constants.Properties.ParameterIndex] = typeArgumentIdx.ToString();
								builder[Constants.Properties.MemberName] = memberName;

								ReportMemberMethodTheoryDataIncompatibleType(context, parameterSyntax.Type.GetLocation(), typeArgument, parameter, builder);
							}

							// Nullability of value types is handled by the type compatibility test,
							// but nullability of reference types isn't
							if (parameterType.IsReferenceType && typeArgument.IsReferenceType)
							{
								if (parameterType.NullableAnnotation == NullableAnnotation.NotAnnotated
									&& typeArgument.NullableAnnotation == NullableAnnotation.Annotated)
								{
									var builder = ImmutableDictionary.CreateBuilder<string, string?>();
									builder[Constants.Properties.ParameterIndex] = typeArgumentIdx.ToString();
									builder[Constants.Properties.MemberName] = memberName;

									ReportMemberMethodTheoryDataNullability(context, parameterSyntax.Type.GetLocation(), typeArgument, parameter, builder);
								}
							}

							if (!parameter.IsParams)
							{
								// Stop moving parameterTypeIdx forward if the argument is a parameter array, regardless of xunit's support for it
								parameterTypeIdx++;
							}
						}
					}
				}
			}
		}, SyntaxKind.MethodDeclaration);
	}

	static IList<ExpressionSyntax>? GetParameterExpressionsFromArrayArgument(List<AttributeArgumentSyntax> arguments)
	{
		if (arguments.Count > 1)
			return arguments.Select(a => a.Expression).ToList();
		if (arguments.Count != 1)
			return null;

		var argumentExpression = arguments.Single().Expression;

		var initializer = argumentExpression.Kind() switch
		{
			SyntaxKind.ArrayCreationExpression => ((ArrayCreationExpressionSyntax)argumentExpression).Initializer,
			SyntaxKind.ImplicitArrayCreationExpression => ((ImplicitArrayCreationExpressionSyntax)argumentExpression).Initializer,
			_ => null,
		};

		if (initializer is null)
			return new List<ExpressionSyntax> { argumentExpression };

		return initializer.Expressions.ToList();
	}

	static ISymbol? FindMemberSymbol(
		string memberName,
		ITypeSymbol? type,
		int paramsCount)
	{
		if (paramsCount > 0 && FindMethodSymbol(memberName, type, paramsCount) is ISymbol methodSymbol)
			return methodSymbol;

		while (type is not null)
		{
			var memberSymbol = type.GetMembers(memberName).FirstOrDefault();
			if (memberSymbol is not null)
				return memberSymbol;

			type = type.BaseType;
		}

		return null;
	}

	public static ISymbol? FindMethodSymbol(
		string memberName,
		ITypeSymbol? type,
		int paramsCount)
	{
		while (type is not null)
		{
			var methodSymbol =
				type
					.GetMembers(memberName)
					.OfType<IMethodSymbol>()
					.FirstOrDefault(x => x.Parameters.Length == paramsCount);

			if (methodSymbol is not null)
				return methodSymbol;

			type = type.BaseType;
		}

		return null;
	}

	static void ReportIllegalNonMethodArguments(
		SyntaxNodeAnalysisContext context,
		AttributeSyntax attribute,
		List<AttributeArgumentSyntax> extraArguments)
	{
		var span = TextSpan.FromBounds(extraArguments.First().Span.Start, extraArguments.Last().Span.End);

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X1021_MemberDataNonMethodShouldNotHaveParameters,
				Location.Create(attribute.SyntaxTree, span)
			)
		);
	}

	static void ReportIncorrectMemberType(
		SyntaxNodeAnalysisContext context,
		AttributeSyntax attribute) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1018_MemberDataMustReferenceValidMemberKind,
					attribute.GetLocation()
				)
			);

	static void ReportIncorrectReturnType(
		SyntaxNodeAnalysisContext context,
		INamedTypeSymbol iEnumerableOfObjectArrayType,
		INamedTypeSymbol? iEnumerableOfTheoryDataRowType,
		AttributeSyntax attribute,
		ImmutableDictionary<string, string?> memberProperties,
		ITypeSymbol memberType)
	{
		var validSymbols = "'" + SymbolDisplay.ToDisplayString(iEnumerableOfObjectArrayType) + "'";

		if (iEnumerableOfTheoryDataRowType is not null)
			validSymbols += " or '" + SymbolDisplay.ToDisplayString(iEnumerableOfTheoryDataRowType) + "'";

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X1019_MemberDataMustReferenceMemberOfValidType,
				attribute.GetLocation(),
				memberProperties,
				validSymbols,
				SymbolDisplay.ToDisplayString(memberType)
			)
		);
	}

	static void ReportMissingMember(
		SyntaxNodeAnalysisContext context,
		AttributeSyntax attribute,
		string memberName,
		ITypeSymbol declaredMemberTypeSymbol) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1015_MemberDataMustReferenceExistingMember,
					attribute.GetLocation(),
					memberName,
					SymbolDisplay.ToDisplayString(declaredMemberTypeSymbol)
				)
			);

	static void ReportNonPublicAccessibility(
		SyntaxNodeAnalysisContext context,
		AttributeSyntax attribute,
		ImmutableDictionary<string, string?> memberProperties) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1016_MemberDataMustReferencePublicMember,
					attribute.GetLocation(),
					memberProperties
				)
			);

	static void ReportNonPublicPropertyGetter(
		SyntaxNodeAnalysisContext context,
		AttributeSyntax attribute) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1020_MemberDataPropertyMustHaveGetter,
					attribute.GetLocation()
				)
			);

	static void ReportNonStatic(
		SyntaxNodeAnalysisContext context,
		AttributeSyntax attribute,
		ImmutableDictionary<string, string?> memberProperties) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1017_MemberDataMustReferenceStaticMember,
					attribute.GetLocation(),
					memberProperties
				)
			);

	static void ReportUseNameof(
		SyntaxNodeAnalysisContext context,
		AttributeArgumentSyntax memberNameArgument,
		string memberName,
		INamedTypeSymbol testClassTypeSymbol,
		ISymbol memberSymbol)
	{
		var builder = ImmutableDictionary.CreateBuilder<string, string?>();
		if (!SymbolEqualityComparer.Default.Equals(memberSymbol.ContainingType, testClassTypeSymbol))
			builder.Add("DeclaringType", memberSymbol.ContainingType.ToDisplayString());

		context.ReportDiagnostic(
			Diagnostic.Create(
				Descriptors.X1014_MemberDataShouldUseNameOfOperator,
				memberNameArgument.Expression.GetLocation(),
				builder.ToImmutable(),
				memberName,
				memberSymbol.ContainingType.ToDisplayString()
			)
		);
	}

	static void ReportMemberMethodParametersDoNotMatchArgumentTypes(
		SyntaxNodeAnalysisContext context,
		ExpressionSyntax syntax,
		IParameterSymbol parameter,
		ITypeSymbol? paramsElementType,
		ImmutableDictionary<string, string?>.Builder builder) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1035_MemberDataArgumentsMustMatchMethodParameters_IncompatibleValueType,
					syntax.GetLocation(),
					builder.ToImmutable(),
					parameter.Name,
					SymbolDisplay.ToDisplayString(paramsElementType ?? parameter.Type)
				)
			);

	static void ReportTooManyArgumentsProvided(
		SyntaxNodeAnalysisContext context,
		ExpressionSyntax syntax,
		object? value,
		ImmutableDictionary<string, string?>.Builder builder) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1036_MemberDataArgumentsMustMatchMethodParameters_ExtraValue,
					syntax.GetLocation(),
					builder.ToImmutable(),
					value?.ToString() ?? "null"
				)
			);

	static void ReportMemberMethodParameterNullability(
		SyntaxNodeAnalysisContext context,
		ExpressionSyntax syntax,
		IParameterSymbol parameter,
		ITypeSymbol? paramsElementType,
		ImmutableDictionary<string, string?>.Builder builder) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1034_MemberDataArgumentsMustMatchMethodParameters_NullShouldNotBeUsedForIncompatibleParameter,
					syntax.GetLocation(),
					builder.ToImmutable(),
					parameter.Name,
					SymbolDisplay.ToDisplayString(paramsElementType ?? parameter.Type)
				)
			);

	static void ReportMemberMethodTheoryDataTooFewTypeArguments(
		SyntaxNodeAnalysisContext context,
		Location location,
		ImmutableDictionary<string, string?>.Builder builder) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1037_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_TooFewTypeParameters,
					location,
					builder.ToImmutable()
				)
			);

	static void ReportMemberMethodTheoryDataExtraTypeArguments(
		SyntaxNodeAnalysisContext context,
		Location location,
		ImmutableDictionary<string, string?>.Builder builder) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1038_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_ExtraTypeParameters,
					location,
					builder.ToImmutable()
				)
			);

	static void ReportMemberMethodTheoryDataIncompatibleType(
		SyntaxNodeAnalysisContext context,
		Location location,
		ITypeSymbol theoryDataTypeParameter,
		IParameterSymbol parameter,
		ImmutableDictionary<string, string?>.Builder builder) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1039_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes,
					location,
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(theoryDataTypeParameter),
					parameter.Name
				)
			);

	static void ReportMemberMethodTheoryDataNullability(
		SyntaxNodeAnalysisContext context,
		Location location,
		ITypeSymbol theoryDataTypeParameter,
		IParameterSymbol parameter,
		ImmutableDictionary<string, string?>.Builder builder) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1040_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability,
					location,
					builder.ToImmutable(),
					SymbolDisplay.ToDisplayString(theoryDataTypeParameter),
					parameter.Name
				)
			);

	public static (INamedTypeSymbol? TestClass, ITypeSymbol? MemberClass) GetClassTypesForAttribute(
		AttributeArgumentListSyntax attributeList, SemanticModel semanticModel, CancellationToken cancellationToken)
	{
		var memberTypeArgument = attributeList.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.ValueText == Constants.AttributeProperties.MemberType);
		var memberTypeSymbol = default(ITypeSymbol);
		if (memberTypeArgument?.Expression is TypeOfExpressionSyntax typeofExpression)
		{
			var typeSyntax = typeofExpression.Type;
			memberTypeSymbol = semanticModel.GetTypeInfo(typeSyntax, cancellationToken).Type;
		}

		var classSyntax = attributeList.FirstAncestorOrSelf<ClassDeclarationSyntax>();
		if (classSyntax is null)
			return (null, null);

		var testClassTypeSymbol = semanticModel.GetDeclaredSymbol(classSyntax);
		if (testClassTypeSymbol is null)
			return (null, null);

		var declaredMemberTypeSymbol = memberTypeSymbol ?? testClassTypeSymbol;
		if (declaredMemberTypeSymbol is null)
			return (testClassTypeSymbol, null);

		return (testClassTypeSymbol, declaredMemberTypeSymbol);
	}
}
