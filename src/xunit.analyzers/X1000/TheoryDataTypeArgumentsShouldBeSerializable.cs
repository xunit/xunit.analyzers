using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TheoryDataTypeArgumentsShouldBeSerializable : XunitDiagnosticAnalyzer
{
	public TheoryDataTypeArgumentsShouldBeSerializable() :
		base(
			Descriptors.X1044_AvoidUsingTheoryDataTypeArgumentsThatAreNotSerializable,
			Descriptors.X1045_AvoidUsingTheoryDataTypeArgumentsThatMightNotBeSerializable
		)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		if (xunitContext.HasV3AotReferences)
			return;

		if (SerializableTypeSymbols.Create(context.Compilation, xunitContext) is not SerializableTypeSymbols typeSymbols)
			return;

		var finder = new TheoryDataTypeArgumentFinder(typeSymbols);
		var analyzer = new SerializabilityAnalyzer(typeSymbols);

		context.RegisterSyntaxNodeAction(context =>
		{
			var semanticModel = context.SemanticModel;
			var cancellationToken = context.CancellationToken;

			if (context.Node is not MethodDeclarationSyntax methodSyntax)
				return;
			if (semanticModel.GetDeclaredSymbol(methodSyntax, cancellationToken) is not IMethodSymbol method)
				return;
			if (method.ContainingType is not INamedTypeSymbol testClass)
				return;
			if (!method.GetAttributes().ContainsAttributeType(typeSymbols.TheoryAttribute))
				return;
			if (DiscoveryEnumerationIsDisabled(method, typeSymbols))
				return;

			var methodAttributes = method.GetAttributes();
			foreach (var attributeList in methodSyntax.AttributeLists)
			{
				foreach (var dataAttributeSyntax in attributeList.Attributes)
				{
					var dataAttribute = methodAttributes.FirstOrDefault(a => a.ApplicationSyntaxReference?.GetSyntax(cancellationToken) == dataAttributeSyntax);
					if (dataAttribute == null || !dataAttribute.IsInstanceOf(typeSymbols.DataAttribute))
						continue;
					cancellationToken.ThrowIfCancellationRequested();

					var (types, dataSource) = finder.FindTypeArgumentsInfo(dataAttribute, testClass);

					foreach (var type in types)
					{
						if (analyzer.TypeShouldBeIgnored(type))
							continue;

						var serializability = analyzer.AnalyzeSerializability(type, xunitContext);

						if (serializability != Serializability.AlwaysSerializable)
						{
							if (CanStaticallyVerifyAllValuesAreSerializable(dataSource, semanticModel, analyzer, xunitContext, cancellationToken))
								continue;

							context.ReportDiagnostic(
								Diagnostic.Create(
									serializability == Serializability.NeverSerializable
										? Descriptors.X1044_AvoidUsingTheoryDataTypeArgumentsThatAreNotSerializable
										: Descriptors.X1045_AvoidUsingTheoryDataTypeArgumentsThatMightNotBeSerializable,
									dataAttributeSyntax.GetLocation(),
									type.ToMinimalDisplayString(semanticModel, dataAttributeSyntax.SpanStart)
								)
							);
						}
					}
				}
			}
		}, SyntaxKind.MethodDeclaration);
	}

	static bool CanStaticallyVerifyAllValuesAreSerializable(
		ISymbol? dataSource,
		SemanticModel semanticModel,
		SerializabilityAnalyzer analyzer,
		XunitContext xunitContext,
		CancellationToken cancellationToken)
	{
		if (dataSource is null)
			return false;

		var syntaxReferences = dataSource.DeclaringSyntaxReferences;
		if (syntaxReferences.Length == 0)
			return false;

		bool foundAnyInitializer = false;

		foreach (var syntaxRef in syntaxReferences)
		{
			var syntax = syntaxRef.GetSyntax(cancellationToken);
			if (syntax.SyntaxTree != semanticModel.SyntaxTree)
				return false;

			ExpressionSyntax? initializerExpression = null;

			if (syntax is PropertyDeclarationSyntax property)
			{
				initializerExpression = property.Initializer?.Value ?? property.ExpressionBody?.Expression;
				if (initializerExpression == null && property.AccessorList != null)
				{
					var getter = property.AccessorList.Accessors.FirstOrDefault(a => a.Keyword.IsKind(SyntaxKind.GetKeyword));
					if (getter?.Body?.Statements.Count == 1 && getter.Body.Statements[0] is ReturnStatementSyntax returnStmt)
						initializerExpression = returnStmt.Expression;
					else if (getter?.ExpressionBody != null)
						initializerExpression = getter.ExpressionBody.Expression;
				}
			}
			else if (syntax is VariableDeclaratorSyntax variableDeclarator)
			{
				initializerExpression = variableDeclarator.Initializer?.Value;
			}
			else if (syntax is MethodDeclarationSyntax method)
			{
				initializerExpression = method.ExpressionBody?.Expression;
				if (initializerExpression == null && method.Body?.Statements.Count == 1 && method.Body.Statements[0] is ReturnStatementSyntax returnStmt)
				{
					initializerExpression = returnStmt.Expression;
				}
			}
			else if (syntax is ClassDeclarationSyntax _)
			{
				return false;
			}

			if (initializerExpression == null)
				continue;

			foundAnyInitializer = true;

			if (!IsExpressionAllSerializable(initializerExpression, semanticModel, analyzer, xunitContext, cancellationToken))
				return false;
		}

		return foundAnyInitializer;
	}

	static bool IsExpressionAllSerializable(
		ExpressionSyntax expression,
		SemanticModel model,
		SerializabilityAnalyzer analyzer,
		XunitContext xunitContext,
		CancellationToken cancellationToken)
	{
		InitializerExpressionSyntax? initializer = null;
		if (expression is ObjectCreationExpressionSyntax objCreation)
			initializer = objCreation.Initializer;
		else if (expression is ImplicitObjectCreationExpressionSyntax implicitObjCreation)
			initializer = implicitObjCreation.Initializer;

		if (initializer == null || !initializer.IsKind(SyntaxKind.CollectionInitializerExpression))
			return false;

		if (initializer.Expressions.Count == 0)
			return false;

		foreach (var element in initializer.Expressions)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (element is InitializerExpressionSyntax complexElement)
			{
				foreach (var innerElement in complexElement.Expressions)
				{
					if (!IsValueSerializable(innerElement, model, analyzer, xunitContext, cancellationToken))
						return false;
				}
			}
			else
			{
				if (!IsValueSerializable(element, model, analyzer, xunitContext, cancellationToken))
					return false;
			}
		}

		return true;
	}

	static bool IsValueSerializable(
		ExpressionSyntax expression,
		SemanticModel model,
		SerializabilityAnalyzer analyzer,
		XunitContext xunitContext,
		CancellationToken cancellationToken)
	{
		var expr = expression;
		while (true)
		{
			if (expr is CastExpressionSyntax castExpr)
				expr = castExpr.Expression;
			else if (expr is ParenthesizedExpressionSyntax paren)
				expr = paren.Expression;
			else
				break;
		}

		if (expr.IsKind(SyntaxKind.NullLiteralExpression))
			return true;

		var typeInfo = model.GetTypeInfo(expr, cancellationToken);
		if (typeInfo.Type == null || analyzer.AnalyzeSerializability(typeInfo.Type, xunitContext) != Serializability.AlwaysSerializable)
			return false;

		return true;
	}

	static bool AttributeIsTheoryOrDataAttribute(
		AttributeData attribute,
		SerializableTypeSymbols typeSymbols) =>
			attribute.IsInstanceOf(typeSymbols.TheoryAttribute, exactMatch: true) || attribute.IsInstanceOf(typeSymbols.DataAttribute);

	static bool DiscoveryEnumerationIsDisabled(
		IMethodSymbol method,
		SerializableTypeSymbols typeSymbols) =>
			method
				.GetAttributes()
				.Where(attribute => AttributeIsTheoryOrDataAttribute(attribute, typeSymbols))
				.SelectMany(attribute => attribute.NamedArguments)
				.Any(argument => argument.Key == "DisableDiscoveryEnumeration" && argument.Value.Value is true);

	sealed class TheoryDataTypeArgumentFinder(SerializableTypeSymbols typeSymbols)
	{
		const string MemberType = nameof(MemberType);

		/// <summary>
		/// Find all TheoryData type arguments for a data source referenced by the given data attribute
		/// in the given test class, if applicable. If the data source's type is not compatible with
		/// a generic TheoryData class, such as if it is a member with the type <see cref="IEnumerable{T}"/>
		/// of <see cref="object"/>[], then no type arguments will be found.
		/// </summary>
		public (IEnumerable<ITypeSymbol> TypeArguments, ISymbol? DataSource) FindTypeArgumentsInfo(
			AttributeData dataAttribute,
			INamedTypeSymbol testClass)
		{
			var dataSource = FindDataSourceSymbol(dataAttribute, testClass);
			var type = dataSource switch
			{
				INamedTypeSymbol namedType => namedType,
				_ => GetMemberType(dataSource) as INamedTypeSymbol
			};

			return (type is not null ? GetTheoryDataTypeArguments(type) : [], dataSource);
		}

		ISymbol? FindDataSourceSymbol(
			AttributeData dataAttribute,
			INamedTypeSymbol testClass)
		{
			if (dataAttribute.IsInstanceOf(typeSymbols.ClassDataAttribute, exactMatch: true))
				return dataAttribute.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol;

			if (dataAttribute.IsInstanceOf(typeSymbols.MemberDataAttribute, exactMatch: true))
				return GetMemberSymbol(dataAttribute, testClass);

			return null;
		}

		static IMethodSymbol? GetCompatibleMethod(
			ITypeSymbol type,
			string name,
			ImmutableArray<TypedConstant> arguments)
		{
			foreach (var currentType in GetTypesForMemberResolution(type, includeInterfaces: true))
			{
				var methods =
					currentType
						.GetMembers(name)
						.Where(member => member.DeclaredAccessibility == Accessibility.Public)
						.Select(member => member as IMethodSymbol)
						.WhereNotNull()
						.Where(method => ParameterTypesAreCompatible(method.Parameters, arguments))
						.ToArray();

				if (methods.Length == 0)
					continue;

				if (methods.Length == 1)
					return methods[0];

				return methods.FirstOrDefault(method => method.Parameters.Length == arguments.Length);
			}

			return null;
		}

		/// <remarks>
		/// The logic in this method corresponds to the logic in MemberDataAttributeBase.GetFieldAccessor.
		/// </remarks>
		static IFieldSymbol? GetField(
			ITypeSymbol type,
			string name)
		{
			var field = GetPublicMember<IFieldSymbol>(type, name, includeInterfaces: false);

			if (field is not null && field.IsStatic)
				return field;

			return null;
		}

		static ITypeSymbol? GetMemberContainingType(AttributeData memberDataAttribute) =>
			memberDataAttribute
				.NamedArguments
				.Where(namedArgument => namedArgument.Key == MemberType)
				.Select(namedArgument => namedArgument.Value.Type)
				.WhereNotNull()
				.FirstOrDefault();

		/// <remarks>
		/// The logic in this method corresponds to the logic in MemberDataAttributeBase.GetData.
		/// </remarks>
		static ISymbol? GetMemberSymbol(
			AttributeData memberDataAttribute,
			INamedTypeSymbol testClass)
		{
			var name = memberDataAttribute.ConstructorArguments.FirstOrDefault();

			if (name.Value is string memberName)
			{
				var containingType = GetMemberContainingType(memberDataAttribute) ?? testClass;

				return GetProperty(containingType, memberName)
						?? GetField(containingType, memberName)
						?? GetMethod(containingType, memberName, memberDataAttribute) as ISymbol;
			}

			return null;
		}

		static ITypeSymbol? GetMemberType(ISymbol? member) =>
			member switch
			{
				IPropertySymbol property => property.Type,
				IFieldSymbol field => field.Type,
				IMethodSymbol method when !method.ReturnsVoid => method.ReturnType,
				_ => null,
			};

		/// <remarks>
		/// The logic in this method corresponds to the logic in MemberDataAttributeBase.GetMethodAccessor.
		/// </remarks>
		static IMethodSymbol? GetMethod(
			ITypeSymbol type,
			string name,
			AttributeData memberDataAttribute)
		{
			var arguments =
				memberDataAttribute.ConstructorArguments.Length > 1
					? memberDataAttribute.ConstructorArguments[1].Values
					: ImmutableArray<TypedConstant>.Empty;
			var method = GetCompatibleMethod(type, name, arguments);

			if (method is not null && method.IsStatic)
				return method;

			return null;
		}

		/// <remarks>
		/// The logic in this method corresponds to the logic in MemberDataAttributeBase.GetPropertyAccessor.
		/// </remarks>
		static IPropertySymbol? GetProperty(
			ITypeSymbol type,
			string name)
		{
			var property = GetPublicMember<IPropertySymbol>(type, name, includeInterfaces: true);

			if (property is not null && property.GetMethod is not null && property.GetMethod.IsStatic)
				return property;

			return null;
		}

		static TSymbol? GetPublicMember<TSymbol>(
			ITypeSymbol type,
			string name,
			bool includeInterfaces)
			where TSymbol : class, ISymbol
		{
			foreach (var currentType in GetTypesForMemberResolution(type, includeInterfaces))
			{
				var member =
					currentType
						.GetMembers(name)
						.Where(member => member.DeclaredAccessibility == Accessibility.Public)
						.Select(member => member as TSymbol)
						.WhereNotNull()
						.FirstOrDefault();

				if (member is not null)
					return member;
			}

			return null;
		}

		IEnumerable<ITypeSymbol> GetTheoryDataTypeArguments(INamedTypeSymbol type)
		{
			if (type.TypeKind != TypeKind.Class || type.SpecialType != SpecialType.None)
				return [];

			if (typeSymbols.TheoryDataBaseType is not INamedTypeSymbol theoryDataBaseType)
				return [];

			// For v2 and early versions of v3, the base type is "TheoryData" (non-generic).
			// For later versions of v3, it's "TheoryDataBase<TTheoryDataRow, TRawDataRow>".
			// We need to compare unbound to unbound when the type is generic.
			if (theoryDataBaseType.IsGenericType)
				theoryDataBaseType = theoryDataBaseType.ConstructUnboundGenericType();

			for (var currentType = type; currentType is not null; currentType = currentType.BaseType)
			{
				var baseType = currentType.BaseType;
				if (baseType?.IsGenericType == true)
					baseType = baseType.ConstructUnboundGenericType();

				if (theoryDataBaseType.Equals(baseType, SymbolEqualityComparer.Default))
				{
					var theoryDataType = typeSymbols.TheoryData(currentType.Arity);
					if (currentType.ConstructedFrom.Equals(theoryDataType, SymbolEqualityComparer.Default))
						return currentType.TypeArguments;
				}
			}

			return [];
		}

		/// <remarks>
		/// The logic in this method corresponds to the logic in MemberDataAttributeBase.GetTypesForMemberResolution.
		/// </remarks>
		static IEnumerable<ITypeSymbol> GetTypesForMemberResolution(
			ITypeSymbol type,
			bool includeInterfaces)
		{
			for (var currentType = type; currentType is not null; currentType = currentType.BaseType)
				yield return currentType;

			if (includeInterfaces)
				foreach (var @interface in type.AllInterfaces)
					yield return @interface;
		}

		/// <remarks>
		/// The logic in this method corresponds to the logic in MemberDataAttributeBase.ParameterTypesCompatible.
		/// </remarks>
		static bool ParameterTypesAreCompatible(
			ImmutableArray<IParameterSymbol> parameters,
			ImmutableArray<TypedConstant> arguments)
		{
			if (parameters.Length < arguments.Length)
				return false;

			var i = 0;
			for (; i < arguments.Length; i++)
				if (arguments[i].Type is ITypeSymbol argumentType && !parameters[i].Type.IsAssignableFrom(argumentType))
					return false;

			for (; i < parameters.Length; i++)
				if (!parameters[i].IsOptional)
					return false;

			return true;
		}
	}
}
