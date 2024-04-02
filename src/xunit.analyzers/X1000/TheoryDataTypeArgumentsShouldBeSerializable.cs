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
public class TheoryDataTypeArgumentsShouldBeSerializable : XunitDiagnosticAnalyzer
{
	public TheoryDataTypeArgumentsShouldBeSerializable() :
		base(
			Descriptors.X1044_TheoryDataTypeArgumentsShouldBeSerializable,
			Descriptors.X1045_TheoryDataTypeArgumentsShouldBeDefinitelySerializable
		)
	{ }

	public override void AnalyzeCompilation(
		CompilationStartAnalysisContext context,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(context);
		Guard.ArgumentNotNull(xunitContext);

		if (TypeSymbols.Create(context.Compilation, xunitContext) is not TypeSymbols typeSymbols)
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

			var dataAttributes =
				methodSyntax
					.AttributeLists
					.SelectMany(list => list.Attributes)
					.Zip(method.GetAttributes(), (attributeSyntax, attribute) => (attributeSyntax, attribute))
					.Where((tuple) => tuple.attribute.IsInstanceOf(typeSymbols.DataAttribute));

			foreach ((var dataAttributeSyntax, var dataAttribute) in dataAttributes)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var types = finder.FindTypeArguments(dataAttribute, testClass);

				foreach (var type in types)
				{
					if (analyzer.TypeShouldBeIgnored(type))
						continue;

					var serializability = analyzer.AnalayzeSerializability(type);

					if (serializability != Serializability.AlwaysSerializable)
						context.ReportDiagnostic(
							Diagnostic.Create(
								serializability == Serializability.NeverSerializable
									? Descriptors.X1044_TheoryDataTypeArgumentsShouldBeSerializable
									: Descriptors.X1045_TheoryDataTypeArgumentsShouldBeDefinitelySerializable,
								dataAttributeSyntax.GetLocation(),
								type.ToMinimalDisplayString(semanticModel, dataAttributeSyntax.SpanStart)
							)
						);
				}
			}
		}, SyntaxKind.MethodDeclaration);
	}

	enum Serializability
	{
		NeverSerializable,
		PossiblySerializable,
		AlwaysSerializable
	}

	sealed class SerializabilityAnalyzer
	{
		readonly TypeSymbols typeSymbols;

		public SerializabilityAnalyzer(TypeSymbols typeSymbols) =>
			this.typeSymbols = typeSymbols;

		/// <summary>
		/// Analyze the given type to determine whether it is always, possibly, or never serializable.
		/// </summary>
		/// <remarks>
		/// The logic in this method corresponds to the logic in SerializationHelper.IsSerializable
		/// and SerializationHelper.Serialize.
		/// </remarks>
		public Serializability AnalayzeSerializability(ITypeSymbol type)
		{
			type = type.UnwrapNullable();

			if (GetTypeKindSerializability(type.TypeKind) == Serializability.NeverSerializable)
				return Serializability.NeverSerializable;

			if (type.TypeKind == TypeKind.Array && type is IArrayTypeSymbol arrayType)
				return AnalayzeSerializability(arrayType.ElementType);

			if (typeSymbols.Type.IsAssignableFrom(type))
				return Serializability.AlwaysSerializable;

			if (type.Equals(typeSymbols.TraitDictionary, SymbolEqualityComparer.Default))
				return Serializability.AlwaysSerializable;

			if (typeSymbols.IXunitSerializable.IsAssignableFrom(type))
				return Serializability.AlwaysSerializable;

			if (type.SpecialType != SpecialType.None)
				return GetSpecialTypeSerializability(type.SpecialType);

			if (type.Equals(typeSymbols.BigInteger, SymbolEqualityComparer.Default)
				|| type.Equals(typeSymbols.DateTimeOffset, SymbolEqualityComparer.Default)
				|| type.Equals(typeSymbols.TimeSpan, SymbolEqualityComparer.Default)
				|| type.Equals(typeSymbols.DateOnly, SymbolEqualityComparer.Default)
				|| type.Equals(typeSymbols.TimeOnly, SymbolEqualityComparer.Default))
				return Serializability.AlwaysSerializable;

			if (type.TypeKind == TypeKind.Class && !type.IsSealed)
				return Serializability.PossiblySerializable;

			if (type.TypeKind == TypeKind.Interface)
				return Serializability.PossiblySerializable;

			if (type.TypeKind == TypeKind.Enum)
				return Serializability.PossiblySerializable;

			return Serializability.NeverSerializable;
		}

		static Serializability GetSpecialTypeSerializability(SpecialType type) =>
			type switch
			{
				SpecialType.System_String
					or SpecialType.System_Char
					or SpecialType.System_Byte
					or SpecialType.System_SByte
					or SpecialType.System_Int16
					or SpecialType.System_UInt16
					or SpecialType.System_Int32
					or SpecialType.System_UInt32
					or SpecialType.System_Int64
					or SpecialType.System_UInt64
					or SpecialType.System_Single
					or SpecialType.System_Double
					or SpecialType.System_Decimal
					or SpecialType.System_Boolean
					or SpecialType.System_DateTime => Serializability.AlwaysSerializable,

				SpecialType.None
					or SpecialType.System_Object
					or SpecialType.System_Array
					or SpecialType.System_Enum
					or SpecialType.System_ValueType
					or SpecialType.System_Nullable_T
					or SpecialType.System_Collections_IEnumerable
					or SpecialType.System_IDisposable => Serializability.PossiblySerializable,

				_ => Serializability.NeverSerializable
			};

		static Serializability GetTypeKindSerializability(TypeKind kind) =>
			kind switch
			{
				TypeKind.Array
					or TypeKind.Class
					or TypeKind.Enum
					or TypeKind.Interface
					or TypeKind.Struct => Serializability.PossiblySerializable,

				_ => Serializability.NeverSerializable
			};

		static bool TypeKindShouldBeIgnored(TypeKind kind) =>
			kind switch
			{
				TypeKind.Unknown
					or TypeKind.Enum
					or TypeKind.Error
					or TypeKind.Module
					or TypeKind.TypeParameter
					or TypeKind.Submission => true,

				_ => false
			};

		/// <summary>
		/// Determine whether the given type should be ignored when analyzing serializability.
		/// Types are ignored by type kind (and special type for <see cref="SpecialType.System_Enum"/>).
		/// Arrays and generic types are ignored if they are composed of ignored types, recursively.
		/// </summary>
		/// <remarks>
		/// Enumerations are serializable if and only if they are from a local assembly and not from
		/// the Global Assembly Cache. However, static analysis cannot determine whether a type is from
		/// a local assembly or the GAC, because this requires trying to load the assembly by name
		/// using reflection, which is banned in analyzers. Therefore, <see cref="TypeKind.Enum"/> and
		/// <see cref="SpecialType.System_Enum"/> are ignored, in order to prevent a diagnostic from
		/// being always found for all enumeration types.
		/// </remarks>
		public bool TypeShouldBeIgnored(ITypeSymbol type)
		{
			if (TypeKindShouldBeIgnored(type.TypeKind) || type.SpecialType == SpecialType.System_Enum)
				return true;

			if (type is IArrayTypeSymbol arrayType)
				return TypeShouldBeIgnored(arrayType.ElementType);

			if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
				return namedType.TypeArguments.Where(TypeShouldBeIgnored).Any();

			return false;
		}
	}

	sealed class TheoryDataTypeArgumentFinder
	{
		const string MemberType = nameof(MemberType);

		readonly TypeSymbols typeSymbols;

		public TheoryDataTypeArgumentFinder(TypeSymbols typeSymbols) =>
			this.typeSymbols = typeSymbols;

		/// <summary>
		/// Find all TheoryData type arguments for a data source referenced by the given data attribute
		/// in the given test class, if applicable. If the data source's type is not compatible with
		/// a generic TheoryData class, such as if it is a member with the type <see cref="IEnumerable{T}"/>
		/// of <see cref="object"/>[], then no type arguments will be found.
		/// </summary>
		public IEnumerable<ITypeSymbol> FindTypeArguments(
			AttributeData dataAttribute,
			INamedTypeSymbol testClass) =>
				GetDataSourceType(dataAttribute, testClass) is INamedTypeSymbol type
					? GetTheoryDataTypeArguments(type)
					: [];

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

				return methods.Where(method => method.Parameters.Length == arguments.Length).FirstOrDefault();
			}

			return null;
		}

		INamedTypeSymbol? GetDataSourceType(
			AttributeData dataAttribute,
			INamedTypeSymbol testClass)
		{
			if (dataAttribute.IsInstanceOf(typeSymbols.ClassDataAttribute))
				return dataAttribute.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol;

			if (dataAttribute.IsInstanceOf(typeSymbols.MemberDataAttribute))
				return GetMemberType(dataAttribute, testClass) as INamedTypeSymbol;

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
		static ITypeSymbol? GetMemberType(
			AttributeData memberDataAttribute,
			INamedTypeSymbol testClass)
		{
			var name = memberDataAttribute.ConstructorArguments.FirstOrDefault();

			if (name.Value is string memberName)
			{
				var containingType = GetMemberContainingType(memberDataAttribute) ?? testClass;

				var member =
					GetProperty(containingType, memberName)
						?? GetField(containingType, memberName)
						?? GetMethod(containingType, memberName, memberDataAttribute) as ISymbol;

				return GetMemberType(member);
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
			var arguments = memberDataAttribute.ConstructorArguments[1].Values;
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

			if (typeSymbols.TheoryData(arity: 0) is not INamedTypeSymbol baseTheoryDataType)
				return [];

			for (var currentType = type; currentType is not null; currentType = currentType.BaseType)
			{
				if (baseTheoryDataType.Equals(currentType.BaseType, SymbolEqualityComparer.Default))
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

	sealed class TypeSymbols
	{
		readonly Lazy<INamedTypeSymbol?> bigInteger;
		readonly Compilation compilation;
		readonly Lazy<INamedTypeSymbol?> dateOnly;
		readonly Lazy<INamedTypeSymbol?> dateTimeOffset;
		readonly Lazy<INamedTypeSymbol?> iXunitSerializable;
		readonly Dictionary<int, INamedTypeSymbol?> theoryDataTypes;
		readonly Lazy<INamedTypeSymbol?> timeOnly;
		readonly Lazy<INamedTypeSymbol?> timeSpan;
		readonly Lazy<INamedTypeSymbol?> traitDictionary;
		readonly Lazy<INamedTypeSymbol?> type;

		TypeSymbols(
			Compilation compilation,
			XunitContext xunitContext,
			INamedTypeSymbol classDataAttribute,
			INamedTypeSymbol dataAttribute,
			INamedTypeSymbol memberDataAttribute,
			INamedTypeSymbol theoryAttribute)
		{
			this.compilation = compilation;

			bigInteger = new(() => TypeSymbolFactory.BigInteger(compilation));
			dateOnly = new(() => TypeSymbolFactory.DateOnly(compilation));
			dateTimeOffset = new(() => TypeSymbolFactory.DateTimeOffset(compilation));
			iXunitSerializable = new(() =>
				TypeSymbolFactory.IXunitSerializable_V3(compilation)
					?? xunitContext.V2Abstractions?.IXunitSerializableType
			);
			type = new(() => TypeSymbolFactory.Type(compilation));
			theoryDataTypes = [];
			timeOnly = new(() => TypeSymbolFactory.TimeOnly(compilation));
			timeSpan = new(() => TypeSymbolFactory.TimeSpan(compilation));
			traitDictionary = new(() => GetTraitDictionary(compilation));

			ClassDataAttribute = classDataAttribute;
			DataAttribute = dataAttribute;
			MemberDataAttribute = memberDataAttribute;
			TheoryAttribute = theoryAttribute;
		}

		public INamedTypeSymbol? BigInteger => bigInteger.Value;
		public INamedTypeSymbol ClassDataAttribute { get; }
		public INamedTypeSymbol DataAttribute { get; }
		public INamedTypeSymbol? DateOnly => dateOnly.Value;
		public INamedTypeSymbol? DateTimeOffset => dateTimeOffset.Value;
		public INamedTypeSymbol? IXunitSerializable => iXunitSerializable.Value;
		public INamedTypeSymbol MemberDataAttribute { get; }
		public INamedTypeSymbol TheoryAttribute { get; }
		public INamedTypeSymbol? TimeOnly => timeOnly.Value;
		public INamedTypeSymbol? TimeSpan => timeSpan.Value;
		public INamedTypeSymbol? TraitDictionary => traitDictionary.Value;
		public INamedTypeSymbol? Type => type.Value;

		public static TypeSymbols? Create(
			Compilation compilation,
			XunitContext xunitContext)
		{
			if (xunitContext.Core.TheoryAttributeType is not INamedTypeSymbol theoryAttribute)
				return null;
			if (xunitContext.Core.DataAttributeType is not INamedTypeSymbol dataAttribute)
				return null;
			if (xunitContext.Core.ClassDataAttributeType is not INamedTypeSymbol classDataAttribute)
				return null;
			if (xunitContext.Core.MemberDataAttributeType is not INamedTypeSymbol memberDataAttribute)
				return null;

			return new TypeSymbols(
				compilation,
				xunitContext,
				classDataAttribute,
				dataAttribute,
				memberDataAttribute,
				theoryAttribute);
		}

		static INamedTypeSymbol? GetTraitDictionary(Compilation compilation)
		{
			if (TypeSymbolFactory.DictionaryofTKeyTValue(compilation) is not INamedTypeSymbol dictionaryType)
				return null;

			if (TypeSymbolFactory.ListOfT(compilation) is not INamedTypeSymbol listType)
				return null;

			var stringType = compilation.GetSpecialType(SpecialType.System_String);
			var listOfStringType = listType.Construct(stringType);
			return dictionaryType.Construct(stringType, listOfStringType);
		}

		public INamedTypeSymbol? TheoryData(int arity)
		{
			if (!theoryDataTypes.ContainsKey(arity))
			{
				if (arity == 0)
					theoryDataTypes[arity] = TypeSymbolFactory.TheoryData(compilation);
				else
					theoryDataTypes[arity] = TypeSymbolFactory.TheoryDataN(compilation, arity);
			}

			return theoryDataTypes[arity];
		}
	}
}
