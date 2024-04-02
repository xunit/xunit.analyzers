using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataTypeArgumentsShouldBeSerializable>;

public class TheoryDataTypeArgumentsShouldBeSerializableTests
{
	const string IsNotSerializable = "is not serializable";
	const string MightNotBeSerializable = "might not be serializable";

	public static TheoryData<string, string, string, string> TheoryDataClass(
		string type1,
		string type2,
		string type3)
	{
		var source = $@"
using System;
using Xunit;

public class TestClass {{
    [Theory]
    [ClassData(typeof(DerivedClass))]
    public void TestMethod({type1} a, {type2} b, {type3} c) {{ }}
}}

public class BaseClass<T1, T2, T3, T4> : TheoryData<T1, T2, {type3}> {{ }}

public class DerivedClass : BaseClass<{type1}, {type2}, object, Delegate> {{ }}";

		return new TheoryData<string, string, string, string>
		{
			{ source, type1, type2, type3 }
		};
	}

	public static TheoryData<string, string, string> TheoryDataMembers(string type)
	{
		var @class = $@"public sealed class Class : TheoryData<{type}> {{ }}";
		var field = $@"public static readonly TheoryData<{type}> Field = new TheoryData<{type}>() {{ }};";
		var method = $@"public static TheoryData<{type}> Method(int a, string b) => new TheoryData<{type}>() {{ }};";
		var property = $@"public static TheoryData<{type}> Property => new TheoryData<{type}>() {{ }};";

		return new TheoryData<string, string, string>
		{
			{ @class, "ClassData(typeof(Class))", type },
			{ field, "MemberData(nameof(Field))", type },
			{ method, @"MemberData(nameof(Method), 1, ""2"")", type },
			{ property, "MemberData(nameof(Property))", type }
		};
	}

	[Fact]
	public async void GivenMethodWithoutAttributes_DoesNotFindDiagnostic()
	{
		var source = @"
public class TestClass {
    public void TestMethod() { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void GivenFactMethod_DoesNotFindDiagnostic()
	{
		var source = @"
public class TestClass {
    [Xunit.Fact]
    public void TestMethod() { }
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Fact]
	public async void GivenTheoryMethod_WithoutTheoryDataAsDataSource_DoesNotFindDiagnostic()
	{
		var source = @"
using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class TestClass {
    public static IEnumerable<object[]> Property1 => Array.Empty<object[]>();
    public static DataSource<IDisposable, Action> Property2 => new DataSource<IDisposable, Action>();

    [Theory]
    [MemberData(nameof(Property1))]
    [MemberData(nameof(Property2))]
    public void TestMethod(object a, object b) { }
}

public class DataSource<T1, T2> : IEnumerable<object[]> {
    public IEnumerator<object[]> GetEnumerator() { yield break; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(TheoryDataMembers), "string")]
	[MemberData(nameof(TheoryDataMembers), "string[]")]
	[MemberData(nameof(TheoryDataMembers), "string[][]")]
	[MemberData(nameof(TheoryDataMembers), "char")]
	[MemberData(nameof(TheoryDataMembers), "char?")]
	[MemberData(nameof(TheoryDataMembers), "byte")]
	[MemberData(nameof(TheoryDataMembers), "byte?")]
	[MemberData(nameof(TheoryDataMembers), "sbyte")]
	[MemberData(nameof(TheoryDataMembers), "sbyte?")]
	[MemberData(nameof(TheoryDataMembers), "short")]
	[MemberData(nameof(TheoryDataMembers), "short?")]
	[MemberData(nameof(TheoryDataMembers), "ushort")]
	[MemberData(nameof(TheoryDataMembers), "ushort?")]
	[MemberData(nameof(TheoryDataMembers), "int")]
	[MemberData(nameof(TheoryDataMembers), "int[]")]
	[MemberData(nameof(TheoryDataMembers), "int[][]")]
	[MemberData(nameof(TheoryDataMembers), "int?")]
	[MemberData(nameof(TheoryDataMembers), "int?[]")]
	[MemberData(nameof(TheoryDataMembers), "int?[][]")]
	[MemberData(nameof(TheoryDataMembers), "uint")]
	[MemberData(nameof(TheoryDataMembers), "uint?")]
	[MemberData(nameof(TheoryDataMembers), "long")]
	[MemberData(nameof(TheoryDataMembers), "long?")]
	[MemberData(nameof(TheoryDataMembers), "ulong")]
	[MemberData(nameof(TheoryDataMembers), "ulong?")]
	[MemberData(nameof(TheoryDataMembers), "float")]
	[MemberData(nameof(TheoryDataMembers), "float?")]
	[MemberData(nameof(TheoryDataMembers), "double")]
	[MemberData(nameof(TheoryDataMembers), "double?")]
	[MemberData(nameof(TheoryDataMembers), "decimal")]
	[MemberData(nameof(TheoryDataMembers), "decimal?")]
	[MemberData(nameof(TheoryDataMembers), "bool")]
	[MemberData(nameof(TheoryDataMembers), "bool?")]
	[MemberData(nameof(TheoryDataMembers), "DateTime")]
	[MemberData(nameof(TheoryDataMembers), "DateTime?")]
	[MemberData(nameof(TheoryDataMembers), "DateTimeOffset")]
	[MemberData(nameof(TheoryDataMembers), "DateTimeOffset?")]
	[MemberData(nameof(TheoryDataMembers), "TimeSpan")]
	[MemberData(nameof(TheoryDataMembers), "TimeSpan?")]
	[MemberData(nameof(TheoryDataMembers), "BigInteger")]
	[MemberData(nameof(TheoryDataMembers), "BigInteger?")]
	[MemberData(nameof(TheoryDataMembers), "Type")]
	[MemberData(nameof(TheoryDataMembers), "Enum")]
	[MemberData(nameof(TheoryDataMembers), "SerializableEnumeration")]
	[MemberData(nameof(TheoryDataMembers), "SerializableEnumeration?")]
	[MemberData(nameof(TheoryDataMembers), "Dictionary<string, List<string>>")]

#if NET6_0_OR_GREATER && ROSLYN_4_4_OR_GREATER

	[MemberData(nameof(TheoryDataMembers), "DateOnly")]
	[MemberData(nameof(TheoryDataMembers), "DateOnly[]")]
	[MemberData(nameof(TheoryDataMembers), "DateOnly?")]
	[MemberData(nameof(TheoryDataMembers), "DateOnly?[]")]
	[MemberData(nameof(TheoryDataMembers), "TimeOnly")]
	[MemberData(nameof(TheoryDataMembers), "TimeOnly[]")]
	[MemberData(nameof(TheoryDataMembers), "TimeOnly?")]
	[MemberData(nameof(TheoryDataMembers), "TimeOnly?[]")]

#endif

	public async void GivenTheoryMethod_WithSerializableTheoryDataMember_DoesNotFindDiagnostic(
		string member,
		string attribute,
		string type)
	{
		var source = $@"
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

public class TestClass {{
    {member}

    [Theory]
    [{attribute}]
    public void TestMethod({type} parameter) {{ }}
}}

public enum SerializableEnumeration {{ Zero }}";

		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(TheoryDataMembers), "IXunitSerializable")]
	[MemberData(nameof(TheoryDataMembers), "IXunitSerializable[]")]
	[MemberData(nameof(TheoryDataMembers), "ISerializableInterface")]
	[MemberData(nameof(TheoryDataMembers), "ISerializableInterface[]")]
	[MemberData(nameof(TheoryDataMembers), "SerializableClass")]
	[MemberData(nameof(TheoryDataMembers), "SerializableClass[]")]
	[MemberData(nameof(TheoryDataMembers), "SerializableStruct")]
	[MemberData(nameof(TheoryDataMembers), "SerializableStruct[]")]
	[MemberData(nameof(TheoryDataMembers), "SerializableStruct?")]
	[MemberData(nameof(TheoryDataMembers), "SerializableStruct?[]")]
	public async void GivenTheoryMethod_WithIXunitSerializableTheoryDataMember_DoesNotFindDiagnostic(
		string member,
		string attribute,
		string type)
	{
		var sourceV2 = GetSource(iXunitSerializableNamespace: "Xunit.Abstractions");
		var sourceV3 = GetSource(iXunitSerializableNamespace: "Xunit.Sdk");

		await Verify.VerifyAnalyzerV2(sourceV2);
		await Verify.VerifyAnalyzerV3(sourceV3);

		string GetSource(string iXunitSerializableNamespace)
		{
			return $@"
using Xunit;
using {iXunitSerializableNamespace};

public class TestClass {{
    {member}

    [Theory]
    [{attribute}]
    public void TestMethod({type} parameter) {{ }}
}}

public interface ISerializableInterface : IXunitSerializable {{ }}

public class SerializableClass : ISerializableInterface {{
    public void Deserialize(IXunitSerializationInfo info) {{ }}
    public void Serialize(IXunitSerializationInfo info) {{ }}
}}

public struct SerializableStruct : ISerializableInterface {{
    public void Deserialize(IXunitSerializationInfo info) {{ }}
    public void Serialize(IXunitSerializationInfo info) {{ }}
}}";
		}
	}

	[Theory]
	[MemberData(nameof(TheoryDataMembers), "object")]
	[MemberData(nameof(TheoryDataMembers), "object[]")]
	[MemberData(nameof(TheoryDataMembers), "Array")]
	[MemberData(nameof(TheoryDataMembers), "Array[]")]
	[MemberData(nameof(TheoryDataMembers), "ValueType")]
	[MemberData(nameof(TheoryDataMembers), "ValueType[]")]
	[MemberData(nameof(TheoryDataMembers), "IEnumerable")]
	[MemberData(nameof(TheoryDataMembers), "IEnumerable[]")]
	[MemberData(nameof(TheoryDataMembers), "IEnumerable<int>")]
	[MemberData(nameof(TheoryDataMembers), "IEnumerable<int>[]")]
	[MemberData(nameof(TheoryDataMembers), "Dictionary<int, string>")]
	[MemberData(nameof(TheoryDataMembers), "Dictionary<int, string>[]")]
	[MemberData(nameof(TheoryDataMembers), "IPossiblySerializableInterface")]
	[MemberData(nameof(TheoryDataMembers), "IPossiblySerializableInterface[]")]
	[MemberData(nameof(TheoryDataMembers), "PossiblySerializableUnsealedClass")]
	[MemberData(nameof(TheoryDataMembers), "PossiblySerializableUnsealedClass[]")]
	public async void GivenTheoryMethod_WithPossiblySerializableTheoryDataMember_FindsWeakDiagnostic(
		string member,
		string attribute,
		string type)
	{
		var source = $@"
using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

public class TestClass {{
    {member}

    [Theory]
    [{attribute}]
    public void TestMethod({type} parameter) {{ }}
}}

public interface IPossiblySerializableInterface {{ }}

public class PossiblySerializableUnsealedClass {{ }}";

		var expected =
			Verify
				.Diagnostic()
				.WithSpan(11, 6, 11, 6 + attribute.Length)
				.WithArguments(type, MightNotBeSerializable);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(TheoryDataMembers), "Delegate")]
	[MemberData(nameof(TheoryDataMembers), "Delegate[]")]
	[MemberData(nameof(TheoryDataMembers), "Func<int>")]
	[MemberData(nameof(TheoryDataMembers), "Func<int>[]")]
	[MemberData(nameof(TheoryDataMembers), "NonSerializableSealedClass")]
	[MemberData(nameof(TheoryDataMembers), "NonSerializableSealedClass[]")]
	[MemberData(nameof(TheoryDataMembers), "NonSerializableStruct")]
	[MemberData(nameof(TheoryDataMembers), "NonSerializableStruct[]")]
	[MemberData(nameof(TheoryDataMembers), "NonSerializableStruct?")]
	[MemberData(nameof(TheoryDataMembers), "NonSerializableStruct?[]")]
	public async void GivenTheoryMethod_WithNonSerializableTheoryDataMember_FindsStrongDiagnostic(
		string member,
		string attribute,
		string type)
	{
		var source = $@"
using System;
using System.Text;
using Xunit;

public class TestClass {{
    {member}

    [Theory]
    [{attribute}]
    public void TestMethod({type} parameter) {{ }}
}}

public sealed class NonSerializableSealedClass {{ }}

public struct NonSerializableStruct {{ }}";

		var expected =
			Verify
				.Diagnostic()
				.WithSpan(10, 6, 10, 6 + attribute.Length)
				.WithArguments(type, IsNotSerializable);

		await Verify.VerifyAnalyzer(source, expected);
	}

	[Theory]
	[MemberData(nameof(TheoryDataClass), "int", "double", "string")]
	public async void GivenTheoryMethod_WithSerializableTheoryDataClass_DoesNotFindDiagnostic(
		string source,
		string _1,
		string _2,
		string _3)
	{
		await Verify.VerifyAnalyzer(source);
	}

	[Theory]
	[MemberData(nameof(TheoryDataClass), "object[]", "Array", "IDisposable")]
	public async void GivenTheoryMethod_WithPossiblySerializableTheoryDataClass_FindsWeakDiagnostic(
		string source,
		string type1,
		string type2,
		string type3)
	{
		var expectedForType1 =
			Verify
				.Diagnostic()
				.WithSpan(7, 6, 7, 37)
				.WithArguments(type1, MightNotBeSerializable);

		var expectedForType2 =
			Verify
				.Diagnostic()
				.WithSpan(7, 6, 7, 37)
				.WithArguments(type2, MightNotBeSerializable);

		var expectedForType3 =
			Verify
				.Diagnostic()
				.WithSpan(7, 6, 7, 37)
				.WithArguments(type3, MightNotBeSerializable);

		await Verify.VerifyAnalyzer(
			source,
			expectedForType1,
			expectedForType2,
			expectedForType3
		);
	}

	[Theory]
	[MemberData(nameof(TheoryDataClass), "Action", "TimeZoneInfo", "TimeZoneInfo.TransitionTime")]
	public async void GivenTheoryMethod_WithNonSerializableTheoryDataClass_FindsStrongDiagnostic(
		string source,
		string type1,
		string type2,
		string type3)
	{
		var expectedForType1 =
			Verify
				.Diagnostic()
				.WithSpan(7, 6, 7, 37)
				.WithArguments(type1, IsNotSerializable);

		var expectedForType2 =
			Verify
				.Diagnostic()
				.WithSpan(7, 6, 7, 37)
				.WithArguments(type2, IsNotSerializable);

		var expectedForType3 =
			Verify
				.Diagnostic()
				.WithSpan(7, 6, 7, 37)
				.WithArguments(type3, IsNotSerializable);

		await Verify.VerifyAnalyzer(
			source,
			expectedForType1,
			expectedForType2,
			expectedForType3
		);
	}
}
