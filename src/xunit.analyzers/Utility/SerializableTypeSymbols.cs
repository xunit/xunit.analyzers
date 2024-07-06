using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Xunit.Analyzers;

public sealed class SerializableTypeSymbols
{
	readonly Lazy<INamedTypeSymbol?> bigInteger;
	readonly Lazy<INamedTypeSymbol?> dateOnly;
	readonly Lazy<INamedTypeSymbol?> dateTimeOffset;
	readonly Lazy<INamedTypeSymbol?> iXunitSerializable;
	readonly Dictionary<int, INamedTypeSymbol> theoryDataTypes;
	readonly Lazy<INamedTypeSymbol?> timeOnly;
	readonly Lazy<INamedTypeSymbol?> timeSpan;
	readonly Lazy<INamedTypeSymbol?> traitDictionary;
	readonly Lazy<INamedTypeSymbol?> type;

	SerializableTypeSymbols(
		Compilation compilation,
		XunitContext xunitContext,
		INamedTypeSymbol classDataAttribute,
		INamedTypeSymbol dataAttribute,
		INamedTypeSymbol memberDataAttribute,
		INamedTypeSymbol theoryAttribute,
		Dictionary<int, INamedTypeSymbol> theoryDataTypes)
	{
		this.theoryDataTypes = theoryDataTypes;

		bigInteger = new(() => TypeSymbolFactory.BigInteger(compilation));
		dateOnly = new(() => TypeSymbolFactory.DateOnly(compilation));
		dateTimeOffset = new(() => TypeSymbolFactory.DateTimeOffset(compilation));
		iXunitSerializable = new(() => xunitContext.Abstractions.IXunitSerializableType);
		type = new(() => TypeSymbolFactory.Type(compilation));
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

	public static SerializableTypeSymbols? Create(
		Compilation compilation,
		XunitContext xunitContext)
	{
		Guard.ArgumentNotNull(compilation);
		Guard.ArgumentNotNull(xunitContext);

		if (xunitContext.Core.TheoryAttributeType is not INamedTypeSymbol theoryAttribute)
			return null;
		if (xunitContext.Core.DataAttributeType is not INamedTypeSymbol dataAttribute)
			return null;
		if (xunitContext.Core.ClassDataAttributeType is not INamedTypeSymbol classDataAttribute)
			return null;
		if (xunitContext.Core.MemberDataAttributeType is not INamedTypeSymbol memberDataAttribute)
			return null;

		return new SerializableTypeSymbols(
			compilation,
			xunitContext,
			classDataAttribute,
			dataAttribute,
			memberDataAttribute,
			theoryAttribute,
			TypeSymbolFactory.TheoryData_ByGenericArgumentCount(compilation)
		);
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
		theoryDataTypes.TryGetValue(arity, out var result);
		return result;
	}
}
