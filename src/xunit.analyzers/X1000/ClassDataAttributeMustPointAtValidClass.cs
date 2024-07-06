using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ClassDataAttributeMustPointAtValidClass : XunitDiagnosticAnalyzer
{
	const string typesV2 = "IEnumerable<object[]>";
	const string typesV3 = "IEnumerable<object[]>, IAsyncEnumerable<object[]>, IEnumerable<ITheoryDataRow>, or IAsyncEnumerable<ITheoryDataRow>";

	public ClassDataAttributeMustPointAtValidClass() :
		base(
			Descriptors.X1007_ClassDataAttributeMustPointAtValidClass,
			Descriptors.X1037_TheoryDataTypeArgumentsMustMatchTestMethodParameters_TooFewTypeParameters,
			Descriptors.X1038_TheoryDataTypeArgumentsMustMatchTestMethodParameters_ExtraTypeParameters,
			Descriptors.X1039_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes,
			Descriptors.X1040_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability,
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

		context.RegisterSyntaxNodeAction(context =>
		{
			if (context.Node is not MethodDeclarationSyntax testMethod)
				return;

			var attributeLists = testMethod.AttributeLists;
			var semanticModel = context.SemanticModel;

			foreach (var attributeSyntax in attributeLists.WhereNotNull().SelectMany(attList => attList.Attributes))
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				// Only work against ClassDataAttribute
				if (!SymbolEqualityComparer.Default.Equals(semanticModel.GetTypeInfo(attributeSyntax).Type, xunitContext.Core.ClassDataAttributeType))
					continue;

				// Need the referenced type to do anything
				if (attributeSyntax.ArgumentList is null)
					continue;
				if (attributeSyntax.ArgumentList.Arguments[0].Expression is not TypeOfExpressionSyntax typeOfExpression)
					continue;
				if (semanticModel.GetTypeInfo(typeOfExpression.Type).Type is not INamedTypeSymbol classType)
					continue;
				if (classType.Kind == SymbolKind.ErrorType)
					continue;

				// Make sure the class implements a compatible interface
				var isValidDeclaration = VerifyDataSourceDeclaration(context, compilation, xunitContext, classType, attributeSyntax);

				// Everything from here is based on ensuring I(Async)Enumerable<TheoryDataRow<>>, which is
				// only available in v3.
				if (!xunitContext.HasV3References)
					continue;

				var rowType = classType.UnwrapEnumerable(compilation);
				if (rowType is null)
					continue;

				if (IsGenericTheoryDataRowType(rowType, theoryDataRowTypes, out var theoryDataReturnType))
					VerifyGenericArgumentTypes(semanticModel, context, testMethod, theoryDataRowTypes[0], theoryDataReturnType, classType, attributeSyntax);
				else if (isValidDeclaration)
					ReportClassReturnsUnsafeTypeValue(context, attributeSyntax);
			}
		}, SyntaxKind.MethodDeclaration);
	}

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

	static void ReportClassReturnsUnsafeTypeValue(
		SyntaxNodeAnalysisContext context,
		AttributeSyntax attribute) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1050_ClassDataTheoryDataRowIsRecommendedForStronglyTypedAnalysis,
					attribute.GetLocation()
				)
			);

	static void ReportExtraTypeArguments(
		SyntaxNodeAnalysisContext context,
		AttributeSyntax attribute,
		INamedTypeSymbol theoryDataType) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1038_TheoryDataTypeArgumentsMustMatchTestMethodParameters_ExtraTypeParameters,
					attribute.GetLocation(),
					SymbolDisplay.ToDisplayString(theoryDataType)
				)
			);

	static void ReportIncompatibleType(
		SyntaxNodeAnalysisContext context,
		TypeSyntax parameterType,
		ITypeSymbol theoryDataTypeParameter,
		INamedTypeSymbol namedClassType,
		IParameterSymbol parameter) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1039_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleTypes,
					parameterType.GetLocation(),
					SymbolDisplay.ToDisplayString(theoryDataTypeParameter),
					SymbolDisplay.ToDisplayString(namedClassType),
					parameter.Name
				)
			);

	static void ReportIncorrectImplementationType(
		SyntaxNodeAnalysisContext context,
		string validSymbols,
		AttributeSyntax attribute,
		ITypeSymbol classType) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1007_ClassDataAttributeMustPointAtValidClass,
					attribute.GetLocation(),
					classType.Name,
					validSymbols
				)
			);

	static void ReportNullabilityMismatch(
		SyntaxNodeAnalysisContext context,
		TypeSyntax parameterType,
		ITypeSymbol theoryDataTypeParameter,
		INamedTypeSymbol namedClassType,
		IParameterSymbol parameter) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1040_TheoryDataTypeArgumentsMustMatchTestMethodParameters_IncompatibleNullability,
					parameterType.GetLocation(),
					SymbolDisplay.ToDisplayString(theoryDataTypeParameter),
					SymbolDisplay.ToDisplayString(namedClassType),
					parameter.Name
				)
			);

	static void ReportTooFewTypeArguments(
		SyntaxNodeAnalysisContext context,
		AttributeSyntax attribute,
		INamedTypeSymbol theoryDataType) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1037_TheoryDataTypeArgumentsMustMatchTestMethodParameters_TooFewTypeParameters,
					attribute.GetLocation(),
					SymbolDisplay.ToDisplayString(theoryDataType)
				)
			);

	static bool VerifyDataSourceDeclaration(
		SyntaxNodeAnalysisContext context,
		Compilation compilation,
		XunitContext xunitContext,
		INamedTypeSymbol classType,
		AttributeSyntax attribute)
	{
		var v3 = xunitContext.HasV3References;
		var iEnumerableOfObjectArrayType = TypeSymbolFactory.IEnumerableOfObjectArray(compilation);
		var iEnumerableOfTheoryDataRowType = TypeSymbolFactory.IEnumerableOfITheoryDataRow(compilation);
		var iAsyncEnumerableOfObjectArrayType = TypeSymbolFactory.IAsyncEnumerableOfObjectArray(compilation);
		var iAsyncEnumerableOfTheoryDataRowType = TypeSymbolFactory.IAsyncEnumerableOfITheoryDataRow(compilation);

		// Make sure we implement one of the interfaces
		var valid = iEnumerableOfObjectArrayType.IsAssignableFrom(classType);

		if (!valid && v3 && iAsyncEnumerableOfObjectArrayType is not null)
			valid = iAsyncEnumerableOfObjectArrayType.IsAssignableFrom(classType);

		if (!valid && v3 && iEnumerableOfTheoryDataRowType is not null)
			valid = iEnumerableOfTheoryDataRowType.IsAssignableFrom(classType);

		if (!valid && v3 && iAsyncEnumerableOfTheoryDataRowType is not null)
			valid = iAsyncEnumerableOfTheoryDataRowType.IsAssignableFrom(classType);

		// Also make sure we're non-abstract and have an empty constructor
		valid =
			valid &&
			!classType.IsAbstract &&
			classType.InstanceConstructors.Any(c => c.Parameters.IsEmpty && c.DeclaredAccessibility == Accessibility.Public);

		if (!valid)
			ReportIncorrectImplementationType(
				context,
				xunitContext.HasV3References ? typesV3 : typesV2,
				attribute,
				classType
			);

		return valid;
	}

	static void VerifyGenericArgumentTypes(
		SemanticModel semanticModel,
		SyntaxNodeAnalysisContext context,
		MethodDeclarationSyntax testMethod,
		INamedTypeSymbol theoryDataType,
		INamedTypeSymbol theoryReturnType,
		ITypeSymbol classType,
		AttributeSyntax attribute)
	{
		if (classType is not INamedTypeSymbol namedClassType)
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
			ReportTooFewTypeArguments(context, attribute, theoryDataType);
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
					ReportIncompatibleType(context, parameterSyntax.Type, typeArgument, namedClassType, parameter);
			}

			// Nullability of value types is handled by the type compatibility test,
			// but nullability of reference types isn't
			if (parameterType.IsReferenceType
					&& typeArgument.IsReferenceType
					&& parameterType.NullableAnnotation == NullableAnnotation.NotAnnotated
					&& typeArgument.NullableAnnotation == NullableAnnotation.Annotated)
				ReportNullabilityMismatch(context, parameterSyntax.Type, typeArgument, namedClassType, parameter);

			// Only move the parameter type index forward when the current parameter is not a 'params'
			if (!parameter.IsParams)
				parameterTypeIdx++;
		}

		if (typeArgumentIdx < returnTypeArguments.Length)
			ReportExtraTypeArguments(context, attribute, theoryDataType);
	}
}
