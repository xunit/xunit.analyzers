using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataTypeArgumentsShouldBeSerializable>;

public class TheoryDataTypeArgumentsShouldBeSerializableTests
{
	public static TheoryData<string, string, string, string> TheoryDataClass(
		string type1,
		string type2,
		string type3) =>
			TheoryDataClass(theoryAttribute: "Theory", type1, type2, type3);

	public static TheoryData<string, string, string, string> TheoryDataClass(
		string theoryAttribute,
		string type1,
		string type2,
		string type3)
	{
		var source = $@"
using System;
using Xunit;

public class TestClass {{
    [{theoryAttribute}]
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

	public static TheoryData<string, string, string> TheoryDataMembersWithDiscoveryEnumerationDisabled(string type)
	{
		var field = $@"public static readonly TheoryData<{type}> Field = new TheoryData<{type}>() {{ }};";
		var method = $@"public static TheoryData<{type}> Method(int a, string b) => new TheoryData<{type}>() {{ }};";
		var property = $@"public static TheoryData<{type}> Property => new TheoryData<{type}>() {{ }};";

		return new TheoryData<string, string, string>
		{
			{ field, "MemberData(nameof(Field), DisableDiscoveryEnumeration = true)", type },
			{ method, @"MemberData(nameof(Method), 1, ""2"", DisableDiscoveryEnumeration = true)", type },
			{ property, "MemberData(nameof(Property), DisableDiscoveryEnumeration = true)", type }
		};
	}

	public sealed class NoDiagnostic : TheoryDataTypeArgumentsShouldBeSerializableTests
	{
		[Fact]
		public async void GivenMethodWithoutAttributes_FindsNoDiagnostic()
		{
			var source = @"
public class TestClass {
    public void TestMethod() { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void GivenFact_FindsNoDiagnostic()
		{
			var source = @"
public class TestClass {
    [Xunit.Fact]
    public void TestMethod() { }
}";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async void GivenTheory_WithoutTheoryDataAsDataSource_FindsNoDiagnostic()
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

		public async void GivenTheory_WithSerializableTheoryDataMember_FindsNoDiagnostic(
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
		public async void GivenTheory_WithIXunitSerializableTheoryDataMember_FindsNoDiagnostic(
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
		[MemberData(nameof(TheoryDataMembers), "Delegate")]
		[MemberData(nameof(TheoryDataMembers), "Delegate[]")]
		[MemberData(nameof(TheoryDataMembers), "NonSerializableSealedClass")]
		[MemberData(nameof(TheoryDataMembers), "NonSerializableStruct")]
		[MemberData(nameof(TheoryDataMembers), "object")]
		[MemberData(nameof(TheoryDataMembers), "object[]")]
		[MemberData(nameof(TheoryDataMembers), "IPossiblySerializableInterface")]
		[MemberData(nameof(TheoryDataMembers), "PossiblySerializableUnsealedClass")]
		public async void GivenTheory_WithNonSerializableTheoryDataMember_WithDiscoveryEnumerationDisabledForTheory_FindsNoDiagnostic(
			string member,
			string attribute,
			string type)
		{
			var source = $@"
using System;
using Xunit;

public class TestClass {{
    {member}

    [Theory(DisableDiscoveryEnumeration = true)]
    [{attribute}]
    public void TestMethod({type} parameter) {{ }}
}}

public sealed class NonSerializableSealedClass {{ }}

public struct NonSerializableStruct {{ }}

public interface IPossiblySerializableInterface {{ }}

public class PossiblySerializableUnsealedClass {{ }}";

			await Verify.VerifyAnalyzerV3(source);
		}

		[Theory]
		[MemberData(nameof(TheoryDataMembersWithDiscoveryEnumerationDisabled), "Delegate")]
		[MemberData(nameof(TheoryDataMembersWithDiscoveryEnumerationDisabled), "Delegate[]")]
		[MemberData(nameof(TheoryDataMembersWithDiscoveryEnumerationDisabled), "NonSerializableSealedClass")]
		[MemberData(nameof(TheoryDataMembersWithDiscoveryEnumerationDisabled), "NonSerializableStruct")]
		[MemberData(nameof(TheoryDataMembersWithDiscoveryEnumerationDisabled), "object")]
		[MemberData(nameof(TheoryDataMembersWithDiscoveryEnumerationDisabled), "object[]")]
		[MemberData(nameof(TheoryDataMembersWithDiscoveryEnumerationDisabled), "IPossiblySerializableInterface")]
		[MemberData(nameof(TheoryDataMembersWithDiscoveryEnumerationDisabled), "PossiblySerializableUnsealedClass")]
		public async void GivenTheory_WithNonSerializableTheoryDataMember_WithDiscoveryEnumerationDisabledForMemberData_FindsNoDiagnostic(
			string member,
			string attribute,
			string type)
		{
			var source = $@"
using System;
using Xunit;

public class TestClass {{
    {member}

    [Theory]
    [{attribute}]
    public void TestMethod({type} parameter) {{ }}
}}

public sealed class NonSerializableSealedClass {{ }}

public struct NonSerializableStruct {{ }}

public interface IPossiblySerializableInterface {{ }}

public class PossiblySerializableUnsealedClass {{ }}";

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(TheoryDataClass), "int", "double", "string")]
		public async void GivenTheory_WithSerializableTheoryDataClass_FindsNoDiagnostic(
			string source,
			string _1,
			string _2,
			string _3)
		{
			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(TheoryDataClass), "Theory(DisableDiscoveryEnumeration = true)", "Action", "TimeZoneInfo", "TimeZoneInfo.TransitionTime")]
		[MemberData(nameof(TheoryDataClass), "Theory(DisableDiscoveryEnumeration = true)", "object[]", "Array", "IDisposable")]
		public async void GivenTheory_WithNonSerializableTheoryDataClass_WithDiscoveryEnumerationDisabled_FindsNoDiagnostic(
			string source,
			string _1,
			string _2,
			string _3)
		{
			await Verify.VerifyAnalyzerV3(source);
		}
	}

	public sealed class X1044_AvoidUsingTheoryDataTypeArgumentsThatAreNotSerializable : TheoryDataTypeArgumentsShouldBeSerializableTests
	{
		const string Id = "xUnit1044";

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
		public async void GivenTheory_WithNonSerializableTheoryDataMember_FindsDiagnostic(
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
					.Diagnostic(Id)
					.WithSpan(10, 6, 10, 6 + attribute.Length)
					.WithArguments(type);

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(TheoryDataClass), "Action", "TimeZoneInfo", "TimeZoneInfo.TransitionTime")]
		public async void GivenTheory_WithNonSerializableTheoryDataClass_FindsDiagnostic(
			string source,
			string type1,
			string type2,
			string type3)
		{
			var expectedForType1 =
				Verify
					.Diagnostic(Id)
					.WithSpan(7, 6, 7, 37)
					.WithArguments(type1);

			var expectedForType2 =
				Verify
					.Diagnostic(Id)
					.WithSpan(7, 6, 7, 37)
					.WithArguments(type2);

			var expectedForType3 =
				Verify
					.Diagnostic(Id)
					.WithSpan(7, 6, 7, 37)
					.WithArguments(type3);

			await Verify.VerifyAnalyzer(
				source,
				expectedForType1,
				expectedForType2,
				expectedForType3
			);
		}
	}

	public sealed class X1045_AvoidUsingTheoryDataTypeArgumentsThatMightNotBeSerializable : TheoryDataTypeArgumentsShouldBeSerializableTests
	{
		const string Id = "xUnit1045";

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
		public async void GivenTheory_WithPossiblySerializableTheoryDataMember_FindsDiagnostic(
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
					.Diagnostic(Id)
					.WithSpan(11, 6, 11, 6 + attribute.Length)
					.WithArguments(type);

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(TheoryDataClass), "object[]", "Array", "IDisposable")]
		public async void GivenTheory_WithPossiblySerializableTheoryDataClass_FindsDiagnostic(
			string source,
			string type1,
			string type2,
			string type3)
		{
			var expectedForType1 =
				Verify
					.Diagnostic(Id)
					.WithSpan(7, 6, 7, 37)
					.WithArguments(type1);

			var expectedForType2 =
				Verify
					.Diagnostic(Id)
					.WithSpan(7, 6, 7, 37)
					.WithArguments(type2);

			var expectedForType3 =
				Verify
					.Diagnostic(Id)
					.WithSpan(7, 6, 7, 37)
					.WithArguments(type3);

			await Verify.VerifyAnalyzer(
				source,
				expectedForType1,
				expectedForType2,
				expectedForType3
			);
		}
	}
}
