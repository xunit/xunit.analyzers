using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ClassDataAttributeMustPointAtValidClass : XunitDiagnosticAnalyzer
{
	const string typesV2 = "IEnumerable<object[]>";
	const string typesV3 = "IEnumerable<object[]>, IAsyncEnumerable<object[]>, IEnumerable<ITheoryDataRow>, or IAsyncEnumerable<ITheoryDataRow>";

	public ClassDataAttributeMustPointAtValidClass() :
		base(
			Descriptors.X1007_ClassDataAttributeMustPointAtValidClass,
			Descriptors.X1037_TheoryArgumentsMustMatchTestMethodParameters_TooFewTypeParameters,
			Descriptors.X1038_TheoryArgumentsMustMatchTestMethodParameters_ExtraTypeParameters,
			Descriptors.X1039_TheoryArgumentsMustMatchTestMethodParameters_IncompatibleTypes,
			Descriptors.X1040_TheoryArgumentsMustMatchTestMethodParameters_IncompatibleNullability,
			Descriptors.X1050_ClassDataTheoryDataRowIsRecommendedForStronglyTypedAnalysis
		)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		var compilation = context.Compilation;
		var iEnumerableOfObjectArray = TypeSymbolFactory.IEnumerableOfObjectArray(compilation);
		var iEnumerableOfTheoryDataRow = TypeSymbolFactory.IEnumerableOfITheoryDataRow(compilation);
		var iAsyncEnumerableOfObjectArray = TypeSymbolFactory.IAsyncEnumerableOfObjectArray(compilation);
		var iAsyncEnumerableOfTheoryDataRow = TypeSymbolFactory.IAsyncEnumerableOfITheoryDataRow(compilation);
		var theoryDataRowTypes = TypeSymbolFactory.TheoryDataRow_ByGenericArgumentCount_V3(compilation);
		var iTupleType = TypeSymbolFactory.ITuple(compilation);

		var classDataOfTType = xunitContext.V3Core?.ClassDataAttributeOfTType;

		context.RegisterOperationAction(context =>
		{
			if (context.ContainingSymbol is not IMethodSymbol testMethod)
				return;
			if (context.Operation is not IAttributeOperation { Operation: IObjectCreationOperation { Type: INamedTypeSymbol attributeType } attributeCreation } attributeOperation)
				return;

			var classType = default(INamedTypeSymbol);

			// [ClassData(typeof(...))]
			if (SymbolEqualityComparer.Default.Equals(attributeType, xunitContext.Core.ClassDataAttributeType))
			{
				if (attributeCreation.Arguments.FirstOrDefault()?.Value is not ITypeOfOperation typeOfOperation)
					return;

				classType = typeOfOperation.TypeOperand as INamedTypeSymbol;
			}
			// [ClassData<...>]
			else if (classDataOfTType is not null && SymbolEqualityComparer.Default.Equals(attributeType.OriginalDefinition, classDataOfTType))
				classType = attributeType.TypeArguments[0] as INamedTypeSymbol;

			if (classType is null || classType.Kind == SymbolKind.ErrorType)
				return;

			var attributeLocation = attributeOperation.Syntax.GetLocation();

			// Make sure the class implements a compatible interface
			var isValidDeclaration = VerifyDataSourceDeclaration(context, compilation, xunitContext, classType, attributeLocation);

			// Everything from here is based on ensuring I(Async)Enumerable<TheoryDataRow<>>, which is
			// only available in v3.
			if (!xunitContext.HasV3References)
				return;

			var rowType = classType.UnwrapEnumerable(compilation);
			if (rowType is null)
				return;

			if (IsGenericTheoryDataRowType(rowType, theoryDataRowTypes, out var theoryDataReturnType))
				VerifyGenericArgumentTypes(context, testMethod, theoryDataRowTypes[0], theoryDataReturnType, classType, attributeLocation);
			else if (IsTupleDataRowType(rowType, iTupleType, out var namedTupleType))
				VerifyGenericArgumentTypes(context, testMethod, namedTupleType, namedTupleType, classType, attributeLocation);
			else if (isValidDeclaration)
				ReportClassReturnsUnsafeTypeValue(context, attributeLocation);
		}, OperationKind.Attribute);
	}

	static Location? GetParameterTypeLocation(
		IParameterSymbol parameter,
		CancellationToken cancellationToken) =>
			parameter.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is ParameterSyntax { Type: { } parameterType }
				? parameterType.GetLocation()
				: null;

	static bool IsGenericTheoryDataRowType(
		ITypeSymbol? rowType,
		Dictionary<int, INamedTypeSymbol> theoryDataRowTypes,
		[NotNullWhen(true)] out INamedTypeSymbol? theoryReturnType)
	{
		theoryReturnType = default;

		var working = rowType as INamedTypeSymbol;
		for (; working is not null; working = working.BaseType)
		{
			var returnTypeArguments = working.TypeArguments;
			if (returnTypeArguments.Length != 0
				&& theoryDataRowTypes.TryGetValue(returnTypeArguments.Length, out var theoryDataType)
				&& SymbolEqualityComparer.Default.Equals(theoryDataType, working.OriginalDefinition))
				break;
		}

		if (working is null)
			return false;

		theoryReturnType = working;
		return true;
	}

	static bool IsTupleDataRowType(
		ITypeSymbol? rowType,
		INamedTypeSymbol? iTupleType,
		[NotNullWhen(true)] out INamedTypeSymbol? namedTupleType)
	{
		if (rowType is INamedTypeSymbol namedRowType &&
			iTupleType is not null &&
			rowType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iTupleType)))
		{
			namedTupleType = namedRowType;
			return true;
		}

		namedTupleType = default;
		return false;
	}

	static void ReportClassReturnsUnsafeTypeValue(
		OperationAnalysisContext context,
		Location attributeLocation) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1050_ClassDataTheoryDataRowIsRecommendedForStronglyTypedAnalysis,
					attributeLocation
				)
			);

	static void ReportExtraTypeArguments(
		OperationAnalysisContext context,
		Location attributeLocation,
		INamedTypeSymbol theoryDataType) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1038_TheoryArgumentsMustMatchTestMethodParameters_ExtraTypeParameters,
					attributeLocation,
					SymbolDisplay.ToDisplayString(theoryDataType)
				)
			);

	static void ReportIncompatibleType(
		OperationAnalysisContext context,
		Location parameterTypeLocation,
		ITypeSymbol theoryDataTypeParameter,
		INamedTypeSymbol namedClassType,
		IParameterSymbol parameter) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1039_TheoryArgumentsMustMatchTestMethodParameters_IncompatibleTypes,
					parameterTypeLocation,
					SymbolDisplay.ToDisplayString(theoryDataTypeParameter),
					SymbolDisplay.ToDisplayString(namedClassType),
					parameter.Name
				)
			);

	static void ReportIncorrectImplementationType(
		OperationAnalysisContext context,
		string validSymbols,
		Location attributeLocation,
		ITypeSymbol classType) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1007_ClassDataAttributeMustPointAtValidClass,
					attributeLocation,
					classType.Name,
					validSymbols
				)
			);

	static void ReportNullabilityMismatch(
		OperationAnalysisContext context,
		Location parameterTypeLocation,
		ITypeSymbol theoryDataTypeParameter,
		INamedTypeSymbol namedClassType,
		IParameterSymbol parameter) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1040_TheoryArgumentsMustMatchTestMethodParameters_IncompatibleNullability,
					parameterTypeLocation,
					SymbolDisplay.ToDisplayString(theoryDataTypeParameter),
					SymbolDisplay.ToDisplayString(namedClassType),
					parameter.Name
				)
			);

	static void ReportTooFewTypeArguments(
		OperationAnalysisContext context,
		Location attributeLocation,
		INamedTypeSymbol theoryDataType) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1037_TheoryArgumentsMustMatchTestMethodParameters_TooFewTypeParameters,
					attributeLocation,
					SymbolDisplay.ToDisplayString(theoryDataType)
				)
			);

	static bool VerifyDataSourceDeclaration(
		OperationAnalysisContext context,
		Compilation compilation,
		XunitContext xunitContext,
		INamedTypeSymbol classType,
		Location attributeLocation)
	{
		var v3 = xunitContext.HasV3References;
		var valid =
			classType.IsValidDataSource(v3, compilation) &&
			!classType.IsAbstract &&
			classType.InstanceConstructors.Any(c => c.Parameters.IsEmpty && c.DeclaredAccessibility == Accessibility.Public);

		if (!valid)
			ReportIncorrectImplementationType(context, v3 ? typesV3 : typesV2, attributeLocation, classType);

		return valid;
	}

	static void VerifyGenericArgumentTypes(
		OperationAnalysisContext context,
		IMethodSymbol testMethod,
		INamedTypeSymbol theoryDataType,
		INamedTypeSymbol theoryReturnType,
		ITypeSymbol classType,
		Location attributeLocation)
	{
		if (classType is not INamedTypeSymbol namedClassType)
			return;

		var returnTypeArguments = theoryReturnType.TypeArguments;
		var testMethodParameterSymbols = testMethod.Parameters;

		if (testMethodParameterSymbols.Length > returnTypeArguments.Length
			&& testMethodParameterSymbols.Skip(returnTypeArguments.Length).Any(p => !p.IsOptional && !p.IsParams))
		{
			ReportTooFewTypeArguments(context, attributeLocation, theoryDataType);
			return;
		}

		int typeArgumentIdx = 0, parameterTypeIdx = 0;
		for (; typeArgumentIdx < returnTypeArguments.Length && parameterTypeIdx < testMethodParameterSymbols.Length; typeArgumentIdx++)
		{
			var parameter = testMethodParameterSymbols[parameterTypeIdx];
			if (parameter.Type is null)
				continue;

			var parameterTypeLocation = GetParameterTypeLocation(parameter, context.CancellationToken);
			if (parameterTypeLocation is null)
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
				var report = true;

				// The user might be providing the full array for 'params'; if they do, we need to move
				// the parameter type index forward because it's been consumed by the array
				if (parameter.IsParams && parameter.Type.IsAssignableFrom(typeArgument))
				{
					report = false;
					parameterTypeIdx++;
				}

				if (report)
					ReportIncompatibleType(context, parameterTypeLocation, typeArgument, namedClassType, parameter);
			}

			// Nullability of value types is handled by the type compatibility test,
			// but nullability of reference types isn't
			if (parameterType.IsReferenceType
					&& typeArgument.IsReferenceType
					&& parameterType.NullableAnnotation == NullableAnnotation.NotAnnotated
					&& typeArgument.NullableAnnotation == NullableAnnotation.Annotated)
				ReportNullabilityMismatch(context, parameterTypeLocation, typeArgument, namedClassType, parameter);

			// Only move the parameter type index forward when the current parameter is not a 'params'
			if (!parameter.IsParams)
				parameterTypeIdx++;
		}

		if (typeArgumentIdx < returnTypeArguments.Length)
			ReportExtraTypeArguments(context, attributeLocation, theoryDataType);
	}
}
