using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Analyzers;
using Verify = CSharpVerifier<Xunit.Analyzers.InlineDataMustMatchTheoryParameters>;
using Verify_v2_Pre240 = CSharpVerifier<X1010_InlineDataMustMatchTheoryParametersTests.Analyzer_v2_Pre240>;

public class X1010_InlineDataMustMatchTheoryParametersTests
{
	[Fact]
	public async ValueTask V2_and_V3()
	{
		var source = /* lang=c#-test */ """
			using System;
			using System.Runtime.InteropServices;
			using Xunit;

			class TestClass {
				[Theory]
				[InlineData("abc", 1, null)]
				void MethodWithImplicitParameters(string a, int b, object c) { }

				[Theory]
				[InlineData(new object[] { "abc", 1, null })]
				void MethodWithExplicitParameters(string a, int b, object c) { }

				[Theory]
				[InlineData(data: new object[] { "abc", 1, null })]
				void MethodWithExplicitParameters_Named(string a, int b, object c) { }

				[Theory]
				[InlineData(new byte[0])]
				void MethodWithEmptyArray(byte[] input) { }

				// https://github.com/xunit/xunit/issues/3000
				[Theory]
				[InlineData({|CS0182:0.1m|})]
				void MethodWithDecimalValue(decimal m)
				{ }

				// Optional parameters

				[Theory]
				[InlineData("abc")]
				[InlineData("abc", "def")]
				[InlineData("abc", "def", "ghi")]
				void MethodWithDefaultParameterValues(string a, string b = "default", string c = null) { }

				[Theory]
				[InlineData]
				[InlineData("abc")]
				[InlineData("abc", "def")]
				void MethodWithOptionalAttribute([Optional] string a, [Optional] string b) { }

				// Params parameter

				[Theory]
				[InlineData("abc", "xyz")]
				void MethodWithImplicitParams(params string[] args) { }

				[Theory]
				[InlineData(new object[] { new string[] { "abc", "xyz" } })]
				void MethodWithExplicitParams(params string[] args) { }

				[Theory]
				[InlineData]
				void MethodWithImplicitEmptyParams(params string[] args) { }

				[Theory]
				[InlineData(new object[] { new string[0] })]
				void MethodWithExplicitEmptyParams(params string[] args) { }

				[Theory]
				[InlineData(null)]
				void MethodWithNullParams(params string[] args) { }

				[Theory]
				[InlineData("abc", "xyz")]
				void MethodWithMixedNormalAndImplicitParams(string first, params string[] args) { }

				[Theory]
				[InlineData("abc", new[] { "xyz" })]
				void MethodWithMixedNormalAndExplicitParams(string first, params string[] args) { }

				[Theory]
				[InlineData("abc")]
				void MethodWithMixedNormalAndImplicitEmptyParams(string first, params string[] args) { }

				[Theory]
				[InlineData("abc", new string[] { })]
				void MethodWithMixedNormalAndExplicitEmptyParams(string first, params string[] args) { }

				[Theory]
				[InlineData("abc", null)]
				void MethodWithMixedNormalAndNullParams(string first, params string[] args) { }

				// Mixed optional and params parameters

				[Theory]
				[InlineData("abc")]
				void MethodWithOptionalParametersWithDefaultValuesAndParamsParameter(string a, string b = "default", string c = null, params string[] d) { }

				[Theory]
				[InlineData(21.12, {|xUnit1010:new object[] { }|})]
				void IncorrectParamsArrayType(double d, params int[] sq) { }

				// Numeric parameters

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData(-42)][InlineData(-42L)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToInt16(short n) { }

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData({|xUnit1010:-42|})][InlineData({|xUnit1010:-42L|})]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToUInt16(ushort n) { }

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData(-42)][InlineData(-42L)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToInt32(int n) { }

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData({|xUnit1010:-42|})][InlineData({|xUnit1010:-42L|})]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToUInt32(uint n) { }

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData(-42)][InlineData(-42L)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToInt64(long n) { }

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData({|xUnit1010:-42|})][InlineData({|xUnit1010:-42L|})]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToUInt64(ulong n) { }

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData({|xUnit1010:-42|})][InlineData({|xUnit1010:-42L|})]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToByte(byte n) { }

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData(-42)][InlineData(-42L)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToSByte(sbyte n) { }

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData(-42)][InlineData(-42L)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToFloat(float n) { }

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData(-42)][InlineData(-42L)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToDouble(double n) { }

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData(-42)][InlineData(-42L)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToDecimal(decimal n) { }

				// Nullable numeric parameters

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableInt16(short? n) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableUInt16(ushort? n) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableInt32(int? n) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableUInt32(uint? n) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableInt64(long? n) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableUInt64(ulong? n) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableByte(byte? n) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableSByte(sbyte? n) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableFloat(float? n) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableDouble(double? n) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableDecimal(decimal? n) { }

				// To boolean

				[Theory]
				[InlineData({|xUnit1010:42|})][InlineData({|xUnit1010:42L|})][InlineData({|xUnit1010:42U|})][InlineData({|xUnit1010:42UL|})]
				[InlineData({|xUnit1010:(short)42|})][InlineData({|xUnit1010:(byte)42|})][InlineData({|xUnit1010:(ushort)42|})][InlineData({|xUnit1010:(sbyte)42|})]
				[InlineData({|xUnit1010:42F|})][InlineData({|xUnit1010:42D|})]
				[InlineData({|xUnit1010:'a'|})][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData({|xUnit1010:StringComparison.InvariantCulture|})]
				[InlineData(true)][InlineData(false)]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToBoolean(bool b) { }

				[Theory]
				[InlineData(null)]
				[InlineData({|xUnit1010:42|})][InlineData({|xUnit1010:42L|})][InlineData({|xUnit1010:42U|})][InlineData({|xUnit1010:42UL|})]
				[InlineData({|xUnit1010:(short)42|})][InlineData({|xUnit1010:(byte)42|})][InlineData({|xUnit1010:(ushort)42|})][InlineData({|xUnit1010:(sbyte)42|})]
				[InlineData({|xUnit1010:42F|})][InlineData({|xUnit1010:42D|})]
				[InlineData({|xUnit1010:'a'|})][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData({|xUnit1010:StringComparison.InvariantCulture|})]
				[InlineData(true)][InlineData(false)]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableBoolean(bool? b) { }

				// To character

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData({|xUnit1010:42F|})][InlineData({|xUnit1010:42D|})]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToChar(char c) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				// vvv These shouldn't pass, but they do because Roslyn conversion says it's an acceptable explicit nullable conversion
				// [InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableChar(char? c) { }

				// To enum

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToEnum(StringComparison s) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableEnum(StringComparison? s) { }

				// To Type

				[Theory]
				[InlineData({|xUnit1010:42|})][InlineData({|xUnit1010:42L|})][InlineData({|xUnit1010:42U|})][InlineData({|xUnit1010:42UL|})]
				[InlineData({|xUnit1010:(short)42|})][InlineData({|xUnit1010:(byte)42|})][InlineData({|xUnit1010:(ushort)42|})][InlineData({|xUnit1010:(sbyte)42|})]
				[InlineData({|xUnit1010:42F|})][InlineData({|xUnit1010:42D|})]
				[InlineData({|xUnit1010:'a'|})][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData({|xUnit1010:StringComparison.InvariantCulture|})]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData(typeof(string))]
				void ToType(Type t) { }

				[Theory]
				[InlineData(null)]
				[InlineData({|xUnit1010:42|})][InlineData({|xUnit1010:42L|})][InlineData({|xUnit1010:42U|})][InlineData({|xUnit1010:42UL|})]
				[InlineData({|xUnit1010:(short)42|})][InlineData({|xUnit1010:(byte)42|})][InlineData({|xUnit1010:(ushort)42|})][InlineData({|xUnit1010:(sbyte)42|})]
				[InlineData({|xUnit1010:42F|})][InlineData({|xUnit1010:42D|})]
				[InlineData({|xUnit1010:'a'|})][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData({|xUnit1010:StringComparison.InvariantCulture|})]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData(typeof(string))]
				void ToNullableType(Type? t) { }

				// To string

				[Theory]
				[InlineData({|xUnit1010:42|})][InlineData({|xUnit1010:42L|})][InlineData({|xUnit1010:42U|})][InlineData({|xUnit1010:42UL|})]
				[InlineData({|xUnit1010:(short)42|})][InlineData({|xUnit1010:(byte)42|})][InlineData({|xUnit1010:(ushort)42|})][InlineData({|xUnit1010:(sbyte)42|})]
				[InlineData({|xUnit1010:42F|})][InlineData({|xUnit1010:42D|})]
				[InlineData({|xUnit1010:'a'|})][InlineData("Hello world")]
				[InlineData({|xUnit1010:StringComparison.InvariantCulture|})]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToString(string s) { }

				[Theory]
				[InlineData(null)]
				[InlineData({|xUnit1010:42|})][InlineData({|xUnit1010:42L|})][InlineData({|xUnit1010:42U|})][InlineData({|xUnit1010:42UL|})]
				[InlineData({|xUnit1010:(short)42|})][InlineData({|xUnit1010:(byte)42|})][InlineData({|xUnit1010:(ushort)42|})][InlineData({|xUnit1010:(sbyte)42|})]
				[InlineData({|xUnit1010:42F|})][InlineData({|xUnit1010:42D|})]
				[InlineData({|xUnit1010:'a'|})][InlineData("Hello world")]
				[InlineData({|xUnit1010:StringComparison.InvariantCulture|})]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableString(string? t) { }

				// To interface

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				// vvv This is allowed in .NET, but not in .NET Framework
				// [InlineData('a')]
				[InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToInterface(IFormattable f) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				// vvv This is allowed in .NET, but not in .NET Framework
				// [InlineData('a')]
				[InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToNullableInterface(IFormattable? f) { }

				// To object

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData("Hello world")]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData(true)][InlineData(false)]
				[InlineData(typeof(string))]
				void ToObject(object o) { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData("Hello world")]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData(true)][InlineData(false)]
				[InlineData(typeof(string))]
				void ToNullableObject(object? o) { }

				// To generic parameter

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData("Hello world")]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData(true)][InlineData(false)]
				[InlineData(typeof(string))]
				void ToGenericWithoutConstraint<T>(T o) { }

				[Theory]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				[InlineData('a')][InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData(true)][InlineData(false)]
				[InlineData({|xUnit1010:typeof(string)|})]
				void ToGenericWithStructConstraint<T>(T o) where T : struct { }

				[Theory]
				[InlineData(null)]
				[InlineData({|xUnit1010:42|})][InlineData({|xUnit1010:42L|})][InlineData({|xUnit1010:42U|})][InlineData({|xUnit1010:42UL|})]
				[InlineData({|xUnit1010:(short)42|})][InlineData({|xUnit1010:(byte)42|})][InlineData({|xUnit1010:(ushort)42|})][InlineData({|xUnit1010:(sbyte)42|})]
				[InlineData({|xUnit1010:42F|})][InlineData({|xUnit1010:42D|})]
				[InlineData({|xUnit1010:'a'|})][InlineData("Hello world")]
				[InlineData({|xUnit1010:StringComparison.InvariantCulture|})]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData(typeof(string))]
				void ToGenericWithClassConstraint<T>(T o) where T : class { }

				[Theory]
				[InlineData(null)]
				[InlineData(42)][InlineData(42L)][InlineData(42U)][InlineData(42UL)]
				[InlineData((short)42)][InlineData((byte)42)][InlineData((ushort)42)][InlineData((sbyte)42)]
				[InlineData(42F)][InlineData(42D)]
				// vvv This is allowed in .NET, but not in .NET Framework
				// [InlineData('a')]
				[InlineData({|xUnit1010:"Hello world"|})]
				[InlineData(StringComparison.InvariantCulture)]
				[InlineData({|xUnit1010:true|})][InlineData({|xUnit1010:false|})]
				[InlineData({|xUnit1010:typeof(string)|})]
				[InlineData(new int[] { {|xUnit1010:1|}, 2, 3 })]  // Incompatible array type
				void ToGenericWithInterfaceConstraint<T>(T o) where T : IConvertible, IFormattable { }

				[Theory]
				[InlineData(new int[] { 1, 2, 3 })]
				void ToGenericArray<T>(T[] a) { }

				// To date-time parameter

				[Theory]
				[InlineData({|xUnit1010:42|})]
				[InlineData("2026-03-23T13:57:23Z")]
				void ToDateTime(DateTime d) { }

				[Theory]
				[InlineData({|xUnit1010:42|})]
				[InlineData("2026-03-23T13:57:23Z")]
				void ToDateTimeOffset(DateTimeOffset d) { }

				// To Guid parameter

				[Theory]
				[InlineData({|xUnit1010:42|})]
				[InlineData("4EBCD32C-A2B8-4600-9E72-3873347E285C")]
				void ToGuid(Guid g) { }
			}

			#nullable enable

			class NullableTestClass {
				[Theory]
				[InlineData("abc", 1, null)]
				void MethodWithImplicitParameters(string a, int b, object? c) { }

				[Theory]
				[InlineData(new object[] { "abc", 1, null })]
				void MethodWithExplicitParameters(string a, int b, object? c) { }

				[Theory]
				[InlineData(data: new object[] { "abc", 1, null })]
				void MethodWithExplicitParameters_Named(string a, int b, object? c) { }

				// Optional parameters

				[Theory]
				[InlineData("abc")]
				[InlineData("abc", "def")]
				[InlineData("abc", "def", "ghi")]
				void MethodWithDefaultParameterValues(string a, string b = "default", string? c = null) { }

				[Theory]
				[InlineData]
				[InlineData("abc")]
				[InlineData("abc", "def")]
				void MethodWithOptionalAttribute([Optional] string? a, [Optional] string? b) { }

				// Params parameter

				[Theory]
				[InlineData(null)]
				void MethodWithNullParams(params string[]? args) { }

				[Theory]
				[InlineData("abc", null)]
				void MethodWithMixedNormalAndNullParams(string first, params string[]? args) { }

				// Mixed optional and params parameters

				[Theory]
				[InlineData("abc")]
				void MethodWithOptionalParametersWithDefaultValuesAndParamsParameter(string a, string b = "default", string? c = null, params string[] d) { }
			}
			""";

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp8, source);
	}

	[Fact]
	public async ValueTask V2_only_Pre240()
	{
		var source = /* lang=c#-test */ """
			using System;
			using Xunit;

			class TestClass {
				[Theory]
				[InlineData({|xUnit1010:42|})]
				[InlineData({|xUnit1010:"2026-03-23T13:57:23Z"|})]
				void ToDateTimeOffset(DateTimeOffset d) { }

				[Theory]
				[InlineData({|xUnit1010:42|})]
				[InlineData({|xUnit1010:"4EBCD32C-A2B8-4600-9E72-3873347E285C"|})]
				void ToGuid(Guid g) { }
			}
			""";

		await Verify_v2_Pre240.VerifyAnalyzerV2(source);
	}

	[Fact]
	public async ValueTask V2_and_V3_NonAOT()
	{
		var source = /* lang=c#-test */ """
			using Xunit;

			public class TestClass {
				[Theory]
				[InlineData("abc")]
				public void ParameterDeclaredImplicitConversion(Implicit i) => Assert.Equal("abc", i.Value);

				public class Implicit {
					public string Value { get; set; }
					public static implicit operator Implicit(string value) => new Implicit() { Value = value };
					public static implicit operator string(Implicit i) => i.Value;
				}

				[Theory]
				[InlineData("abc")]
				public void ParameterDeclaredExplicitConversion(Explicit i) => Assert.Equal("abc", i.Value);

				public class Explicit {
					public string Value { get; set; }
					public static explicit operator Explicit(string value) => new Explicit() { Value = value };
					public static explicit operator string(Explicit i) => i.Value;
				}
			}
			""";

		await Verify.VerifyAnalyzerNonAot(source);
	}

	[Fact]
	public async ValueTask V3_only()
	{
		var source = /* lang=c#-test */ """
			using System.Runtime.InteropServices;
			using Xunit;

			class TestClass {
				[CulturedTheory(new[] { "en-US" })]
				[InlineData("abc", 1, null)]
				public void MethodWithImplicitParameters(string a, int b, object c) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(new object[] { "abc", 1, null })]
				public void MethodWithExplicitParameters(string a, int b, object c) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(data: new object[] { "abc", 1, null })]
				public void MethodWithExplicitParameters_Named(string a, int b, object c) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(new byte[0])]
				public void MethodWithEmptyArray(byte[] input) { }

				// https://github.com/xunit/xunit/issues/3000
				[CulturedTheory(new[] { "en-US" })]
				[InlineData({|CS0182:0.1m|})]
				public void MethodWithDecimalValue(decimal m)
				{ }

				// Optional parameters

				[CulturedTheory(new[] { "en-US" })]
				[InlineData("abc")]
				[InlineData("abc", "def")]
				[InlineData("abc", "def", "ghi")]
				public void MethodWithDefaultParameterValues(string a, string b = "default", string c = null) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData]
				[InlineData("abc")]
				[InlineData("abc", "def")]
				public void MethodWithOptionalAttribute([Optional] string a, [Optional] string b) { }

				// Params parameter

				[CulturedTheory(new[] { "en-US" })]
				[InlineData("abc", "xyz")]
				public void MethodWithImplicitParams(params string[] args) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(new object[] { new string[] { "abc", "xyz" } })]
				public void MethodWithExplicitParams(params string[] args) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData]
				public void MethodWithImplicitEmptyParams(params string[] args) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(new object[] { new string[0] })]
				public void MethodWithExplicitEmptyParams(params string[] args) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(null)]
				public void MethodWithNullParams(params string[] args) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData("abc", "xyz")]
				public void MethodWithMixedNormalAndImplicitParams(string first, params string[] args) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData("abc", new[] { "xyz" })]
				public void MethodWithMixedNormalAndExplicitParams(string first, params string[] args) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData("abc")]
				public void MethodWithMixedNormalAndImplicitEmptyParams(string first, params string[] args) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData("abc", new string[] { })]
				public void MethodWithMixedNormalAndExplicitEmptyParams(string first, params string[] args) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData("abc", null)]
				public void MethodWithMixedNormalAndNullParams(string first, params string[] args) { }

				// Mixed optional and params parameters

				[CulturedTheory(new[] { "en-US" })]
				[InlineData("abc")]
				public void MethodWithOptionalParametersWithDefaultValuesAndParamsParameter(string a, string b = "default", string c = null, params string[] d) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(21.12, {|xUnit1010:new object[] { }|})]
				void IncorrectParamsArrayType_Triggers(double d, params int[] sq) { }
			}

			#nullable enable

			public class NullableTestClass {
				[CulturedTheory(new[] { "en-US" })]
				[InlineData("abc", 1, null)]
				public void MethodWithImplicitParameters(string a, int b, object? c) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(new object[] { "abc", 1, null })]
				public void MethodWithExplicitParameters(string a, int b, object? c) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(data: new object[] { "abc", 1, null })]
				public void MethodWithExplicitParameters_Named(string a, int b, object? c) { }

				// Optional parameters

				[CulturedTheory(new[] { "en-US" })]
				[InlineData("abc")]
				[InlineData("abc", "def")]
				[InlineData("abc", "def", "ghi")]
				public void MethodWithDefaultParameterValues(string a, string b = "default", string? c = null) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData]
				[InlineData("abc")]
				[InlineData("abc", "def")]
				public void MethodWithOptionalAttribute([Optional] string? a, [Optional] string? b) { }

				// Params parameter

				[CulturedTheory(new[] { "en-US" })]
				[InlineData(null)]
				public void MethodWithNullParams(params string[]? args) { }

				[CulturedTheory(new[] { "en-US" })]
				[InlineData("abc", null)]
				public void MethodWithMixedNormalAndNullParams(string first, params string[]? args) { }

				// Mixed optional and params parameters

				[CulturedTheory(new[] { "en-US" })]
				[InlineData("abc")]
				public void MethodWithOptionalParametersWithDefaultValuesAndParamsParameter(string a, string b = "default", string? c = null, params string[] d) { }
			}
			""";

		await Verify.VerifyAnalyzerV3(LanguageVersion.CSharp8, source);
	}

#if NETCOREAPP && ROSLYN_LATEST

	[Fact]
	public async ValueTask V2_and_V3_WithCTRP() // CRTP: Curiously Recurring Template Pattern or T: Interface<T>
	{
		var source = /* lang=c#-test */ """
			using Xunit;
			using System.Numerics;

			public class TestClass {
				[Theory]
				[InlineData(0U)]
				[InlineData(2U)]
				[InlineData(5294967295U)] // ulong value
				[InlineData({|xUnit1010:-1U|})]
				[InlineData({|xUnit1010:2|})]
				[InlineData({|xUnit1010:0|})]
				[InlineData({|xUnit1010:"A"|})]
				public void UnsignedNumberIsAtLeastZero<T>(T number)
					where T : IUnsignedNumber<T> => Assert.False(T.IsNegative(number));
			}
			""";

		await Verify.VerifyAnalyzer(LanguageVersion.CSharp11, source);
	}

	[Fact]
	public async ValueTask V3_only_AOT()
	{
		var source = /* lang=c#-test */ """
				using Xunit;

				public class TestClass {
					[Theory]
					[InlineData({|#0:"abc"|})]
					public void ParameterDeclaredImplicitConversion(Implicit i) => Assert.Equal("abc", i.Value);

					public class Implicit {
						public string Value { get; set; }
						public static implicit operator Implicit(string value) => new Implicit() { Value = value };
						public static implicit operator string(Implicit i) => i.Value;
					}

					[Theory]
					[InlineData({|#1:"abc"|})]
					public void ParameterDeclaredExplicitConversion(Explicit i) => Assert.Equal("abc", i.Value);

					public class Explicit {
						public string Value { get; set; }
						public static explicit operator Explicit(string value) => new Explicit() { Value = value };
						public static explicit operator string(Explicit i) => i.Value;
					}
				}
				""";
		var expected = new[] {
				Verify.Diagnostic("xUnit1010").WithLocation(0).WithArguments("i", "TestClass.Implicit"),
				Verify.Diagnostic("xUnit1010").WithLocation(1).WithArguments("i", "TestClass.Explicit"),
			};

		await Verify.VerifyAnalyzerV3Aot(source, expected);
	}

#endif  // NETCOREAPP && ROSLYN_LATEST

	internal class Analyzer_v2_Pre240 : InlineDataMustMatchTheoryParameters
	{
		protected override XunitContext CreateXunitContext(Compilation compilation) =>
			XunitContext.ForV2(compilation, new Version(2, 3, 999));
	}
}
