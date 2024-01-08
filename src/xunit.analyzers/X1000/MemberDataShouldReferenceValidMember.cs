using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
			Descriptors.X1040_MemberDataTheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability,
			Descriptors.X1042_MemberDataTheoryDataIsRecommendedForStronglyTypedAnalysis
		)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

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
				var iEnumerableOfTheoryDataRowType = TypeSymbolFactory.IEnumerableOfITheoryDataRow(compilation);
				var IsValidMemberReturnType =
					VerifyDataSourceReturnType(context, compilation, xunitContext, memberReturnType, memberProperties, attributeSyntax, iEnumerableOfTheoryDataRowType);

				// Make sure public properties have a public getter
				if (memberSymbol.Kind == SymbolKind.Property && memberSymbol.DeclaredAccessibility == Accessibility.Public)
					if (memberSymbol is IPropertySymbol propertySymbol)
					{
						var getMethod = propertySymbol.GetMethod;
						if (getMethod is null || getMethod.DeclaredAccessibility != Accessibility.Public)
							ReportNonPublicPropertyGetter(context, attributeSyntax);
					}

				// If the member returns TheoryData, ensure that the types are compatible
				// If the member does not return TheoryData, gently suggest to the user that TheoryData is better for type safety
				if (IsTheoryDataType(memberReturnType, theoryDataTypes, out var theoryReturnType))
					VerifyTheoryDataUsage(semanticModel, context, testMethod, theoryReturnType, memberName, declaredMemberTypeSymbol, attributeSyntax);
				else if (IsValidMemberReturnType && !IsTheoryDataRowType(memberReturnType, iEnumerableOfTheoryDataRowType))
					ReportMemberReturnsTypeUnsafeValue(context, attributeSyntax);

				// Get the arguments that are to be passed to the method
				var extraArguments = attributeSyntax.ArgumentList.Arguments.Skip(1).TakeWhile(a => a.NameEquals is null).ToList();

				if (memberSymbol.Kind == SymbolKind.Method)
					VerifyDataMethodParameterUsage(semanticModel, context, compilation, xunitContext, memberSymbol, memberName, extraArguments);
				else
				{
					// Make sure we only have arguments for method-based member data
					if (extraArguments.Count != 0)
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

	public static ISymbol? FindMethodSymbol(
		string memberName,
		ITypeSymbol? type,
		int paramsCount)
	{
		Guard.ArgumentNotNull(memberName);

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

	public static (INamedTypeSymbol? TestClass, ITypeSymbol? MemberClass) GetClassTypesForAttribute(
		AttributeArgumentListSyntax attributeList,
		SemanticModel semanticModel,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(attributeList);
		Guard.ArgumentNotNull(semanticModel);

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

		var testClassTypeSymbol = semanticModel.GetDeclaredSymbol(classSyntax, cancellationToken);
		return (testClassTypeSymbol, memberTypeSymbol ?? testClassTypeSymbol);
	}

	static IList<ExpressionSyntax>? GetParameterExpressionsFromArrayArgument(
		List<AttributeArgumentSyntax> arguments, SemanticModel semanticModel)
	{
		if (arguments.Count > 1)
			return arguments.Select(a => a.Expression).ToList();
		if (arguments.Count != 1)
			return null;

		var argumentExpression = arguments.Single().Expression;

		var kind = argumentExpression.Kind();
		var initializer = kind switch
		{
			SyntaxKind.ArrayCreationExpression => ((ArrayCreationExpressionSyntax)argumentExpression).Initializer,
			SyntaxKind.ImplicitArrayCreationExpression => ((ImplicitArrayCreationExpressionSyntax)argumentExpression).Initializer,
			_ => null,
		};

		if (initializer is null)
			return new List<ExpressionSyntax> { argumentExpression };

		// In the special case where the argument is an object[], treat like params
		var type = semanticModel.GetTypeInfo(argumentExpression).Type;
		if (type is IArrayTypeSymbol arrayType && arrayType.ElementType.SpecialType == SpecialType.System_Object)
			return initializer.Expressions.ToList();

		return new List<ExpressionSyntax> { argumentExpression };
	}

	static bool IsTheoryDataRowType(
		ITypeSymbol? memberReturnType,
		INamedTypeSymbol? iEnumerableOfTheoryDataRowType) =>
			iEnumerableOfTheoryDataRowType?.IsAssignableFrom(memberReturnType) ?? false;

	static bool IsTheoryDataType(
		ITypeSymbol? memberReturnType,
		Dictionary<int, INamedTypeSymbol> theoryDataTypes,
		[NotNullWhen(true)] out INamedTypeSymbol? theoryReturnType)
	{
		theoryReturnType = default;
		if (memberReturnType is not INamedTypeSymbol namedReturnType)
			return false;

		INamedTypeSymbol? working = namedReturnType;
		while (working is not null)
		{
			var returnTypeArguments = working.TypeArguments;
			if (theoryDataTypes.TryGetValue(returnTypeArguments.Length, out var theoryDataType)
				&& SymbolEqualityComparer.Default.Equals(theoryDataType, working.OriginalDefinition))
				break;
			working = working.BaseType;
		}

		if (working is null)
			return false;

		theoryReturnType = working;
		return true;
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

	static void ReportMemberReturnsTypeUnsafeValue(
		SyntaxNodeAnalysisContext context,
		AttributeSyntax attribute) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1042_MemberDataTheoryDataIsRecommendedForStronglyTypedAnalysis,
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
		var argumentSyntaxList = GetParameterExpressionsFromArrayArgument(extraArguments, semanticModel);
		if (argumentSyntaxList is null)
			return;

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
				xunitContext.Core.TheorySupportsParameterArrays && parameter.IsParams && parameter.Type is IArrayTypeSymbol arrayParam
					? arrayParam.ElementType
					: null;

			// For params array of object, just consume everything that's left
			if (paramsElementType is not null
				&& SymbolEqualityComparer.Default.Equals(paramsElementType, compilation.ObjectType)
				&& paramsElementType.NullableAnnotation != NullableAnnotation.NotAnnotated)
			{
				valueIdx = extraArguments.Count;
				break;
			}

			if (value.HasValue && value.Value is null)
			{
				var isValueTypeParam =
					paramsElementType is not null
						? paramsElementType.IsValueType && paramsElementType.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T
						: parameter.Type.IsValueType && parameter.Type.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T;

				var isNonNullableReferenceTypeParam =
					paramsElementType is not null
						? paramsElementType.IsReferenceType && paramsElementType.NullableAnnotation == NullableAnnotation.NotAnnotated
						: parameter.Type.IsReferenceType && parameter.Type.NullableAnnotation == NullableAnnotation.NotAnnotated;

				if (isValueTypeParam || isNonNullableReferenceTypeParam)
				{
					var builder = ImmutableDictionary.CreateBuilder<string, string?>();
					builder[Constants.Properties.ParameterIndex] = paramIdx.ToString(CultureInfo.InvariantCulture);
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
				if (!isCompatible && paramsElementType is not null)
					isCompatible = ConversionChecker.IsConvertible(compilation, valueType, paramsElementType, xunitContext);

				if (!isCompatible)
				{
					var builder = ImmutableDictionary.CreateBuilder<string, string?>();
					builder[Constants.Properties.ParameterIndex] = paramIdx.ToString(CultureInfo.InvariantCulture);
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

			builder[Constants.Properties.ParameterIndex] = valueIdx.ToString(CultureInfo.InvariantCulture);
			builder[Constants.Properties.ParameterSpecialType] = valueType?.SpecialType.ToString() ?? string.Empty;
			builder[Constants.Properties.MemberName] = memberName;

			ReportTooManyArgumentsProvided(context, argumentSyntaxList[valueIdx], value.Value, builder);
		}
	}

	static bool VerifyDataSourceReturnType(
		SyntaxNodeAnalysisContext context,
		Compilation compilation,
		XunitContext xunitContext,
		ITypeSymbol memberType,
		ImmutableDictionary<string, string?> memberProperties,
		AttributeSyntax attributeSyntax,
		INamedTypeSymbol? iEnumerableOfTheoryDataRowType)
	{
		var iEnumerableOfObjectArrayType = TypeSymbolFactory.IEnumerableOfObjectArray(compilation);
		var valid = iEnumerableOfObjectArrayType.IsAssignableFrom(memberType);

		if (!valid && xunitContext.HasV3References && iEnumerableOfTheoryDataRowType is not null)
			valid = iEnumerableOfTheoryDataRowType.IsAssignableFrom(memberType);

		if (!valid)
			ReportIncorrectReturnType(context, iEnumerableOfObjectArrayType, iEnumerableOfTheoryDataRowType, attributeSyntax, memberProperties, memberType);

		return valid;
	}

	static void VerifyTheoryDataUsage(
		SemanticModel semanticModel,
		SyntaxNodeAnalysisContext context,
		MethodDeclarationSyntax testMethod,
		INamedTypeSymbol theoryReturnType,
		string memberName,
		ITypeSymbol memberType,
		AttributeSyntax attributeSyntax)
	{
		if (memberType is not INamedTypeSymbol namedMemberType)
			return;

		var returnTypeArguments = theoryReturnType.TypeArguments;
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
			{
				bool report = true;

				// The user might be providing the full array for 'params'; if they do, we need to move
				// the parameter type index forward because it's been consumed by the array
				if (parameter.IsParams && parameter.Type.IsAssignableFrom(typeArgument))
				{
					report = false;
					parameterTypeIdx++;
				}

				if (report)
					ReportMemberMethodTheoryDataIncompatibleType(context, parameterSyntax.Type.GetLocation(), typeArgument, namedMemberType, memberName, parameter);
			}

			// Nullability of value types is handled by the type compatibility test,
			// but nullability of reference types isn't
			if (parameterType.IsReferenceType
					&& typeArgument.IsReferenceType
					&& parameterType.NullableAnnotation == NullableAnnotation.NotAnnotated
					&& typeArgument.NullableAnnotation == NullableAnnotation.Annotated)
				ReportMemberMethodTheoryDataNullability(context, parameterSyntax.Type.GetLocation(), typeArgument, namedMemberType, memberName, parameter);

			// Only move the parameter type index forward when the current parameter is not a 'params'
			if (!parameter.IsParams)
				parameterTypeIdx++;
		}

		if (typeArgumentIdx < returnTypeArguments.Length)
		{
			var builder = ImmutableDictionary.CreateBuilder<string, string?>();
			builder[Constants.Properties.MemberName] = memberName;

			ReportMemberMethodTheoryDataExtraTypeArguments(context, attributeSyntax.GetLocation(), builder);
		}
	}
}
