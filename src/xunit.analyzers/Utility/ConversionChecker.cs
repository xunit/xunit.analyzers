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

	static readonly Dictionary<SpecialType, Type> StringConversionTargets = new()
	{
		{ SpecialType.System_Boolean, typeof(bool) },
		{ SpecialType.System_Char, typeof(char) },
		{ SpecialType.System_SByte, typeof(sbyte) },
		{ SpecialType.System_Byte, typeof(byte) },
		{ SpecialType.System_Int16, typeof(short) },
		{ SpecialType.System_UInt16, typeof(ushort) },
		{ SpecialType.System_Int32, typeof(int) },
		{ SpecialType.System_UInt32, typeof(uint) },
		{ SpecialType.System_Int64, typeof(long) },
		{ SpecialType.System_UInt64, typeof(ulong) },
		{ SpecialType.System_Single, typeof(float) },
		{ SpecialType.System_Double, typeof(double) },
		{ SpecialType.System_Decimal, typeof(decimal) },
	};

	public static bool IsConvertible(
		Compilation compilation,
		ITypeSymbol source,
		ITypeSymbol destination,
		XunitContext xunitContext,
		object? valueSource = null,
		bool valueIsConvertedAtRuntime = false)
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
			|| (xunitContext.Core.TheorySupportsConversionFromStringToDateTimeOffsetAndGuid && IsDateTimeOffsetOrGuid(destination)))
		{
			// Allow all conversions from strings. All parsing issues will be reported at runtime.
			return source.SpecialType == SpecialType.System_String;
		}

		if (valueIsConvertedAtRuntime && CanConvertStringValue(source, destination, valueSource))
			return true;

		// User-defined conversion not supported in AOT
		if (xunitContext.HasV3AotReferences && conversion.IsUserDefined)
			return false;

		// Rules of last resort
		return conversion.IsImplicit
			|| conversion.IsUnboxing
			|| (conversion.IsExplicit && conversion.IsEnumeration)
			|| (conversion.IsExplicit && conversion.IsUserDefined)
			|| (conversion.IsExplicit && conversion.IsNullable);
	}

	static bool CanConvertStringValue(
		ITypeSymbol source,
		ITypeSymbol destination,
		object? valueSource)
	{
		if (source.SpecialType != SpecialType.System_String || valueSource is not string stringValue)
			return false;

		if (!StringConversionTargets.TryGetValue(destination.SpecialType, out var destinationType))
			return false;

		try
		{
			Convert.ChangeType(stringValue, destinationType, CultureInfo.InvariantCulture);
			return true;
		}
		catch (FormatException)
		{
			return false;
		}
		catch (OverflowException)
		{
			return false;
		}
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

		return destination.MetadataName is (nameof(DateTimeOffset)) or (nameof(Guid));
	}

	static bool IsSigned(ITypeSymbol typeSymbol) =>
		SignedIntegralTypes.Contains(typeSymbol.SpecialType);

	static bool IsUnsigned(ITypeSymbol typeSymbol) =>
		UnsignedIntegralTypes.Contains(typeSymbol.SpecialType);
}
