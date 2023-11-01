using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
			Descriptors.X1034_MemberDataMethodReturnsNullableWithNonNullableTestParameters
		)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		if (xunitContext.Core.TheoryAttributeType is null || xunitContext.Core.MemberDataAttributeType is null)
			return;

		var compilation = context.Compilation;

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

				var classSyntax = attributeSyntax.FirstAncestorOrSelf<ClassDeclarationSyntax>();
				if (classSyntax is null)
					continue;

				var testClassTypeSymbol = semanticModel.GetDeclaredSymbol(classSyntax);
				if (testClassTypeSymbol is null)
					continue;

				var declaredMemberTypeSymbol = memberTypeSymbol ?? testClassTypeSymbol;
				if (declaredMemberTypeSymbol is null)
					continue;

				// TODO: Adjust member lookup for method selection based on new default values logic
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
						var dataMethodSymbol = (IMethodSymbol)memberSymbol;
						var dataMethodParameterSymbols = dataMethodSymbol.Parameters;
						// TODO

						// Second check: method return type satisfies test method parameters' nullability
						var rowType = memberType.GetItemType();
						var itemType = rowType?.GetItemType();

						if (itemType is not null && itemType.NullableAnnotation == NullableAnnotation.Annotated)
						{
							var testMethodSymbol = semanticModel.GetDeclaredSymbol(testMethod, context.CancellationToken);
							if (testMethodSymbol is null)
								continue;
							var testMethodParameterSymbols = testMethodSymbol.Parameters;
							var testMethodParameterSyntaxes = testMethod.ParameterList.Parameters;

							// The method output may contain nulls, so validate whether the test method parameters are nullable
							for (int i = 0; i < testMethodParameterSymbols.Length; i++)
							{
								var parameter = testMethodParameterSymbols[i];
								if (parameter.Type is null)
									continue;

								if (parameter.Type.IsReferenceType && parameter.Type.NullableAnnotation == NullableAnnotation.NotAnnotated)
								{
									ReportNullableMethodReturnWithNonNullableTestParameter(
										context,
										dataMethodSymbol.Name,
										parameter.Type,
										testMethodParameterSyntaxes[i]);
								}
								else if (parameter.Type.IsValueType && parameter.Type.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
								{
									ReportNullableMethodReturnWithNonNullableTestParameter(
										context,
										dataMethodSymbol.Name,
										parameter.Type,
										testMethodParameterSyntaxes[i]);
								}
							}
						}
					}
				}
			}
		}, SyntaxKind.MethodDeclaration);
	}

	static IList<ExpressionSyntax>? GetParameterExpressionsFromArrayArgument(List<AttributeArgumentSyntax> arguments)
	{
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
			return null;

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

	static ISymbol? FindMethodSymbol(
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

	static void ReportNullableMethodReturnWithNonNullableTestParameter(
		SyntaxNodeAnalysisContext context,
		string methodName,
		ITypeSymbol typeSymbol,
		ParameterSyntax parameterSyntax) =>
			context.ReportDiagnostic(
				Diagnostic.Create(
					Descriptors.X1034_MemberDataMethodReturnsNullableWithNonNullableTestParameters,
					parameterSyntax.Type!.GetLocation(),
					methodName,
					typeSymbol.ToDisplayString()
				)
			);
}
