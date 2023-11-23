using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
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

		var compilation = context.Compilation;

		Dictionary<int, INamedTypeSymbol> theoryDataTypes = new();
		for (int i = 1; i <= 10; i++)
		{
			var symbol = TypeSymbolFactory.TheoryDataN(compilation, i);
			if (symbol is not null)
				theoryDataTypes.Add(i, symbol);
		}

		context.RegisterSyntaxNodeAction(context =>
		{
			if (context.Node is not MethodDeclarationSyntax testMethod)
				return;

			var attributeLists = testMethod.AttributeLists;

			foreach (var attributeSyntax in attributeLists.WhereNotNull().SelectMany(attList => attList.Attributes))
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				// Only work against MemberDataAttribute
				var semanticModel = context.SemanticModel;
				if (!SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeInfo(attributeSyntax, context.CancellationToken).Type, xunitContext.Core.MemberDataAttributeType))
					continue;

				// Need the name of the member to do anything
				if (attributeSyntax.ArgumentList is null)
					continue;

				var memberNameArgument = attributeSyntax.ArgumentList.Arguments.FirstOrDefault();
				if (memberNameArgument is null)
					continue;

				var constantValue = semanticModel.GetConstantValue(memberNameArgument.Expression, context.CancellationToken);
				if (constantValue.Value is not string memberName)
					continue;

				// Figure out which parameters are named property arguments
				var propertyAttributeParameters =
					attributeSyntax
						.ArgumentList
						.Arguments
						.Count(a => !string.IsNullOrEmpty(a.NameEquals?.Name.Identifier.ValueText));

				// Everything else will be potential arguments (for the method-based MemberData)
				var paramsCount = attributeSyntax.ArgumentList.Arguments.Count - 1 - propertyAttributeParameters;

				// Determine what type and member name the MemberData targets
				var (testClassTypeSymbol, declaredMemberTypeSymbol) = GetClassTypesForAttribute(attributeSyntax.ArgumentList, semanticModel, context.CancellationToken);
				if (declaredMemberTypeSymbol is null || testClassTypeSymbol is null)
					continue;

				// Ensure we're pointing to something that exists
				var memberSymbol = FindMemberSymbol(memberName, declaredMemberTypeSymbol, paramsCount);
				if (memberSymbol is null)
				{
					ReportMissingMember(context, attributeSyntax, memberName, declaredMemberTypeSymbol);
					return;
				}

				// Ensure we pointing to a field, method, or property
				var memberReturnType = memberSymbol switch
				{
					IFieldSymbol field => field.Type,
					IMethodSymbol method => method.ReturnType,
					IPropertySymbol prop => prop.Type,
					_ => null,
				};
				if (memberReturnType is null)
				{
					ReportIncorrectMemberType(context, attributeSyntax);
					return;
				}

				// Make sure they use nameof() instead of a string constant for the member name
				if (compilation is CSharpCompilation cSharpCompilation &&
						cSharpCompilation.LanguageVersion >= LanguageVersion.CSharp6 &&
						memberNameArgument.Expression.IsKind(SyntaxKind.StringLiteralExpression))
					ReportUseNameof(context, memberNameArgument, memberName, testClassTypeSymbol, memberSymbol);

				// Every error we report will include at least these two properties
				var memberProperties = new Dictionary<string, string?>
				{
					{ Constants.AttributeProperties.DeclaringType, declaredMemberTypeSymbol.ToDisplayString() },
					{ Constants.AttributeProperties.MemberName, memberName }
				}.ToImmutableDictionary();

				// Make sure the member is public
				if (memberSymbol.DeclaredAccessibility != Accessibility.Public)
					ReportNonPublicAccessibility(context, attributeSyntax, memberProperties);

				// Make sure the member is static
				if (!memberSymbol.IsStatic)
					ReportNonStatic(context, attributeSyntax, memberProperties);

				// Make sure the member returns a compatible type
				VerifyDataSourceReturnType(context, compilation, xunitContext, memberReturnType, memberProperties, attributeSyntax);

				// Make sure public properties have a public getter
				if (memberSymbol.Kind == SymbolKind.Property && memberSymbol.DeclaredAccessibility == Accessibility.Public)
					if (memberSymbol is IPropertySymbol propertySymbol)
					{
						var getMethod = propertySymbol.GetMethod;
						if (getMethod is null || getMethod.DeclaredAccessibility != Accessibility.Public)
							ReportNonPublicPropertyGetter(context, attributeSyntax);
					}

				// If the member returns TheoryData, ensure that the types are compatible
				VerifyTheoryDataUsage(semanticModel, context, testMethod, theoryDataTypes, memberReturnType, memberName, declaredMemberTypeSymbol, attributeSyntax);

				// Get the arguments that are to be passed to the method
				var extraArguments = attributeSyntax.ArgumentList.Arguments.Skip(1).TakeWhile(a => a.NameEquals is null).ToList();

				if (memberSymbol.Kind == SymbolKind.Method)
					VerifyDataMethodParameterUsage(semanticModel, context, compilation, xunitContext, memberSymbol, memberName, extraArguments);
				else
				{
					// Make sure we only have arguments for method-based member data
					if (extraArguments.Any())
						ReportIllegalNonMethodArguments(context, attributeSyntax, extraArguments);
				}
			}
		}, SyntaxKind.MethodDeclaration);
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

	public static (INamedTypeSymbol? TestClass, ITypeSymbol? MemberClass) GetClassTypesForAttribute(
		AttributeArgumentListSyntax attributeList,
		SemanticModel semanticModel,
		CancellationToken cancellationToken)
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

	static (IList<ExpressionSyntax>? Expressions, ExpressionSyntax? ArraySyntax) GetParameterExpressionsFromArrayArgument(
		List<AttributeArgumentSyntax> arguments)
	{
		if (arguments.Count > 1)
			return (arguments.Select(a => a.Expression).ToList(), null);
		if (arguments.Count != 1)
			return (null, null);

		var argumentExpression = arguments.Single().Expression;

		var kind = argumentExpression.Kind();
		var initializer = kind switch
		{
			SyntaxKind.ArrayCreationExpression => ((ArrayCreationExpressionSyntax)argumentExpression).Initializer,
			SyntaxKind.ImplicitArrayCreationExpression => ((ImplicitArrayCreationExpressionSyntax)argumentExpression).Initializer,
			_ => null,
		};
		var arraySyntax = kind switch
		{
			SyntaxKind.ArrayCreationExpression => argumentExpression,
			SyntaxKind.ImplicitArrayCreationExpression => argumentExpression,
			_ => null,
		};

		if (initializer is null)
			return (new List<ExpressionSyntax> { argumentExpression }, null);

		return (initializer.Expressions.ToList(), arraySyntax);
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
		INamedTypeSymbol memberType,
		string memberName,
		IParameterSymbol parameter) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1039_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes,
					location,
					SymbolDisplay.ToDisplayString(theoryDataTypeParameter),
					memberType.Name,
					memberName,
					parameter.Name
				)
			);

	static void ReportMemberMethodTheoryDataNullability(
		SyntaxNodeAnalysisContext context,
		Location location,
		ITypeSymbol theoryDataTypeParameter,
		INamedTypeSymbol memberType,
		string memberName,
		IParameterSymbol parameter) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1040_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability,
					location,
					SymbolDisplay.ToDisplayString(theoryDataTypeParameter),
					memberType.Name,
					memberName,
					parameter.Name
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

	static void VerifyDataMethodParameterUsage(
		SemanticModel semanticModel,
		SyntaxNodeAnalysisContext context,
		Compilation compilation,
		XunitContext xunitContext,
		ISymbol memberSymbol,
		string memberName,
		List<AttributeArgumentSyntax> extraArguments)
	{
		(var argumentSyntaxList, var arraySyntax) = GetParameterExpressionsFromArrayArgument(extraArguments);
		if (argumentSyntaxList is null)
			return;

		var dataMethodSymbol = (IMethodSymbol)memberSymbol;
		var dataMethodParameterSymbols = dataMethodSymbol.Parameters;

		if (arraySyntax is not null
			&& dataMethodParameterSymbols.Length > 0
			&& !dataMethodParameterSymbols[0].IsParams
			&& dataMethodParameterSymbols.Skip(1).All(s => s.IsParams || s.IsOptional))
		{
			// We may have a situation where an array argument to the attribute is intended as an array and not params
			var arrayArgumentTypeSymbol = semanticModel.GetTypeInfo(arraySyntax).Type as IArrayTypeSymbol;
			var arrayArgumentElementTypeSymbol = arrayArgumentTypeSymbol?.ElementType;

			var dataMethodFirstParameterSymbolType = dataMethodParameterSymbols[0].Type;
			var dataMethodFirstParameterElementSymbolType = dataMethodFirstParameterSymbolType.Kind == SymbolKind.ArrayType
				? ((IArrayTypeSymbol)dataMethodFirstParameterSymbolType).ElementType
				: dataMethodFirstParameterSymbolType.GetEnumerableType();

			if (arrayArgumentElementTypeSymbol is not null
				&& dataMethodFirstParameterElementSymbolType is not null
				&& ConversionChecker.IsConvertible(
					compilation, arrayArgumentElementTypeSymbol, dataMethodFirstParameterElementSymbolType, xunitContext))
				argumentSyntaxList = new List<ExpressionSyntax> { arraySyntax };
		}

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
				xunitContext.Core.TheorySupportsParameterArrays && parameter.IsParams && parameter.Type is IArrayTypeSymbol arrayParam
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
				var valueType = semanticModel.GetTypeInfo(argumentSyntaxList[valueIdx], context.CancellationToken).Type;
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
	}

	static void VerifyDataSourceReturnType(
		SyntaxNodeAnalysisContext context,
		Compilation compilation,
		XunitContext xunitContext,
		ITypeSymbol memberType,
		ImmutableDictionary<string, string?> memberProperties,
		AttributeSyntax attributeSyntax)
	{
		var iEnumerableOfObjectArrayType = TypeSymbolFactory.IEnumerableOfObjectArray(compilation);
		var iEnumerableOfTheoryDataRowType = TypeSymbolFactory.IEnumerableOfITheoryDataRow(compilation);
		var valid = iEnumerableOfObjectArrayType.IsAssignableFrom(memberType);

		if (!valid && xunitContext.HasV3References && iEnumerableOfTheoryDataRowType is not null)
			valid = iEnumerableOfTheoryDataRowType.IsAssignableFrom(memberType);

		if (!valid)
			ReportIncorrectReturnType(context, iEnumerableOfObjectArrayType, iEnumerableOfTheoryDataRowType, attributeSyntax, memberProperties, memberType);
	}

	static void VerifyTheoryDataUsage(
		SemanticModel semanticModel,
		SyntaxNodeAnalysisContext context,
		MethodDeclarationSyntax testMethod,
		Dictionary<int, INamedTypeSymbol> theoryDataTypes,
		ITypeSymbol? memberReturnType,
		string memberName,
		ITypeSymbol memberType,
		AttributeSyntax attributeSyntax)
	{
		if (memberType is not INamedTypeSymbol namedMemberType)
			return;

		if (memberReturnType is not INamedTypeSymbol namedReturnType || !namedReturnType.IsGenericType)
			return;

		var returnTypeArguments = namedReturnType.TypeArguments;
		if (!theoryDataTypes.TryGetValue(returnTypeArguments.Length, out var theoryDataType))
			return;
		if (!SymbolEqualityComparer.Default.Equals(theoryDataType, namedReturnType.OriginalDefinition))
			return;

		var testMethodSymbol = semanticModel.GetDeclaredSymbol(testMethod, context.CancellationToken);
		if (testMethodSymbol is null)
			return;

		var testMethodParameterSymbols = testMethodSymbol.Parameters;
		var testMethodParameterSyntaxes = testMethod.ParameterList.Parameters;

		if (testMethodParameterSymbols.Length > returnTypeArguments.Length
			&& testMethodParameterSymbols.Skip(returnTypeArguments.Length).Any(p => !p.IsOptional && !p.IsParams))
		{
			var builder = ImmutableDictionary.CreateBuilder<string, string?>();
			builder[Constants.Properties.MemberName] = memberName;

			ReportMemberMethodTheoryDataTooFewTypeArguments(context, attributeSyntax.GetLocation(), builder);
			return;
		}

		if (testMethodParameterSymbols.Length > 0
			&& testMethodParameterSymbols.Length < returnTypeArguments.Length
			&& !testMethodParameterSymbols.Last().IsParams)
		{
			var builder = ImmutableDictionary.CreateBuilder<string, string?>();
			builder[Constants.Properties.MemberName] = memberName;

			ReportMemberMethodTheoryDataExtraTypeArguments(context, attributeSyntax.GetLocation(), builder);
			return;
		}

		int typeArgumentIdx = 0, parameterTypeIdx = 0;
		for (; typeArgumentIdx < returnTypeArguments.Length && parameterTypeIdx < testMethodParameterSymbols.Length; typeArgumentIdx++)
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

			var typeArgument = returnTypeArguments[typeArgumentIdx];
			if (typeArgument is null)
				continue;

			if (parameterType.Kind != SymbolKind.TypeParameter && !parameterType.IsAssignableFrom(typeArgument))
				ReportMemberMethodTheoryDataIncompatibleType(context, parameterSyntax.Type.GetLocation(), typeArgument, namedMemberType, memberName, parameter);

			// Nullability of value types is handled by the type compatibility test,
			// but nullability of reference types isn't
			if (parameterType.IsReferenceType
					&& typeArgument.IsReferenceType
					&& parameterType.NullableAnnotation == NullableAnnotation.NotAnnotated
					&& typeArgument.NullableAnnotation == NullableAnnotation.Annotated)
				ReportMemberMethodTheoryDataNullability(context, parameterSyntax.Type.GetLocation(), typeArgument, namedMemberType, memberName, parameter);

			if (!parameter.IsParams)
			{
				// Stop moving parameterTypeIdx forward if the argument is a parameter array, regardless of xunit's support for it
				parameterTypeIdx++;
			}
		}
	}
}
