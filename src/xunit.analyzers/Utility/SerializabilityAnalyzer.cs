using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public sealed class SerializabilityAnalyzer(SerializableTypeSymbols typeSymbols)
{
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

		if (typeSymbols.TypesWithCustomSerializers.Any(t => t.IsAssignableFrom(type)))
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
	/// Enumerations are serializable if and only if they are not from the Global Assembly Cache,
	/// which exists in .NET Framework only. However, static analysis cannot reliably determine
	/// whether a type is from a local assembly or the GAC. Therefore, <see cref="TypeKind.Enum"/>
	/// and <see cref="SpecialType.System_Enum"/> are ignored, in order to prevent a diagnostic from
	/// being always found for all enumeration types.
	/// </remarks>
	public bool TypeShouldBeIgnored([NotNullWhen(false)] ITypeSymbol? type)
	{
		if (type is null)
			return true;

		if (TypeKindShouldBeIgnored(type.TypeKind) || type.SpecialType == SpecialType.System_Enum)
			return true;

		if (type is IArrayTypeSymbol arrayType)
			return TypeShouldBeIgnored(arrayType.ElementType);

		if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
			return namedType.TypeArguments.Where(TypeShouldBeIgnored).Any();

		return false;
	}
}
