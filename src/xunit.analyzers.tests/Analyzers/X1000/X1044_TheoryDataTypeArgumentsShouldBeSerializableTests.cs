using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataTypeArgumentsShouldBeSerializable>;
using Verify_v3_Pre301 = CSharpVerifier<X1044_TheoryDataTypeArgumentsShouldBeSerializableTests.Analyzer_v3_Pre301>;

public class X1044_TheoryDataTypeArgumentsShouldBeSerializableTests
{
	[Fact]
	public async ValueTask V2_and_V3_NonAOT()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using System;
			using System.Collections;
			using System.Collections.Generic;
			using System.Numerics;
			using Xunit;
			using Xunit.Sdk;

			public class NonTestClass {
				public void NonTestMethod_DoesNotTrigger() { }
			}

			public class TestClass {
				[Fact]
				public void FactMethod_DoesNotTrigger() { }

				public static IEnumerable<object[]> Property1 => Array.Empty<object[]>();
				public static DataSource<IDisposable, Action> Property2 => new DataSource<IDisposable, Action>();

				[Theory]
				[MemberData(nameof(Property1))]
				[MemberData(nameof(Property2))]
				public void TheoryMethod_WithoutTheoryDataAsDataSource_DoesNotTrigger(object a, object b) { }

				public class DataSource<T1, T2> : IEnumerable<object[]> {
					public IEnumerator<object[]> GetEnumerator() { yield break; }
					IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
				}
			}

			// Serializable via XunitSerializationInfo (v2) or SerializationHelper (v3)

			public class TypeClass {
				public sealed class Class : TheoryData<Type> { }
				public static readonly TheoryData<Type> Field = new TheoryData<Type>() { };
				public static TheoryData<Type> Method(int a, string b) => new TheoryData<Type>() { };
				public static TheoryData<Type> Property => new TheoryData<Type>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(Type parameter) { }
			}

			public class TraitsClass {
				public sealed class Class : TheoryData<Dictionary<string, List<string>>> { }
				public static readonly TheoryData<Dictionary<string, List<string>>> Field = new TheoryData<Dictionary<string, List<string>>>() { };
				public static TheoryData<Dictionary<string, List<string>>> Method(int a, string b) => new TheoryData<Dictionary<string, List<string>>>() { };
				public static TheoryData<Dictionary<string, List<string>>> Property => new TheoryData<Dictionary<string, List<string>>>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(Dictionary<string, List<string>> parameter) { }
			}

			public class StringClass {
				public sealed class Class : TheoryData<string> { }
				public static readonly TheoryData<string> Field = new TheoryData<string>() { };
				public static TheoryData<string> Method(int a, string b) => new TheoryData<string>() { };
				public static TheoryData<string> Property => new TheoryData<string>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(string parameter) { }
			}

			public class NullableStringClass {
				public sealed class Class : TheoryData<string?> { }
				public static readonly TheoryData<string?> Field = new TheoryData<string?>() { };
				public static TheoryData<string?> Method(int a, string b) => new TheoryData<string?>() { };
				public static TheoryData<string?> Property => new TheoryData<string?>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(string? parameter) { }
			}

			public class StringArrayClass {
				public sealed class Class : TheoryData<string[]> { }
				public static readonly TheoryData<string[]> Field = new TheoryData<string[]>() { };
				public static TheoryData<string[]> Method(int a, string b) => new TheoryData<string[]>() { };
				public static TheoryData<string[]> Property => new TheoryData<string[]>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(string[] parameter) { }
			}

			public class StringArrayArrayClass {
				public sealed class Class : TheoryData<string[][]> { }
				public static readonly TheoryData<string[][]> Field = new TheoryData<string[][]>() { };
				public static TheoryData<string[][]> Method(int a, string b) => new TheoryData<string[][]>() { };
				public static TheoryData<string[][]> Property => new TheoryData<string[][]>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(string[][] parameter) { }
			}

			public class CharClass {
				public sealed class Class : TheoryData<char> { }
				public static readonly TheoryData<char> Field = new TheoryData<char>() { };
				public static TheoryData<char> Method(int a, string b) => new TheoryData<char>() { };
				public static TheoryData<char> Property => new TheoryData<char>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(char parameter) { }
			}

			public class NullableCharClass {
				public sealed class Class : TheoryData<char?> { }
				public static readonly TheoryData<char?> Field = new TheoryData<char?>() { };
				public static TheoryData<char?> Method(int a, string b) => new TheoryData<char?>() { };
				public static TheoryData<char?> Property => new TheoryData<char?>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(char? parameter) { }
			}

			public class ByteClass {
				public sealed class Class : TheoryData<byte> { }
				public static readonly TheoryData<byte> Field = new TheoryData<byte>() { };
				public static TheoryData<byte> Method(int a, string b) => new TheoryData<byte>() { };
				public static TheoryData<byte> Property => new TheoryData<byte>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(byte parameter) { }
			}

			public class SByteClass {
				public sealed class Class : TheoryData<sbyte> { }
				public static readonly TheoryData<sbyte> Field = new TheoryData<sbyte>() { };
				public static TheoryData<sbyte> Method(int a, string b) => new TheoryData<sbyte>() { };
				public static TheoryData<sbyte> Property => new TheoryData<sbyte>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(sbyte parameter) { }
			}

			public class Int16Class {
				public sealed class Class : TheoryData<short> { }
				public static readonly TheoryData<short> Field = new TheoryData<short>() { };
				public static TheoryData<short> Method(int a, string b) => new TheoryData<short>() { };
				public static TheoryData<short> Property => new TheoryData<short>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(short parameter) { }
			}

			public class UInt16Class {
				public sealed class Class : TheoryData<ushort> { }
				public static readonly TheoryData<ushort> Field = new TheoryData<ushort>() { };
				public static TheoryData<ushort> Method(int a, string b) => new TheoryData<ushort>() { };
				public static TheoryData<ushort> Property => new TheoryData<ushort>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(ushort parameter) { }
			}

			public class Int32Class {
				public sealed class Class : TheoryData<int> { }
				public static readonly TheoryData<int> Field = new TheoryData<int>() { };
				public static TheoryData<int> Method(int a, string b) => new TheoryData<int>() { };
				public static TheoryData<int> Property => new TheoryData<int>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(int parameter) { }
			}

			public class UInt32Class {
				public sealed class Class : TheoryData<uint> { }
				public static readonly TheoryData<uint> Field = new TheoryData<uint>() { };
				public static TheoryData<uint> Method(int a, string b) => new TheoryData<uint>() { };
				public static TheoryData<uint> Property => new TheoryData<uint>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(uint parameter) { }
			}

			public class Int64Class {
				public sealed class Class : TheoryData<long> { }
				public static readonly TheoryData<long> Field = new TheoryData<long>() { };
				public static TheoryData<long> Method(int a, string b) => new TheoryData<long>() { };
				public static TheoryData<long> Property => new TheoryData<long>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(long parameter) { }
			}

			public class UInt64Class {
				public sealed class Class : TheoryData<ulong> { }
				public static readonly TheoryData<ulong> Field = new TheoryData<ulong>() { };
				public static TheoryData<ulong> Method(int a, string b) => new TheoryData<ulong>() { };
				public static TheoryData<ulong> Property => new TheoryData<ulong>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(ulong parameter) { }
			}

			public class SingleClass {
				public sealed class Class : TheoryData<float> { }
				public static readonly TheoryData<float> Field = new TheoryData<float>() { };
				public static TheoryData<float> Method(int a, string b) => new TheoryData<float>() { };
				public static TheoryData<float> Property => new TheoryData<float>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(float parameter) { }
			}

			public class DoubleClass {
				public sealed class Class : TheoryData<double> { }
				public static readonly TheoryData<double> Field = new TheoryData<double>() { };
				public static TheoryData<double> Method(int a, string b) => new TheoryData<double>() { };
				public static TheoryData<double> Property => new TheoryData<double>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(double parameter) { }
			}

			public class DecimalClass {
				public sealed class Class : TheoryData<decimal> { }
				public static readonly TheoryData<decimal> Field = new TheoryData<decimal>() { };
				public static TheoryData<decimal> Method(int a, string b) => new TheoryData<decimal>() { };
				public static TheoryData<decimal> Property => new TheoryData<decimal>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(decimal parameter) { }
			}

			public class BooleanClass {
				public sealed class Class : TheoryData<bool> { }
				public static readonly TheoryData<bool> Field = new TheoryData<bool>() { };
				public static TheoryData<bool> Method(int a, string b) => new TheoryData<bool>() { };
				public static TheoryData<bool> Property => new TheoryData<bool>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(bool parameter) { }
			}

			public class DateTimeClass {
				public sealed class Class : TheoryData<DateTime> { }
				public static readonly TheoryData<DateTime> Field = new TheoryData<DateTime>() { };
				public static TheoryData<DateTime> Method(int a, string b) => new TheoryData<DateTime>() { };
				public static TheoryData<DateTime> Property => new TheoryData<DateTime>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(DateTime parameter) { }
			}

			public class DateTimeOffsetClass {
				public sealed class Class : TheoryData<DateTimeOffset> { }
				public static readonly TheoryData<DateTimeOffset> Field = new TheoryData<DateTimeOffset>() { };
				public static TheoryData<DateTimeOffset> Method(int a, string b) => new TheoryData<DateTimeOffset>() { };
				public static TheoryData<DateTimeOffset> Property => new TheoryData<DateTimeOffset>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(DateTimeOffset parameter) { }
			}

			public class TimeSpanClass {
				public sealed class Class : TheoryData<TimeSpan> { }
				public static readonly TheoryData<TimeSpan> Field = new TheoryData<TimeSpan>() { };
				public static TheoryData<TimeSpan> Method(int a, string b) => new TheoryData<TimeSpan>() { };
				public static TheoryData<TimeSpan> Property => new TheoryData<TimeSpan>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(TimeSpan parameter) { }
			}

			public class BigIntegerClass {
				public sealed class Class : TheoryData<BigInteger> { }
				public static readonly TheoryData<BigInteger> Field = new TheoryData<BigInteger>() { };
				public static TheoryData<BigInteger> Method(int a, string b) => new TheoryData<BigInteger>() { };
				public static TheoryData<BigInteger> Property => new TheoryData<BigInteger>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(BigInteger parameter) { }
			}

			public class EnumClass {
				public sealed class Class : TheoryData<Enum> { }
				public static readonly TheoryData<Enum> Field = new TheoryData<Enum>() { };
				public static TheoryData<Enum> Method(int a, string b) => new TheoryData<Enum>() { };
				public static TheoryData<Enum> Property => new TheoryData<Enum>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(Enum parameter) { }
			}

			public class SerializableEnumerationClass {
				public sealed class Class : TheoryData<SerializableEnumeration> { }
				public static readonly TheoryData<SerializableEnumeration> Field = new TheoryData<SerializableEnumeration>() { };
				public static TheoryData<SerializableEnumeration> Method(int a, string b) => new TheoryData<SerializableEnumeration>() { };
				public static TheoryData<SerializableEnumeration> Property => new TheoryData<SerializableEnumeration>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(SerializableEnumeration parameter) { }
			}

			public enum SerializableEnumeration { Zero }

			// Things which implement IXunitSerializable

			public class IXunitSerializableClass {
				public sealed class Class : TheoryData<IXunitSerializable> { }
				public static readonly TheoryData<IXunitSerializable> Field = new TheoryData<IXunitSerializable>() { };
				public static TheoryData<IXunitSerializable> Method(int a, string b) => new TheoryData<IXunitSerializable>() { };
				public static TheoryData<IXunitSerializable> Property => new TheoryData<IXunitSerializable>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(IXunitSerializable parameter) { }
			}

			public class ISerializableInterfaceClass {
				public sealed class Class : TheoryData<ISerializableInterface> { }
				public static readonly TheoryData<ISerializableInterface> Field = new TheoryData<ISerializableInterface>() { };
				public static TheoryData<ISerializableInterface> Method(int a, string b) => new TheoryData<ISerializableInterface>() { };
				public static TheoryData<ISerializableInterface> Property => new TheoryData<ISerializableInterface>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(ISerializableInterface parameter) { }
			}

			public class SerializableClassClass {
				public sealed class Class : TheoryData<SerializableClass> { }
				public static readonly TheoryData<SerializableClass> Field = new TheoryData<SerializableClass>() { };
				public static TheoryData<SerializableClass> Method(int a, string b) => new TheoryData<SerializableClass>() { };
				public static TheoryData<SerializableClass> Property => new TheoryData<SerializableClass>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(SerializableClass parameter) { }
			}

			public class SerializableStructClass {
				public sealed class Class : TheoryData<SerializableStruct> { }
				public static readonly TheoryData<SerializableStruct> Field = new TheoryData<SerializableStruct>() { };
				public static TheoryData<SerializableStruct> Method(int a, string b) => new TheoryData<SerializableStruct>() { };
				public static TheoryData<SerializableStruct> Property => new TheoryData<SerializableStruct>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(SerializableStruct parameter) { }
			}

			public interface ISerializableInterface : IXunitSerializable { }

			public class SerializableClass : ISerializableInterface {
				public void Deserialize(IXunitSerializationInfo info) { }
				public void Serialize(IXunitSerializationInfo info) { }
			}

			public struct SerializableStruct : ISerializableInterface {
				public void Deserialize(IXunitSerializationInfo info) { }
				public void Serialize(IXunitSerializationInfo info) { }
			}

			// Things which are definitely not serializable

			public class DelegateClass {
				public sealed class Class : TheoryData<Delegate> { }
				public static readonly TheoryData<Delegate> Field = new TheoryData<Delegate>() { };
				public static TheoryData<Delegate> Method(int a, string b) => new TheoryData<Delegate>() { };
				public static TheoryData<Delegate> Property => new TheoryData<Delegate>() { };

				[Theory]
				[MemberData(nameof(Field), DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Method), 1, "2", DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Property), DisableDiscoveryEnumeration = true)]
				public void DoesNotTrigger(Delegate parameter) { }

				[Theory]
				[{|xUnit1044:ClassData(typeof(Class))|}]
				[{|xUnit1044:MemberData(nameof(Field))|}]
				[{|xUnit1044:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1044:MemberData(nameof(Property))|}]
				public void Triggers(Delegate parameter) { }
			}

			public class NonSerializableSealedClassClass {
				public sealed class Class : TheoryData<NonSerializableSealedClass> { }
				public static readonly TheoryData<NonSerializableSealedClass> Field = new TheoryData<NonSerializableSealedClass>() { };
				public static TheoryData<NonSerializableSealedClass> Method(int a, string b) => new TheoryData<NonSerializableSealedClass>() { };
				public static TheoryData<NonSerializableSealedClass> Property => new TheoryData<NonSerializableSealedClass>() { };

				[Theory]
				[MemberData(nameof(Field), DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Method), 1, "2", DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Property), DisableDiscoveryEnumeration = true)]
				public void DoesNotTrigger(NonSerializableSealedClass parameter) { }

				[Theory]
				[{|xUnit1044:ClassData(typeof(Class))|}]
				[{|xUnit1044:MemberData(nameof(Field))|}]
				[{|xUnit1044:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1044:MemberData(nameof(Property))|}]
				public void Triggers(NonSerializableSealedClass parameter) { }
			}

			public class NonSerializableStructClass {
				public sealed class Class : TheoryData<NonSerializableStruct> { }
				public static readonly TheoryData<NonSerializableStruct> Field = new TheoryData<NonSerializableStruct>() { };
				public static TheoryData<NonSerializableStruct> Method(int a, string b) => new TheoryData<NonSerializableStruct>() { };
				public static TheoryData<NonSerializableStruct> Property => new TheoryData<NonSerializableStruct>() { };

				[Theory]
				[MemberData(nameof(Field), DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Method), 1, "2", DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Property), DisableDiscoveryEnumeration = true)]
				public void DoesNotTrigger(NonSerializableStruct parameter) { }

				[Theory]
				[{|xUnit1044:ClassData(typeof(Class))|}]
				[{|xUnit1044:MemberData(nameof(Field))|}]
				[{|xUnit1044:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1044:MemberData(nameof(Property))|}]
				public void Triggers(NonSerializableStruct parameter) { }
			}

			public sealed class NonSerializableSealedClass { }

			public struct NonSerializableStruct { }
			""";

		await Verify.VerifyAnalyzerV2(LanguageVersion.CSharp8, source.Replace("Xunit.Sdk", "Xunit.Abstractions"));
		await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp8, source);
	}

	[Fact]
	public async ValueTask V2_and_V3_NonAOT_PreTupleSupport()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			public class ValueTupleClass {
				public sealed class Class : TheoryData<ValueTuple<string, int>> { }
				public static readonly TheoryData<ValueTuple<string, int>> Field = new TheoryData<ValueTuple<string, int>>() { };
				public static TheoryData<ValueTuple<string, int>> Method(int a, string b) => new TheoryData<ValueTuple<string, int>>() { };
				public static TheoryData<ValueTuple<string, int>> Property => new TheoryData<ValueTuple<string, int>>() { };

				[Theory]
				[MemberData(nameof(Field), DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Method), 1, "2", DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Property), DisableDiscoveryEnumeration = true)]
				public void DoesNotTrigger(ValueTuple<string, int> parameter) { }

				[Theory]
				[{|xUnit1044:ClassData(typeof(Class))|}]
				[{|xUnit1044:MemberData(nameof(Field))|}]
				[{|xUnit1044:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1044:MemberData(nameof(Property))|}]
				public void Triggers(ValueTuple<string, int> parameter) { }
			}
			""";

		await Verify.VerifyAnalyzerV2(source);
		await Verify_v3_Pre301.VerifyAnalyzerV3NonAot(source);
	}

#if NET6_0_OR_GREATER

	[Fact]
	public async ValueTask V2_and_V3_NonAOT_NET60()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;

			// Serializable via XunitSerializationInfo (v2) or SerializationHelper (v3)

			public class DateOnlyClass {
				public sealed class Class : TheoryData<DateOnly> { }
				public static readonly TheoryData<DateOnly> Field = new TheoryData<DateOnly>() { };
				public static TheoryData<DateOnly> Method(int a, string b) => new TheoryData<DateOnly>() { };
				public static TheoryData<DateOnly> Property => new TheoryData<DateOnly>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(DateOnly parameter) { }
			}

			public class TimeOnlyClass {
				public sealed class Class : TheoryData<TimeOnly> { }
				public static readonly TheoryData<TimeOnly> Field = new TheoryData<TimeOnly>() { };
				public static TheoryData<TimeOnly> Method(int a, string b) => new TheoryData<TimeOnly>() { };
				public static TheoryData<TimeOnly> Property => new TheoryData<TimeOnly>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(TimeOnly parameter) { }
			}
			""";

		await Verify.VerifyAnalyzerNonAot(source);
	}

#endif  // NET6_0_OR_GREATER

	[Fact]
	public async ValueTask V2_only()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			// Things which are definitely not serializable in v2

			public class GuidClass {
				public sealed class Class : TheoryData<Guid> { }
				public static readonly TheoryData<Guid> Field = new TheoryData<Guid>() { };
				public static TheoryData<Guid> Method(int a, string b) => new TheoryData<Guid>() { };
				public static TheoryData<Guid> Property => new TheoryData<Guid>() { };

				[Theory]
				[MemberData(nameof(Field), DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Method), 1, "2", DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Property), DisableDiscoveryEnumeration = true)]
				public void DoesNotTrigger(Guid parameter) { }

				[Theory]
				[{|xUnit1044:ClassData(typeof(Class))|}]
				[{|xUnit1044:MemberData(nameof(Field))|}]
				[{|xUnit1044:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1044:MemberData(nameof(Property))|}]
				public void Triggers(Guid parameter) { }
			}

			public class VersionClass {
				public sealed class Class : TheoryData<Version> { }
				public static readonly TheoryData<Version> Field = new TheoryData<Version>() { };
				public static TheoryData<Version> Method(int a, string b) => new TheoryData<Version>() { };
				public static TheoryData<Version> Property => new TheoryData<Version>() { };

				[Theory]
				[MemberData(nameof(Field), DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Method), 1, "2", DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Property), DisableDiscoveryEnumeration = true)]
				public void DoesNotTrigger(Version parameter) { }

				[Theory]
				[{|xUnit1044:ClassData(typeof(Class))|}]
				[{|xUnit1044:MemberData(nameof(Field))|}]
				[{|xUnit1044:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1044:MemberData(nameof(Property))|}]
				public void Triggers(Version parameter) { }
			}
			""";

		await Verify.VerifyAnalyzerV2(source);
	}

#if NET6_0_OR_GREATER

	[Fact]
	public async ValueTask V2_only_NET60()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			// Things which are definitely not serializable in v2

			public class IndexClass {
				public sealed class Class : TheoryData<Index> { }
				public static readonly TheoryData<Index> Field = new TheoryData<Index>() { };
				public static TheoryData<Index> Method(int a, string b) => new TheoryData<Index>() { };
				public static TheoryData<Index> Property => new TheoryData<Index>() { };

				[Theory]
				[MemberData(nameof(Field), DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Method), 1, "2", DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Property), DisableDiscoveryEnumeration = true)]
				public void DoesNotTrigger(Index parameter) { }

				[Theory]
				[{|xUnit1044:ClassData(typeof(Class))|}]
				[{|xUnit1044:MemberData(nameof(Field))|}]
				[{|xUnit1044:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1044:MemberData(nameof(Property))|}]
				public void Triggers(Index parameter) { }
			}

			public class RangeClass {
				public sealed class Class : TheoryData<Range> { }
				public static readonly TheoryData<Range> Field = new TheoryData<Range>() { };
				public static TheoryData<Range> Method(int a, string b) => new TheoryData<Range>() { };
				public static TheoryData<Range> Property => new TheoryData<Range>() { };

				[Theory]
				[MemberData(nameof(Field), DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Method), 1, "2", DisableDiscoveryEnumeration = true)]
				[MemberData(nameof(Property), DisableDiscoveryEnumeration = true)]
				public void DoesNotTrigger(Range parameter) { }

				[Theory]
				[{|xUnit1044:ClassData(typeof(Class))|}]
				[{|xUnit1044:MemberData(nameof(Field))|}]
				[{|xUnit1044:MemberData(nameof(Method), 1, "2")|}]
				[{|xUnit1044:MemberData(nameof(Property))|}]
				public void Triggers(Range parameter) { }
			}
			""";

		await Verify.VerifyAnalyzerV2(source);
	}

#endif  // NET6_0_OR_GREATER

	[Fact]
	public async ValueTask V3_only_NonAOT()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Collections;
			using System.Collections.Generic;
			using Xunit;
			using Xunit.Sdk;

			[assembly: RegisterXunitSerializer(typeof(CustomSerializer), typeof(ICustomSerialized))]

			public class TestClass {
				[CulturedFact(new[] { "en-US" })]
				public void CulturedFactMethod_DoesNotTrigger() { }

				public static IEnumerable<object[]> Property1 => Array.Empty<object[]>();
				public static DataSource<IDisposable, Action> Property2 => new DataSource<IDisposable, Action>();

				[CulturedTheory(new[] { "en-US" })]
				[MemberData(nameof(Property1))]
				[MemberData(nameof(Property2))]
				public void CulturedTheoryMethod_WithoutTheoryDataAsDataSource_DoesNotTrigger(object a, object b) { }

				public class DataSource<T1, T2> : IEnumerable<object[]> {
					public IEnumerator<object[]> GetEnumerator() { yield break; }
					IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
				}
			}

			// Things which are supported by an IXunitSerializer implementation

			public class ICustomSerializedClass {
				public sealed class Class : TheoryData<ICustomSerialized> { }
				public static readonly TheoryData<ICustomSerialized> Field = new TheoryData<ICustomSerialized>() { };
				public static TheoryData<ICustomSerialized> Method(int a, string b) => new TheoryData<ICustomSerialized>() { };
				public static TheoryData<ICustomSerialized> Property => new TheoryData<ICustomSerialized>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(ICustomSerialized parameter) { }
			}

			public class CustomSerializedClass {
				public sealed class Class : TheoryData<CustomSerialized> { }
				public static readonly TheoryData<CustomSerialized> Field = new TheoryData<CustomSerialized>() { };
				public static TheoryData<CustomSerialized> Method(int a, string b) => new TheoryData<CustomSerialized>() { };
				public static TheoryData<CustomSerialized> Property => new TheoryData<CustomSerialized>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(CustomSerialized parameter) { }
			}

			public class CustomSerializedDerivedClass {
				public sealed class Class : TheoryData<CustomSerializedDerived> { }
				public static readonly TheoryData<CustomSerializedDerived> Field = new TheoryData<CustomSerializedDerived>() { };
				public static TheoryData<CustomSerializedDerived> Method(int a, string b) => new TheoryData<CustomSerializedDerived>() { };
				public static TheoryData<CustomSerializedDerived> Property => new TheoryData<CustomSerializedDerived>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(CustomSerializedDerived parameter) { }
			}

			public interface ICustomSerialized { }

			public class CustomSerialized : ICustomSerialized { }

			public class CustomSerializedDerived : CustomSerialized { }

			public class CustomSerializer : IXunitSerializer {
				public object Deserialize(Type type, string serializedValue) =>
					throw new NotImplementedException();

				public bool IsSerializable(Type type, object? value, out string? failureReason) {

					failureReason = null;
					return true;
				}

				public string Serialize(object value) =>
					throw new NotImplementedException();
			}

			// Things which are newly serializable in v3

			public class GuidClass {
				public sealed class Class : TheoryData<Guid> { }
				public static readonly TheoryData<Guid> Field = new TheoryData<Guid>() { };
				public static TheoryData<Guid> Method(int a, string b) => new TheoryData<Guid>() { };
				public static TheoryData<Guid> Property => new TheoryData<Guid>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(Guid parameter) { }
			}

			public class VersionClass {
				public sealed class Class : TheoryData<Version> { }
				public static readonly TheoryData<Version> Field = new TheoryData<Version>() { };
				public static TheoryData<Version> Method(int a, string b) => new TheoryData<Version>() { };
				public static TheoryData<Version> Property => new TheoryData<Version>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(Version parameter) { }
			}

			public class ValueTupleClass {
				public sealed class Class : TheoryData<ValueTuple<string, int>> { }
				public static readonly TheoryData<ValueTuple<string, int>> Field = new TheoryData<ValueTuple<string, int>>() { };
				public static TheoryData<ValueTuple<string, int>> Method(int a, string b) => new TheoryData<ValueTuple<string, int>>() { };
				public static TheoryData<ValueTuple<string, int>> Property => new TheoryData<ValueTuple<string, int>>() { };

				[Theory]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(ValueTuple<string, int> parameter) { }
			}

			// Things which are definitely not serializable

			public class DelegateClass {
				public sealed class Class : TheoryData<Delegate> { }
				public static readonly TheoryData<Delegate> Field = new TheoryData<Delegate>() { };
				public static TheoryData<Delegate> Method(int a, string b) => new TheoryData<Delegate>() { };
				public static TheoryData<Delegate> Property => new TheoryData<Delegate>() { };

				[Theory(DisableDiscoveryEnumeration = true)]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(Delegate parameter) { }
			}

			public class NonSerializableSealedClassClass {
				public sealed class Class : TheoryData<NonSerializableSealedClass> { }
				public static readonly TheoryData<NonSerializableSealedClass> Field = new TheoryData<NonSerializableSealedClass>() { };
				public static TheoryData<NonSerializableSealedClass> Method(int a, string b) => new TheoryData<NonSerializableSealedClass>() { };
				public static TheoryData<NonSerializableSealedClass> Property => new TheoryData<NonSerializableSealedClass>() { };

				[Theory(DisableDiscoveryEnumeration = true)]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(NonSerializableSealedClass parameter) { }
			}

			public class NonSerializableStructClass {
				public sealed class Class : TheoryData<NonSerializableStruct> { }
				public static readonly TheoryData<NonSerializableStruct> Field = new TheoryData<NonSerializableStruct>() { };
				public static TheoryData<NonSerializableStruct> Method(int a, string b) => new TheoryData<NonSerializableStruct>() { };
				public static TheoryData<NonSerializableStruct> Property => new TheoryData<NonSerializableStruct>() { };

				[Theory(DisableDiscoveryEnumeration = true)]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(NonSerializableStruct parameter) { }
			}

			public class ObjectClass {
				public sealed class Class : TheoryData<object> { }
				public static readonly TheoryData<object> Field = new TheoryData<object>() { };
				public static TheoryData<object> Method(int a, string b) => new TheoryData<object>() { };
				public static TheoryData<object> Property => new TheoryData<object>() { };

				[Theory(DisableDiscoveryEnumeration = true)]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(object parameter) { }
			}

			public class IPossiblySerializableInterfaceClass {
				public sealed class Class : TheoryData<IPossiblySerializableInterface> { }
				public static readonly TheoryData<IPossiblySerializableInterface> Field = new TheoryData<IPossiblySerializableInterface>() { };
				public static TheoryData<IPossiblySerializableInterface> Method(int a, string b) => new TheoryData<IPossiblySerializableInterface>() { };
				public static TheoryData<IPossiblySerializableInterface> Property => new TheoryData<IPossiblySerializableInterface>() { };

				[Theory(DisableDiscoveryEnumeration = true)]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(IPossiblySerializableInterface parameter) { }
			}

			public class PossiblySerializableUnsealedClassClass {
				public sealed class Class : TheoryData<PossiblySerializableUnsealedClass> { }
				public static readonly TheoryData<PossiblySerializableUnsealedClass> Field = new TheoryData<PossiblySerializableUnsealedClass>() { };
				public static TheoryData<PossiblySerializableUnsealedClass> Method(int a, string b) => new TheoryData<PossiblySerializableUnsealedClass>() { };
				public static TheoryData<PossiblySerializableUnsealedClass> Property => new TheoryData<PossiblySerializableUnsealedClass>() { };

				[Theory(DisableDiscoveryEnumeration = true)]
				[ClassData(typeof(Class))]
				[MemberData(nameof(Field))]
				[MemberData(nameof(Method), 1, "2")]
				[MemberData(nameof(Property))]
				public void TestMethod(PossiblySerializableUnsealedClass parameter) { }
			}

			public sealed class NonSerializableSealedClass { }

			public struct NonSerializableStruct { }

			public interface IPossiblySerializableInterface { }

			public class PossiblySerializableUnsealedClass { }
			""";

		await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp8, source);
	}

	internal class Analyzer_v3_Pre301 : TheoryDataTypeArgumentsShouldBeSerializable
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV3(compilation, new Version(3, 0, 0));
	}
}
