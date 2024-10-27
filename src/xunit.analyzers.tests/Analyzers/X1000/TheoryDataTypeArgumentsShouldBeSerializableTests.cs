using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
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
		var source = string.Format(/* lang=c#-test */ """
			using System;
			using Xunit;

			public class TestClass {{
			    [{0}]
			    [ClassData(typeof(DerivedClass))]
			    public void TestMethod({1} a, {2} b, {3} c) {{ }}
			}}

			public class BaseClass<T1, T2, T3, T4> : TheoryData<T1, T2, {3}> {{ }}

			public class DerivedClass : BaseClass<{1}, {2}, object, Delegate> {{ }}
			""", theoryAttribute, type1, type2, type3);

		return new TheoryData<string, string, string, string>
		{
			{ source, type1, type2, type3 }
		};
	}

	public static TheoryData<string, string, string> TheoryDataMembers(string type)
	{
		var c = string.Format(/* lang=c#-test */ "public sealed class Class : TheoryData<{0}> {{ }}", type);
		var f = string.Format(/* lang=c#-test */ "public static readonly TheoryData<{0}> Field = new TheoryData<{0}>() {{ }};", type);
		var m = string.Format(/* lang=c#-test */ "public static TheoryData<{0}> Method(int a, string b) => new TheoryData<{0}>() {{ }};", type);
		var p = string.Format(/* lang=c#-test */ "public static TheoryData<{0}> Property => new TheoryData<{0}>() {{ }};", type);

		return new TheoryData<string, string, string>
		{
			{ c, "ClassData(typeof(Class))", type },
			{ f, "MemberData(nameof(Field))", type },
			{ m, @"MemberData(nameof(Method), 1, ""2"")", type },
			{ p, "MemberData(nameof(Property))", type }
		};
	}

	public static TheoryData<string, string, string> TheoryDataMembersWithDiscoveryEnumerationDisabled(string type)
	{
		var f = string.Format(/* lang=c#-test */ "public static readonly TheoryData<{0}> Field = new TheoryData<{0}>() {{ }};", type);
		var m = string.Format(/* lang=c#-test */ "public static TheoryData<{0}> Method(int a, string b) => new TheoryData<{0}>() {{ }};", type);
		var p = string.Format(/* lang=c#-test */ "public static TheoryData<{0}> Property => new TheoryData<{0}>() {{ }};", type);

		return new TheoryData<string, string, string>
		{
			{ f, "MemberData(nameof(Field), DisableDiscoveryEnumeration = true)", type },
			{ m, @"MemberData(nameof(Method), 1, ""2"", DisableDiscoveryEnumeration = true)", type },
			{ p, "MemberData(nameof(Property), DisableDiscoveryEnumeration = true)", type }
		};
	}

	public sealed class NoDiagnostic : TheoryDataTypeArgumentsShouldBeSerializableTests
	{
		[Fact]
		public async Task GivenMethodWithoutAttributes_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    public void TestMethod() { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task GivenFact_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
				public class TestClass {
				    [Xunit.Fact]
				    public void TestMethod() { }
				}
				""";

			await Verify.VerifyAnalyzer(source);
		}

		[Fact]
		public async Task GivenTheory_WithoutTheoryDataAsDataSource_DoesNotTrigger()
		{
			var source = /* lang=c#-test */ """
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
				}
				""";

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
#if NET6_0_OR_GREATER
		[MemberData(nameof(TheoryDataMembers), "DateOnly")]
		[MemberData(nameof(TheoryDataMembers), "DateOnly[]")]
		[MemberData(nameof(TheoryDataMembers), "DateOnly?")]
		[MemberData(nameof(TheoryDataMembers), "DateOnly?[]")]
		[MemberData(nameof(TheoryDataMembers), "TimeOnly")]
		[MemberData(nameof(TheoryDataMembers), "TimeOnly[]")]
		[MemberData(nameof(TheoryDataMembers), "TimeOnly?")]
		[MemberData(nameof(TheoryDataMembers), "TimeOnly?[]")]
#endif
		public async Task GivenTheory_WithSerializableTheoryDataMember_DoesNotTrigger(
			string member,
			string attribute,
			string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System;
				using System.Collections.Generic;
				using System.Numerics;
				using Xunit;

				public class TestClass {{
				    {0}

				    [Theory]
				    [{1}]
				    public void TestMethod({2} parameter) {{ }}
				}}

				public enum SerializableEnumeration {{ Zero }}
				""", member, attribute, type);

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
		public async Task GivenTheory_WithIXunitSerializableTheoryDataMember_DoesNotTrigger(
			string member,
			string attribute,
			string type)
		{
			var sourceV2 = GetSource("Xunit.Abstractions");
			var sourceV3 = GetSource("Xunit.Sdk");

			await Verify.VerifyAnalyzerV2(sourceV2);
			await Verify.VerifyAnalyzerV3(sourceV3);

			string GetSource(string ns) =>
				string.Format(/* lang=c#-test */ """
					using Xunit;
					using {3};

					public class TestClass {{
					    {0}

					    [Theory]
					    [{1}]
					    public void TestMethod({2} parameter) {{ }}
					}}

					public interface ISerializableInterface : IXunitSerializable {{ }}

					public class SerializableClass : ISerializableInterface {{
					    public void Deserialize(IXunitSerializationInfo info) {{ }}
					    public void Serialize(IXunitSerializationInfo info) {{ }}
					}}

					public struct SerializableStruct : ISerializableInterface {{
					    public void Deserialize(IXunitSerializationInfo info) {{ }}
					    public void Serialize(IXunitSerializationInfo info) {{ }}
					}}
					""", member, attribute, type, ns);
		}

		[Theory]
		[MemberData(nameof(TheoryDataMembers), "ICustomSerialized")]
		[MemberData(nameof(TheoryDataMembers), "CustomSerialized")]
		[MemberData(nameof(TheoryDataMembers), "CustomSerializedDerived")]
		public async Task GivenTheory_WithIXunitSerializerTheoryDataMember_DoesNotTrigger(
			string member,
			string attribute,
			string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System;
				using Xunit;
				using Xunit.Sdk;

				[assembly: RegisterXunitSerializer(typeof(CustomSerializer), typeof(ICustomSerialized))]

				public class TestClass {{
				    {0}

				    [Theory]
				    [{1}]
				    public void TestMethod({2} parameter) {{ }}
				}}

				public interface ICustomSerialized {{ }}

				public class CustomSerialized : ICustomSerialized {{ }}

				public class CustomSerializedDerived : CustomSerialized {{ }}

				public class CustomSerializer : IXunitSerializer {{
				    public object Deserialize(Type type, string serializedValue) =>
				        throw new NotImplementedException();

				    public bool IsSerializable(Type type, object? value) =>
				        true;

				    public string Serialize(object value) =>
				        throw new NotImplementedException();
				}}
				""", member, attribute, type
			);

			await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
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
		public async Task GivenTheory_WithNonSerializableTheoryDataMember_WithDiscoveryEnumerationDisabledForTheory_DoesNotTrigger(
			string member,
			string attribute,
			string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System;
				using Xunit;

				public class TestClass {{
				    {0}

				    [Theory(DisableDiscoveryEnumeration = true)]
				    [{1}]
				    public void TestMethod({2} parameter) {{ }}
				}}

				public sealed class NonSerializableSealedClass {{ }}

				public struct NonSerializableStruct {{ }}

				public interface IPossiblySerializableInterface {{ }}

				public class PossiblySerializableUnsealedClass {{ }}
				""", member, attribute, type);

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
		public async Task GivenTheory_WithNonSerializableTheoryDataMember_WithDiscoveryEnumerationDisabledForMemberData_DoesNotTrigger(
			string member,
			string attribute,
			string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System;
				using Xunit;

				public class TestClass {{
				    {0}

				    [Theory]
				    [{1}]
				    public void TestMethod({2} parameter) {{ }}
				}}

				public sealed class NonSerializableSealedClass {{ }}

				public struct NonSerializableStruct {{ }}

				public interface IPossiblySerializableInterface {{ }}

				public class PossiblySerializableUnsealedClass {{ }}
				""", member, attribute, type);

			await Verify.VerifyAnalyzer(source);
		}

		[Theory]
		[MemberData(nameof(TheoryDataClass), "int", "double", "string")]
		public async Task GivenTheory_WithSerializableTheoryDataClass_DoesNotTrigger(
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
		public async Task GivenTheory_WithNonSerializableTheoryDataClass_WithDiscoveryEnumerationDisabled_DoesNotTrigger(
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
		public async Task GivenTheory_WithNonSerializableTheoryDataMember_Triggers(
			string member,
			string attribute,
			string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System;
				using System.Text;
				using Xunit;

				public class TestClass {{
				    {0}

				    [Theory]
				    [{{|#0:{1}|}}]
				    public void TestMethod({2} parameter) {{ }}
				}}

				public sealed class NonSerializableSealedClass {{ }}

				public struct NonSerializableStruct {{ }}
				""", member, attribute, type);
			var expected = Verify.Diagnostic("xUnit1044").WithLocation(0).WithArguments(type);

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(TheoryDataClass), "Action", "TimeZoneInfo", "TimeZoneInfo.TransitionTime")]
		public async Task GivenTheory_WithNonSerializableTheoryDataClass_Triggers(
			string source,
			string type1,
			string type2,
			string type3)
		{
			var expected = new[] {
				Verify.Diagnostic("xUnit1044").WithSpan(6, 6, 6, 37).WithArguments(type1),
				Verify.Diagnostic("xUnit1044").WithSpan(6, 6, 6, 37).WithArguments(type2),
				Verify.Diagnostic("xUnit1044").WithSpan(6, 6, 6, 37).WithArguments(type3),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}
	}

	public sealed class X1045_AvoidUsingTheoryDataTypeArgumentsThatMightNotBeSerializable : TheoryDataTypeArgumentsShouldBeSerializableTests
	{
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
		public async Task GivenTheory_WithPossiblySerializableTheoryDataMember_Triggers(
			string member,
			string attribute,
			string type)
		{
			var source = string.Format(/* lang=c#-test */ """
				using System;
				using System.Collections;
				using System.Collections.Generic;
				using Xunit;

				public class TestClass {{
				    {0}

				    [Theory]
				    [{{|#0:{1}|}}]
				    public void TestMethod({2} parameter) {{ }}
				}}

				public interface IPossiblySerializableInterface {{ }}

				public class PossiblySerializableUnsealedClass {{ }}
				""", member, attribute, type);
			var expected = Verify.Diagnostic("xUnit1045").WithLocation(0).WithArguments(type);

			await Verify.VerifyAnalyzer(source, expected);
		}

		[Theory]
		[MemberData(nameof(TheoryDataClass), "object[]", "Array", "IDisposable")]
		public async Task GivenTheory_WithPossiblySerializableTheoryDataClass_Triggers(
			string source,
			string type1,
			string type2,
			string type3)
		{
			var expected = new[] {
				Verify.Diagnostic("xUnit1045").WithSpan(6, 6, 6, 37).WithArguments(type1),
				Verify.Diagnostic("xUnit1045").WithSpan(6, 6, 6, 37).WithArguments(type2),
				Verify.Diagnostic("xUnit1045").WithSpan(6, 6, 6, 37).WithArguments(type3),
			};

			await Verify.VerifyAnalyzer(source, expected);
		}
	}
}
