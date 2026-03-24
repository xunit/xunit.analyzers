using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Verify = CSharpVerifier<Xunit.Analyzers.TheoryDataRowArgumentsShouldBeSerializable>;

public class X1046_TheoryDataRowArgumentsShouldBeSerializableTests
{
	[Fact]
	public async ValueTask V3_only_NonAOT()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using System;
			using System.Collections.Generic;
			using System.Numerics;
			using Xunit;
			using Xunit.Sdk;

			[assembly: RegisterXunitSerializer(typeof(CustomSerializer), typeof(ICustomSerialized))]

			public class ParamArrayArguments_NotUsingTheoryDataRow_DoesNotTrigger {
				public ParamArrayArguments_NotUsingTheoryDataRow_DoesNotTrigger(params object[] args) { }
			}

			public class TestClass {
				// Values with built-in support

				public IEnumerable<TheoryDataRowBase> StringValue() {
					var value = "String value";
					var defaultValue = default(string);
					var nullValue = default(string?);
					var arrayValue = new string[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<string, string?, string?, string[]>("String value", default(string), default(string?), new string[0]);
				}

				public IEnumerable<TheoryDataRowBase> CharValue() {
					var value = 'a';
					var defaultValue = default(char);
					var nullValue = default(char?);
					var arrayValue = new char[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<char, char, char?, char[]>('a', default(char), default(char?), new char[0]);
				}

				public IEnumerable<TheoryDataRowBase> ByteValue() {
					var value = (byte)42;
					var defaultValue = default(byte);
					var nullValue = default(byte?);
					var arrayValue = new byte[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<byte, byte, byte?, byte[]>(42, default(byte), default(byte?), new byte[0]);
				}

				public IEnumerable<TheoryDataRowBase> SByteValue() {
					var value = (sbyte)42;
					var defaultValue = default(byte);
					var nullValue = default(byte?);
					var arrayValue = new byte[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<sbyte, sbyte, sbyte?, sbyte[]>(42, default(sbyte), default(sbyte?), new sbyte[0]);
				}

				public IEnumerable<TheoryDataRowBase> Int16Value() {
					var value = (short)42;
					var defaultValue = default(short);
					var nullValue = default(short?);
					var arrayValue = new short[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<short, short, short?, short[]>(42, default(short), default(short?), new short[0]);
				}

				public IEnumerable<TheoryDataRowBase> UInt16Value() {
					var value = (ushort)42;
					var defaultValue = default(ushort);
					var nullValue = default(ushort?);
					var arrayValue = new ushort[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<ushort, ushort, ushort?, ushort[]>(42, default(ushort), default(ushort?), new ushort[0]);
				}

				public IEnumerable<TheoryDataRowBase> Int32Value() {
					var value = 42;
					var defaultValue = default(int);
					var nullValue = default(int?);
					var arrayValue = new int[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<int, int, int?, int[]>(42, default(int), default(int?), new int[0]);
				}

				public IEnumerable<TheoryDataRowBase> UInt32Value() {
					var value = 42U;
					var defaultValue = default(uint);
					var nullValue = default(uint?);
					var arrayValue = new uint[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<uint, uint, uint?, uint[]>(42, default(uint), default(uint?), new uint[0]);
				}

				public IEnumerable<TheoryDataRowBase> Int64Value() {
					var value = 42L;
					var defaultValue = default(long);
					var nullValue = default(long?);
					var arrayValue = new long[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<long, long, long?, long[]>(42, default(long), default(long?), new long[0]);
				}

				public IEnumerable<TheoryDataRowBase> UInt64Value() {
					var value = 42UL;
					var defaultValue = default(ulong);
					var nullValue = default(ulong?);
					var arrayValue = new ulong[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<ulong, ulong, ulong?, ulong[]>(42, default(ulong), default(ulong?), new ulong[0]);
				}

				public IEnumerable<TheoryDataRowBase> SingleValue() {
					var value = 21.12F;
					var defaultValue = default(float);
					var nullValue = default(float?);
					var arrayValue = new float[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<float, float, float?, float[]>(21.12F, default(float), default(float?), new float[0]);
				}

				public IEnumerable<TheoryDataRowBase> DoubleValue() {
					var value = 21.12D;
					var defaultValue = default(double);
					var nullValue = default(double?);
					var arrayValue = new double[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<double, double, double?, double[]>(21.12D, default(double), default(double?), new double[0]);
				}

				public IEnumerable<TheoryDataRowBase> DecimalValue() {
					var value = 21.12M;
					var defaultValue = default(decimal);
					var nullValue = default(decimal?);
					var arrayValue = new decimal[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<decimal, decimal, decimal?, decimal[]>(21.12M, default(decimal), default(decimal?), new decimal[0]);
				}

				public IEnumerable<TheoryDataRowBase> BooleanValue() {
					var value = true;
					var defaultValue = default(bool);
					var nullValue = default(bool?);
					var arrayValue = new bool[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<bool, bool, bool?, bool[]>(false, default(bool), default(bool?), new bool[0]);
				}

				public IEnumerable<TheoryDataRowBase> DateTimeValue() {
					var value = DateTime.MinValue;
					var defaultValue = default(DateTime);
					var nullValue = default(DateTime?);
					var arrayValue = new DateTime[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<DateTime, DateTime, DateTime?, DateTime[]>(DateTime.MaxValue, default(DateTime), default(DateTime?), new DateTime[0]);
				}

				public IEnumerable<TheoryDataRowBase> DateTimeOffsetValue() {
					var value = DateTimeOffset.MinValue;
					var defaultValue = default(DateTimeOffset);
					var nullValue = default(DateTimeOffset?);
					var arrayValue = new DateTimeOffset[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<DateTimeOffset, DateTimeOffset, DateTimeOffset?, DateTimeOffset[]>(DateTimeOffset.MaxValue, default(DateTimeOffset), default(DateTimeOffset?), new DateTimeOffset[0]);
				}

				public IEnumerable<TheoryDataRowBase> TimeSpanValue() {
					var value = TimeSpan.MinValue;
					var defaultValue = default(TimeSpan);
					var nullValue = default(TimeSpan?);
					var arrayValue = new TimeSpan[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<TimeSpan, TimeSpan, TimeSpan?, TimeSpan[]>(TimeSpan.MaxValue, default(TimeSpan), default(TimeSpan?), new TimeSpan[0]);
				}

				public IEnumerable<TheoryDataRowBase> BigIntegerValue() {
					var value = BigInteger.Zero;
					var defaultValue = default(BigInteger);
					var nullValue = default(BigInteger?);
					var arrayValue = new BigInteger[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<BigInteger, BigInteger, BigInteger?, BigInteger[]>(BigInteger.One, default(BigInteger), default(BigInteger?), new BigInteger[0]);
				}

				public IEnumerable<TheoryDataRowBase> TypeValue() {
					var value = typeof(string);
					var defaultValue = default(Type);
					var nullValue = default(Type?);
					var arrayValue = new Type[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<Type, Type?, Type?, Type[]>(typeof(object), default(Type), default(Type?), new Type[0]);
				}

				public IEnumerable<TheoryDataRowBase> EnumValue() {
					var value = ConsoleColor.Red;
					var defaultValue = default(ConsoleColor);
					var nullValue = default(ConsoleColor?);
					var arrayValue = new ConsoleColor[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<ConsoleColor, ConsoleColor, ConsoleColor?, ConsoleColor[]>(ConsoleColor.Blue, default(ConsoleColor), default(ConsoleColor?), new ConsoleColor[0]);
				}

				public IEnumerable<TheoryDataRowBase> TraitsDictionaryValue() {
					var value = new Dictionary<string, List<string>>();
					var defaultValue = default(Dictionary<string, List<string>>);
					var nullValue = default(Dictionary<string, List<string>>?);
					var arrayValue = new Dictionary<string, List<string>>[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<Dictionary<string, List<string>>, Dictionary<string, List<string>>?, Dictionary<string, List<string>>?, Dictionary<string, List<string>>[]>(new Dictionary<string, List<string>>(), default(Dictionary<string, List<string>>), default(Dictionary<string, List<string>>?), new Dictionary<string, List<string>>[0]);
				}

				public IEnumerable<TheoryDataRowBase> GuidValue() {
					var value = Guid.Empty;
					var defaultValue = default(Guid);
					var nullValue = default(Guid?);
					var arrayValue = new Guid[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<Guid, Guid?, Guid?, Guid[]>(Guid.Empty, default(Guid), default(Guid?), new Guid[0]);
				}

				public IEnumerable<TheoryDataRowBase> UriValue() {
					var value = new Uri("https://xunit.net/");
					var defaultValue = default(Uri);
					var nullValue = default(Uri?);
					var arrayValue = new Uri[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<Uri, Uri?, Uri?, Uri[]>(new Uri("https://xunit.net/"), default(Uri), default(Uri?), new Uri[0]);
				}

				public IEnumerable<TheoryDataRowBase> VersionValue() {
					var value = new Version("1.2.3");
					var defaultValue = default(Version);
					var nullValue = default(Version?);
					var arrayValue = new Version[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<Version, Version?, Version?, Version[]>(new Version("1.2.3"), default(Version), default(Version?), new Version[0]);
				}

				// Values with customized serialization

				public IEnumerable<TheoryDataRowBase> IXunitSerializableClassValue() {
					var value = new SerializableClass();
					var defaultValue = default(SerializableClass);
					var nullValue = default(SerializableClass?);
					var arrayValue = new SerializableClass[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<SerializableClass, SerializableClass?, SerializableClass?, SerializableClass[]>(new SerializableClass(), default(SerializableClass), default(SerializableClass?), new SerializableClass[0]);
				}

				public IEnumerable<TheoryDataRowBase> IXunitSerializableStructValue() {
					var value = new SerializableStruct();
					var defaultValue = default(SerializableStruct);
					var nullValue = default(SerializableStruct?);
					var arrayValue = new SerializableStruct[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<SerializableStruct, SerializableStruct, SerializableStruct?, SerializableStruct[]>(new SerializableStruct(), default(SerializableStruct), default(SerializableStruct?), new SerializableStruct[0]);
				}

				public IEnumerable<TheoryDataRowBase> IXunitSerializedValue() {
					var value = new CustomSerialized();
					var defaultValue = default(CustomSerialized);
					var nullValue = default(CustomSerialized?);
					var arrayValue = new CustomSerialized[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<CustomSerialized, CustomSerialized?, CustomSerialized?, CustomSerialized[]>(new CustomSerialized(), default(CustomSerialized), default(CustomSerialized?), new CustomSerialized[0]);
				}

				public IEnumerable<TheoryDataRowBase> IXunitSerializedDerivedValue() {
					var value = new CustomSerializedDerived();
					var defaultValue = default(CustomSerializedDerived);
					var nullValue = default(CustomSerializedDerived?);
					var arrayValue = new CustomSerialized[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<CustomSerializedDerived, CustomSerializedDerived?, CustomSerializedDerived?, CustomSerializedDerived[]>(new CustomSerializedDerived(), default(CustomSerializedDerived), default(CustomSerializedDerived?), new CustomSerializedDerived[0]);
				}

				// Known not supported types

				public IEnumerable<TheoryDataRowBase> DelegateValue() {
					var defaultValue = default(Delegate);
					var nullValue = default(Delegate?);
					var arrayValue = new Delegate[0];

					yield return new TheoryDataRow({|#0:defaultValue|}, {|#1:nullValue|}, {|#2:arrayValue|});
					yield return new TheoryDataRow<Delegate?, Delegate?, Delegate[]>({|#3:default(Delegate)|}, {|#4:default(Delegate?)|}, {|#5:new Delegate[0]|});
				}

				public IEnumerable<TheoryDataRowBase> FuncValue() {
					var defaultValue = default(Func<int>);
					var nullValue = default(Func<int>?);
					var arrayValue = new Func<int>[0];

					yield return new TheoryDataRow({|#10:defaultValue|}, {|#11:nullValue|}, {|#12:arrayValue|});
					yield return new TheoryDataRow<Func<int>?, Func<int>?, Func<int>[]>({|#13:default(Func<int>)|}, {|#14:default(Func<int>?)|}, {|#15:new Func<int>[0]|});
				}

				public IEnumerable<TheoryDataRowBase> NonSerializableSealedClassValue() {
					var defaultValue = default(NonSerializableSealedClass);
					var nullValue = default(NonSerializableSealedClass?);
					var arrayValue = new NonSerializableSealedClass[0];

					yield return new TheoryDataRow({|#20:defaultValue|}, {|#21:nullValue|}, {|#22:arrayValue|});
					yield return new TheoryDataRow<NonSerializableSealedClass?, NonSerializableSealedClass?, NonSerializableSealedClass[]>({|#23:default(NonSerializableSealedClass)|}, {|#24:default(NonSerializableSealedClass?)|}, {|#25:new NonSerializableSealedClass[0]|});
				}

				public IEnumerable<TheoryDataRowBase> NonSerializableStructValue() {
					var defaultValue = default(NonSerializableStruct);
					var nullValue = default(NonSerializableStruct?);
					var arrayValue = new NonSerializableStruct[0];

					yield return new TheoryDataRow({|#30:defaultValue|}, {|#31:nullValue|}, {|#32:arrayValue|});
					yield return new TheoryDataRow<NonSerializableStruct, NonSerializableStruct?, NonSerializableStruct[]>({|#33:default(NonSerializableStruct)|}, {|#34:default(NonSerializableStruct?)|}, {|#35:new NonSerializableStruct[0]|});
				}
			}

			// IXunitSerializable

			public interface ISerializableInterface : IXunitSerializable { }

			public class SerializableClass : ISerializableInterface {
				public void Deserialize(IXunitSerializationInfo info) { }
				public void Serialize(IXunitSerializationInfo info) { }
			}

			public struct SerializableStruct : ISerializableInterface {
				public void Deserialize(IXunitSerializationInfo info) { }
				public void Serialize(IXunitSerializationInfo info) { }
			}

			// IXunitSerializer

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

			// Known non-serializable types

			public sealed class NonSerializableSealedClass { }

			public struct NonSerializableStruct { }
			""";
		var expected = new[] {
			Verify.Diagnostic("xUnit1046").WithLocation(0).WithArguments("defaultValue", "Delegate?"),
			Verify.Diagnostic("xUnit1046").WithLocation(1).WithArguments("nullValue", "Delegate?"),
			Verify.Diagnostic("xUnit1046").WithLocation(2).WithArguments("arrayValue", "Delegate[]"),
			Verify.Diagnostic("xUnit1046").WithLocation(3).WithArguments("default(Delegate)", "Delegate?"),
			Verify.Diagnostic("xUnit1046").WithLocation(4).WithArguments("default(Delegate?)", "Delegate?"),
			Verify.Diagnostic("xUnit1046").WithLocation(5).WithArguments("new Delegate[0]", "Delegate[]"),

			Verify.Diagnostic("xUnit1046").WithLocation(10).WithArguments("defaultValue", "Func<int>?"),
			Verify.Diagnostic("xUnit1046").WithLocation(11).WithArguments("nullValue", "Func<int>?"),
			Verify.Diagnostic("xUnit1046").WithLocation(12).WithArguments("arrayValue", "Func<int>[]"),
			Verify.Diagnostic("xUnit1046").WithLocation(13).WithArguments("default(Func<int>)", "Func<int>?"),
			Verify.Diagnostic("xUnit1046").WithLocation(14).WithArguments("default(Func<int>?)", "Func<int>?"),
			Verify.Diagnostic("xUnit1046").WithLocation(15).WithArguments("new Func<int>[0]", "Func<int>[]"),

			Verify.Diagnostic("xUnit1046").WithLocation(20).WithArguments("defaultValue", "NonSerializableSealedClass?"),
			Verify.Diagnostic("xUnit1046").WithLocation(21).WithArguments("nullValue", "NonSerializableSealedClass?"),
			Verify.Diagnostic("xUnit1046").WithLocation(22).WithArguments("arrayValue", "NonSerializableSealedClass[]"),
			Verify.Diagnostic("xUnit1046").WithLocation(23).WithArguments("default(NonSerializableSealedClass)", "NonSerializableSealedClass?"),
			Verify.Diagnostic("xUnit1046").WithLocation(24).WithArguments("default(NonSerializableSealedClass?)", "NonSerializableSealedClass?"),
			Verify.Diagnostic("xUnit1046").WithLocation(25).WithArguments("new NonSerializableSealedClass[0]", "NonSerializableSealedClass[]"),

			Verify.Diagnostic("xUnit1046").WithLocation(30).WithArguments("defaultValue", "NonSerializableStruct"),
			Verify.Diagnostic("xUnit1046").WithLocation(31).WithArguments("nullValue", "NonSerializableStruct?"),
			Verify.Diagnostic("xUnit1046").WithLocation(32).WithArguments("arrayValue", "NonSerializableStruct[]"),
			Verify.Diagnostic("xUnit1046").WithLocation(33).WithArguments("default(NonSerializableStruct)", "NonSerializableStruct"),
			Verify.Diagnostic("xUnit1046").WithLocation(34).WithArguments("default(NonSerializableStruct?)", "NonSerializableStruct?"),
			Verify.Diagnostic("xUnit1046").WithLocation(35).WithArguments("new NonSerializableStruct[0]", "NonSerializableStruct[]"),
		};

		await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp8, source, expected);
	}


	[Fact]
	public async ValueTask V3_only_NonAOT_Net6Types()
	{
		var source = /* lang=c#-test */ """
			#nullable enable

			using System;
			using System.Collections.Generic;
			using System.Numerics;
			using Xunit;

			public class IntrinsicallySerializableValue_DoesNotTrigger {
				public IEnumerable<TheoryDataRowBase> DateOnlyValue() {
					var value = DateOnly.MinValue;
					var defaultValue = default(DateOnly);
					var nullValue = default(DateOnly?);
					var arrayValue = new DateOnly[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<DateOnly, DateOnly, DateOnly?, DateOnly[]>(DateOnly.MaxValue, default(DateOnly), default(DateOnly?), new DateOnly[0]);
				}

				public IEnumerable<TheoryDataRowBase> IndexValue() {
					var value = Index.Start;
					var defaultValue = default(Index);
					var nullValue = default(Index?);
					var arrayValue = new Index[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<Index, Index, Index?, Index[]>(Index.End, default(Index), default(Index?), new Index[0]);
				}

				public IEnumerable<TheoryDataRowBase> RangeValue() {
					var value = Range.All;
					var defaultValue = default(Range);
					var nullValue = default(Range?);
					var arrayValue = new Range[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<Range, Range, Range?, Range[]>(Range.All, default(Range), default(Range?), new Range[0]);
				}

				public IEnumerable<TheoryDataRowBase> TimeOnlyValue() {
					var value = TimeOnly.MinValue;
					var defaultValue = default(TimeOnly);
					var nullValue = default(TimeOnly?);
					var arrayValue = new TimeOnly[0];

					yield return new TheoryDataRow(value, defaultValue, nullValue, arrayValue);
					yield return new TheoryDataRow<TimeOnly, TimeOnly, TimeOnly?, TimeOnly[]>(TimeOnly.MaxValue, default(TimeOnly), default(TimeOnly?), new TimeOnly[0]);
				}
			}
			""";

#if NET6_0_OR_GREATER  // This is here because otherwise `dotnet format` destroys the multi-line source string
		await Verify.VerifyAnalyzerV3NonAot(LanguageVersion.CSharp8, source);
#else
		Assert.NotNull(source);
		await Task.Yield();
#endif
	}
}
