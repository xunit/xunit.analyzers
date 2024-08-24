using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Xunit.Analyzers;

static class ConversionChecker
{
	static readonly HashSet<SpecialType> SignedIntegralTypes = [
		SpecialType.System_SByte,
		SpecialType.System_Int16,
		SpecialType.System_Int32,
		SpecialType.System_Int64,
	];

	static readonly HashSet<SpecialType> UnsignedIntegralTypes = [
		SpecialType.System_Byte,
		SpecialType.System_UInt16,
		SpecialType.System_UInt32,
		SpecialType.System_UInt64,
	];

	public static bool IsConvertible(
		Compilation compilation,
		ITypeSymbol source,
		ITypeSymbol destination,
		XunitContext xunitContext,
		object? valueSource = null)
	{
		Guard.ArgumentNotNull(compilation);
		Guard.ArgumentNotNull(source);
		Guard.ArgumentNotNull(destination);
		Guard.ArgumentNotNull(xunitContext);

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
			return IsConvertibleNumeric(source, destination, valueSource);

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
		ITypeSymbol destination,
		object? valueSource = null)
	{
		var isIntegral = long.TryParse(valueSource?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var integralValue);
		if (isIntegral && integralValue < 0 && IsSigned(source) && IsUnsigned(destination))
			return false;

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

	static bool IsSigned(ITypeSymbol typeSymbol) =>
		SignedIntegralTypes.Contains(typeSymbol.SpecialType);

	static bool IsUnsigned(ITypeSymbol typeSymbol) =>
		UnsignedIntegralTypes.Contains(typeSymbol.SpecialType);
}
